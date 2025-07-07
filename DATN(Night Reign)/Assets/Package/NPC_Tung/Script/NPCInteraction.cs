using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _questUI;
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _declineButton;
    [SerializeField] private Button _claimRewardButton;
    [SerializeField] private GameObject _continueButton; // Nút "Tiếp tục"

    [Header("Quest")]
    public QuestManager questManager;
    [SerializeField] private QuestData _questToGive;
    [SerializeField] private QuestData _nextQuestAfterCompletion;

    [Header("Dialogue")]
    public DialogueManager dialogueManager;

    [Header("Default Dialogues")]
    [Tooltip("Keys for default greeting dialogue lines.")]
    [TextArea(3, 5)][SerializeField] private string[] defaultGreetingDialogueKeys;
    [Tooltip("Keys for default speaker names for greeting dialogue. Must match 'defaultGreetingDialogueKeys' length.")]
    [SerializeField] private string[] defaultGreetingSpeakerKeys;
    [Tooltip("Voice clips for default greeting dialogue lines. Must match 'defaultGreetingDialogueKeys' in length.")]
    [SerializeField] private AudioClip[] defaultGreetingVoiceClips;

    [Tooltip("Keys for dialogue lines when quest is already accepted.")]
    [TextArea(3, 5)][SerializeField] private string[] questAlreadyAcceptedDialogueKeys;
    [Tooltip("Keys for speaker names when quest is already accepted. Must match 'questAlreadyAcceptedDialogueKeys' length.")]
    [SerializeField] private string[] questAlreadyAcceptedSpeakerKeys;
    [Tooltip("Voice clips for dialogue lines when quest is already accepted. Must match 'questAlreadyAcceptedDialogueKeys' in length.")]
    [SerializeField] private AudioClip[] questAlreadyAcceptedVoiceClips;

    [Tooltip("Keys for dialogue lines when no relevant quest is available.")]
    [TextArea(3, 3)][SerializeField] private string[] noRelevantQuestDialogueKeys;
    [Tooltip("Keys for speaker names when no relevant quest is available. Must match 'noRelevantQuestDialogueKeys' length.")]
    [SerializeField] private string[] noRelevantQuestSpeakerKeys;
    [Tooltip("Voice clips for dialogue lines when no relevant quest is available. Must match 'noRelevantQuestDialogueKeys' in length.")]
    [SerializeField] private AudioClip[] noRelevantQuestVoiceClips;


    private bool isPlayerInRange = false;

    void Start()
    {
        if (questManager == null)
        {
            Debug.LogError($"QuestManager chưa được gán cho NPCInteraction trên GameObject: {gameObject.name}");
            enabled = false;
            return;
        }
        if (dialogueManager == null)
        {
            Debug.LogError($"DialogueManager chưa được gán cho NPCInteraction trên GameObject: {gameObject.name}");
            enabled = false;
            return;
        }

        _acceptButton?.onClick.AddListener(AcceptCurrentQuest);
        _declineButton?.onClick.AddListener(DeclineQuest);
        _claimRewardButton?.onClick.AddListener(ClaimReward);
        if (_continueButton != null)
        {
            var btn = _continueButton.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(dialogueManager.DisplayNextSentence);
        }

        if (_questUI != null) _questUI.SetActive(false);
        HideAllButtons();
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        var currentQuestInManager = questManager?.GetCurrentQuest();
        var npcID = GetComponent<NPCIdentity>()?.npcID;

        ShowContinueButtonOnly(); // Luôn hiện nút "Tiếp tục" khi bắt đầu

        // Nếu không có nhiệm vụ hoặc NPCID không hợp lệ, hiển thị lời chào mặc định
        if (currentQuestInManager == null || string.IsNullOrEmpty(npcID))
        {
            dialogueManager.StartDialogue(defaultGreetingDialogueKeys, defaultGreetingSpeakerKeys, defaultGreetingVoiceClips, OnDialogueCompleted);
            if (_questUI != null && _questUI.activeSelf) _questUI.SetActive(false);
            return;
        }

        // Xác định nhiệm vụ liên quan đến NPC này (giao nhiệm vụ hoặc mục tiêu)
        QuestData relevantQuestForNPC = null;
        if (_questToGive != null && currentQuestInManager == _questToGive)
            relevantQuestForNPC = _questToGive;
        else if (_nextQuestAfterCompletion != null && currentQuestInManager == _nextQuestAfterCompletion)
            relevantQuestForNPC = _nextQuestAfterCompletion;

        // Nếu không có nhiệm vụ liên quan, hiển thị thoại "không có nhiệm vụ liên quan"
        if (relevantQuestForNPC == null)
        {
            dialogueManager.StartDialogue(noRelevantQuestDialogueKeys, noRelevantQuestSpeakerKeys, noRelevantQuestVoiceClips, OnDialogueCompleted);
            if (_questUI != null && _questUI.activeSelf) _questUI.SetActive(false);
            return;
        }

        // Hiển thị UI nhiệm vụ nếu cần
        questManager.DisplayQuestOfferUI(relevantQuestForNPC);

        bool isQuestAccepted = questManager.IsQuestAccepted();
        bool isQuestCompleted = questManager.IsQuestCompleted();

        if (_questUI != null) _questUI.SetActive(true);

        // Logic thoại dựa trên trạng thái nhiệm vụ và vai trò của NPC
        if (relevantQuestForNPC.giverNPCID == npcID) // Nếu NPC này là người giao nhiệm vụ
        {
            if (isQuestAccepted && !isQuestCompleted) // Nhiệm vụ đã chấp nhận nhưng chưa hoàn thành mục tiêu
            {
                dialogueManager.StartDialogue(questAlreadyAcceptedDialogueKeys, questAlreadyAcceptedSpeakerKeys, questAlreadyAcceptedVoiceClips, OnDialogueCompleted);
            }
            else if (isQuestCompleted) // Nhiệm vụ đã hoàn thành mục tiêu (chưa nhận thưởng)
            {
                string[] keysToShow = relevantQuestForNPC.keydialogueObjectiveMet.Length > 0 ?
                                      relevantQuestForNPC.keydialogueObjectiveMet :
                                      relevantQuestForNPC.keydialogueAfterComplete;

                AudioClip[] voicesToShow = relevantQuestForNPC.GetDialogueVoiceClips(keysToShow);

                // Tên người nói cho thoại nhiệm vụ (có thể lấy từ NPCID hoặc một key chung)
                string[] speakerKeysToShow = new string[keysToShow.Length];
                for (int i = 0; i < speakerKeysToShow.Length; i++) speakerKeysToShow[i] = "NPC_NAME_" + npcID;

                dialogueManager.StartDialogue(keysToShow, speakerKeysToShow, voicesToShow, OnDialogueCompleted);
            }
            else // Nhiệm vụ chưa được chấp nhận
            {
                string[] speakerKeys = new string[relevantQuestForNPC.keydialogueBeforeComplete.Length];
                for (int i = 0; i < speakerKeys.Length; i++) speakerKeys[i] = "NPC_NAME_" + npcID;

                dialogueManager.StartDialogue(relevantQuestForNPC.keydialogueBeforeComplete, speakerKeys, relevantQuestForNPC.voiceBeforeComplete, OnDialogueCompleted);
            }
        }
        else if (relevantQuestForNPC.questType == QuestType.FindNPC && relevantQuestForNPC.targetNPCID == npcID) // Nếu NPC này là mục tiêu của nhiệm vụ FindNPC
        {
            if (isQuestAccepted && !isQuestCompleted)
            {
                questManager.TryCompleteQuestByTalk();
                bool nowCompleted = questManager.IsQuestCompleted();
                if (nowCompleted)
                {
                    string[] keysToShow = relevantQuestForNPC.keydialogueObjectiveMet.Length > 0 ?
                                          relevantQuestForNPC.keydialogueObjectiveMet :
                                          relevantQuestForNPC.keydialogueAfterComplete;

                    AudioClip[] voicesToShow = relevantQuestForNPC.GetDialogueVoiceClips(keysToShow);

                    string[] speakerKeysToShow = new string[keysToShow.Length];
                    for (int i = 0; i < speakerKeysToShow.Length; i++) speakerKeysToShow[i] = "NPC_NAME_" + npcID;

                    dialogueManager.StartDialogue(keysToShow, speakerKeysToShow, voicesToShow, OnDialogueCompleted);
                }
                else
                {
                    dialogueManager.StartDialogue(noRelevantQuestDialogueKeys, noRelevantQuestSpeakerKeys, noRelevantQuestVoiceClips, OnDialogueCompleted);
                    if (_questUI != null && _questUI.activeSelf) _questUI.SetActive(false);
                }
            }
            else if (isQuestCompleted)
            {
                string[] speakerKeys = new string[relevantQuestForNPC.keydialogueAfterComplete.Length];
                for (int i = 0; i < speakerKeys.Length; i++) speakerKeys[i] = "NPC_NAME_" + npcID;

                dialogueManager.StartDialogue(relevantQuestForNPC.keydialogueAfterComplete, speakerKeys, relevantQuestForNPC.voiceAfterComplete, OnDialogueCompleted);
            }
            else
            {
                dialogueManager.StartDialogue(noRelevantQuestDialogueKeys, noRelevantQuestSpeakerKeys, noRelevantQuestVoiceClips, OnDialogueCompleted);
                if (_questUI != null && _questUI.activeSelf) _questUI.SetActive(false);
            }
        }
        else // NPC không liên quan trực tiếp đến nhiệm vụ đang hoạt động
        {
            dialogueManager.StartDialogue(noRelevantQuestDialogueKeys, noRelevantQuestSpeakerKeys, noRelevantQuestVoiceClips, OnDialogueCompleted);
            if (_questUI != null && _questUI.activeSelf) _questUI.SetActive(false);
        }
    }

    private void OnDialogueCompleted()
    {
        var currentQuestInManager = questManager?.GetCurrentQuest();
        if (currentQuestInManager != null)
        {
            questManager.DisplayQuestOfferUI(currentQuestInManager);
        }
        else
        {
            HideAllButtons();
        }
    }

    public void AcceptCurrentQuest()
    {
        var quest = questManager.GetCurrentQuest();
        if (quest == null) return;

        if (questManager.IsQuestAccepted())
        {
            dialogueManager.StartDialogue(questAlreadyAcceptedDialogueKeys, questAlreadyAcceptedSpeakerKeys, questAlreadyAcceptedVoiceClips, OnDialogueCompleted);
            ShowDefaultUI();
            return;
        }

        questManager.AcceptQuest();
        string[] acceptedKeys = new string[] { "DIALOGUE_KEY_QUEST_ACCEPTED" };
        string[] acceptedSpeakerNames = new string[] { "SYSTEM_MESSAGE" };
        AudioClip[] acceptedVoices = new AudioClip[] { null };
        dialogueManager.StartDialogue(acceptedKeys, acceptedSpeakerNames, acceptedVoices, OnDialogueCompleted);
        ShowDefaultUI();
    }

    public void ClaimReward()
    {
        questManager.CompleteQuest();
        string[] claimedKeys = new string[] { "DIALOGUE_KEY_QUEST_CLAIMED_REWARD" };
        string[] claimedSpeakerNames = new string[] { "SYSTEM_MESSAGE" };
        AudioClip[] claimedVoices = new AudioClip[] { null };
        dialogueManager.StartDialogue(claimedKeys, claimedSpeakerNames, claimedVoices, OnDialogueCompleted);
        HideAllButtons();
    }

    public void DeclineQuest()
    {
        questManager.DeclineQuest();
        string[] declinedKeys = new string[] { "DIALOGUE_KEY_QUEST_DECLINED" };
        string[] declinedSpeakerNames = new string[] { "SYSTEM_MESSAGE" };
        AudioClip[] declinedVoices = new AudioClip[] { null };
        dialogueManager.StartDialogue(declinedKeys, declinedSpeakerNames, declinedVoices, OnDialogueCompleted);
        HideAllButtons();
    }

    private void ShowDefaultUI()
    {
        _acceptButton?.gameObject.SetActive(true);
        _declineButton?.gameObject.SetActive(true);
        _claimRewardButton?.gameObject.SetActive(false);
        _continueButton?.gameObject.SetActive(false);
    }

    private void ShowClaimRewardUI()
    {
        _acceptButton?.gameObject.SetActive(false);
        _declineButton?.gameObject.SetActive(false);
        _claimRewardButton?.gameObject.SetActive(true);
        _continueButton?.gameObject.SetActive(false);
    }

    private void ShowContinueButtonOnly()
    {
        _acceptButton?.gameObject.SetActive(false);
        _declineButton?.gameObject.SetActive(false);
        _claimRewardButton?.gameObject.SetActive(false);
        _continueButton?.gameObject.SetActive(true);
    }

    private void HideAllButtons()
    {
        _acceptButton?.gameObject.SetActive(false);
        _declineButton?.gameObject.SetActive(false);
        _claimRewardButton?.gameObject.SetActive(false);
        _continueButton?.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (_questUI != null) _questUI.SetActive(false);
            dialogueManager?.EndDialogue();
        }
    }
}
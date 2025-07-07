using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System;

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
    [TextArea(3, 5)][SerializeField] private string[] defaultGreetingDialogueKeys; // Localization Keys
    [TextArea(3, 5)][SerializeField] private string[] questAlreadyAcceptedDialogueKeys; // Localization Keys
    [TextArea(3, 3)][SerializeField] private string[] noRelevantQuestDialogueKeys; // Localization Keys

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
                btn.onClick.AddListener(dialogueManager.NextLine);
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

        if (currentQuestInManager == null || string.IsNullOrEmpty(npcID))
        {
            dialogueManager.StartDialogue(defaultGreetingDialogueKeys, OnDialogueCompleted);
            if (_questUI != null && _questUI.activeSelf) _questUI.SetActive(false);
            return;
        }

        QuestData relevantQuestForNPC = null;
        if (_questToGive != null && currentQuestInManager == _questToGive)
            relevantQuestForNPC = _questToGive;
        else if (_nextQuestAfterCompletion != null && currentQuestInManager == _nextQuestAfterCompletion)
            relevantQuestForNPC = _nextQuestAfterCompletion;

        if (relevantQuestForNPC == null)
        {
            dialogueManager.StartDialogue(noRelevantQuestDialogueKeys, OnDialogueCompleted);
            if (_questUI != null && _questUI.activeSelf) _questUI.SetActive(false);
            return;
        }

        questManager.DisplayQuestOfferUI(relevantQuestForNPC);

        bool isQuestAccepted = questManager.IsQuestAccepted();
        bool isQuestCompleted = questManager.IsQuestCompleted();

        if (_questUI != null) _questUI.SetActive(true);

        if (relevantQuestForNPC.giverNPCID == npcID)
        {
            if (isQuestAccepted && !isQuestCompleted)
            {
                dialogueManager.StartDialogue(questAlreadyAcceptedDialogueKeys, OnDialogueCompleted);
            }
            else if (isQuestCompleted)
            {
                string[] keys = relevantQuestForNPC.keydialogueObjectiveMet.Length > 0 ?
                                relevantQuestForNPC.keydialogueObjectiveMet :
                                relevantQuestForNPC.keydialogueAfterComplete;
                dialogueManager.StartDialogue(relevantQuestForNPC.GetDialogueKeys(keys), OnDialogueCompleted);
            }
            else
            {
                dialogueManager.StartDialogue(relevantQuestForNPC.GetDialogueKeys(relevantQuestForNPC.keydialogueBeforeComplete), OnDialogueCompleted);
            }
        }
        else if (relevantQuestForNPC.questType == QuestType.FindNPC && relevantQuestForNPC.targetNPCID == npcID)
        {
            if (isQuestAccepted && !isQuestCompleted)
            {
                questManager.TryCompleteQuestByTalk();
                bool nowCompleted = questManager.IsQuestCompleted();
                if (nowCompleted)
                {
                    string[] keys = relevantQuestForNPC.keydialogueObjectiveMet.Length > 0 ?
                                    relevantQuestForNPC.keydialogueObjectiveMet :
                                    relevantQuestForNPC.keydialogueAfterComplete;
                    dialogueManager.StartDialogue(relevantQuestForNPC.GetDialogueKeys(keys), OnDialogueCompleted);
                }
                else
                {
                    dialogueManager.StartDialogue(noRelevantQuestDialogueKeys, OnDialogueCompleted);
                    if (_questUI != null && _questUI.activeSelf) _questUI.SetActive(false);
                }
            }
            else if (isQuestCompleted)
            {
                dialogueManager.StartDialogue(relevantQuestForNPC.GetDialogueKeys(relevantQuestForNPC.keydialogueAfterComplete), OnDialogueCompleted);
            }
            else
            {
                dialogueManager.StartDialogue(noRelevantQuestDialogueKeys, OnDialogueCompleted);
                if (_questUI != null && _questUI.activeSelf) _questUI.SetActive(false);
            }
        }
        else
        {
            dialogueManager.StartDialogue(noRelevantQuestDialogueKeys, OnDialogueCompleted);
            if (_questUI != null && _questUI.activeSelf) _questUI.SetActive(false);
        }
    }

    private void OnDialogueCompleted(string response)
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
            dialogueManager.StartDialogue(questAlreadyAcceptedDialogueKeys, OnDialogueCompleted);
            ShowDefaultUI();
            return;
        }

        questManager.AcceptQuest();
        dialogueManager.StartDialogue(new string[] { "DIALOGUE_KEY_QUEST_ACCEPTED" }, OnDialogueCompleted);
        ShowDefaultUI();
    }

    public void ClaimReward()
    {
        questManager.CompleteQuest();
        dialogueManager.StartDialogue(new string[] { "DIALOGUE_KEY_QUEST_CLAIMED_REWARD" }, OnDialogueCompleted);
        HideAllButtons();
    }

    public void DeclineQuest()
    {
        questManager.DeclineQuest();
        dialogueManager.StartDialogue(new string[] { "DIALOGUE_KEY_QUEST_DECLINED" }, OnDialogueCompleted);
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

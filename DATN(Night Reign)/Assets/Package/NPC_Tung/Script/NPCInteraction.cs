using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _questUI;
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _declineButton;
    [SerializeField] private Button _claimRewardButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private TextMeshProUGUI panelpress;

    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private string npcID; // ID của NPC này, trùng với quest.giverNPCID

    [Header("Default Dialogues")]
    [TextArea(3, 5)][SerializeField] private string[] defaultGreetingDialogueKeys;
    [SerializeField] private AudioClip[] defaultGreetingVoiceClipsEN;
    [SerializeField] private AudioClip[] defaultGreetingVoiceClipsVI;

    [TextArea(3, 5)][SerializeField] private string[] noRelevantQuestDialogueKeys;
    [SerializeField] private AudioClip[] noRelevantQuestVoiceClipsEN;
    [SerializeField] private AudioClip[] noRelevantQuestVoiceClipsVI;

    private bool isPlayerInRange = false;
    private QuestManager.CurrentQuestStatus lastCompletedQuestStatus = null;
    private bool isWaitingForContinueDialogue = false;

    private void Start()
    {
        if (!questManager || !dialogueManager)
        {
            Debug.LogError("Assign QuestManager and DialogueManager in inspector!");
            enabled = false;
            return;
        }

        _acceptButton.onClick.AddListener(OnAcceptButtonPressed);
        _declineButton.onClick.AddListener(OnDeclineButtonPressed);
        _claimRewardButton.onClick.AddListener(OnClaimRewardButtonPressed);
        _continueButton.onClick.AddListener(OnContinueButtonPressed);

        _questUI.SetActive(false);
        HideAllActionButtons();
        panelpress.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.C) && !isWaitingForContinueDialogue)
        {
            HandleInteraction();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            questManager.ReportKill();
        }
    }

    private void HandleInteraction()
    {
        _questUI.SetActive(true);
        MouseManager.Instance.ShowCursorAndDisableInput();
        panelpress.gameObject.SetActive(false);

        if (isWaitingForContinueDialogue && lastCompletedQuestStatus != null)
        {
            ContinueAfterCompleteDialogue();
            return;
        }

        List<QuestData> relatedQuests = new List<QuestData>();
        foreach (var quest in questManager.questDatabase.quests)
        {
            if (quest.giverNPCID == npcID)
                relatedQuests.Add(quest);
        }

        QuestData questToOffer = null;
        QuestData questInProgress = null;
        QuestData questCompletedAwaitingClaim = null;
        QuestData lastCompletedQuest = null;

        foreach (var quest in relatedQuests)
        {
            var status = questManager.GetQuestStatus(quest);

            if (status == null)
            {
                if (questManager.IsQuestUnlocked(quest))
                {
                    questToOffer = quest;
                    break;
                }
            }
            else
            {
                if (status.isObjectiveMet && !status.isCompleted)
                {
                    questCompletedAwaitingClaim = quest;
                    break;
                }
                else if (!status.isObjectiveMet)
                {
                    questInProgress = quest;
                }
                else if (status.isCompleted)
                {
                    lastCompletedQuest = quest;
                }
            }
        }

        // Logic mới theo yêu cầu
        if (questCompletedAwaitingClaim != null)
        {
            ShowClaimRewardUI();
            // Chỉ nói câu đầu tiên của AfterComplete khi chưa nhận thưởng
            string[] firstLineOnly = new string[] { questCompletedAwaitingClaim.keydialogueAfterComplete[0] };
            StartQuestDialogue(questCompletedAwaitingClaim, firstLineOnly,
                              questCompletedAwaitingClaim.voiceAfterComplete_EN.Take(1).ToArray(),
                              questCompletedAwaitingClaim.voiceAfterComplete_VI.Take(1).ToArray());
            return;
        }
        else if (lastCompletedQuest != null)
        {
            // Nếu đã nhận thưởng, nói từ câu thứ 2 đến hết AfterComplete
            if (lastCompletedQuest.keydialogueAfterComplete.Length > 1)
            {
                string[] remainingLines = new string[lastCompletedQuest.keydialogueAfterComplete.Length - 1];
                Array.Copy(lastCompletedQuest.keydialogueAfterComplete, 1, remainingLines, 0, remainingLines.Length);

                AudioClip[] remainingClipsEN = new AudioClip[lastCompletedQuest.voiceAfterComplete_EN.Length - 1];
                Array.Copy(lastCompletedQuest.voiceAfterComplete_EN, 1, remainingClipsEN, 0, remainingClipsEN.Length);

                AudioClip[] remainingClipsVI = new AudioClip[lastCompletedQuest.voiceAfterComplete_VI.Length - 1];
                Array.Copy(lastCompletedQuest.voiceAfterComplete_VI, 1, remainingClipsVI, 0, remainingClipsVI.Length);

                StartQuestDialogue(lastCompletedQuest, remainingLines, remainingClipsEN, remainingClipsVI);

                // Kiểm tra quest tiếp theo có phải FindNPC không
                if (questManager.IsNextQuestFindNPC())
                {
                    // Tự động nhận quest FindNPC tiếp theo
                    questManager.SetCurrentQuestIndex(questManager._currentQuestIndex + 1);
                    questManager.AcceptQuest();
                }
            }
            return;
        }
        else if (questToOffer != null)
        {
            ShowDefaultUI();
            StartQuestDialogue(questToOffer, QuestDialogueType.BeforeComplete);
            return;
        }
        else if (questInProgress != null)
        {
            HideAllActionButtons();
            StartQuestDialogue(questInProgress, QuestDialogueType.InProgress);
            return;
        }
        else
        {
            dialogueManager.StartDialogue(
                defaultGreetingDialogueKeys,
                defaultGreetingVoiceClipsEN,
                defaultGreetingVoiceClipsVI,
                OnDialogueCompleted);
        }
    }

    // Thêm phương thức mới để bắt đầu thoại với mảng keys và clips tùy chỉnh
    private void StartQuestDialogue(QuestData quest, string[] customKeys, AudioClip[] customClipsEN, AudioClip[] customClipsVI)
    {
        dialogueManager.StartDialogue(customKeys, customClipsEN, customClipsVI, OnDialogueCompleted);
    }

    private void StartQuestDialogue(QuestData quest, QuestDialogueType dialogueType)
    {
        var keys = quest.GetDialogueKeys(dialogueType);
        var clipsEN = quest.GetDialogueVoiceClips(dialogueType);
        var clipsVI = quest.GetDialogueVoiceClips(dialogueType);

        dialogueManager.StartDialogue(keys, clipsEN, clipsVI, OnDialogueCompleted);
    }

    private void OnAcceptButtonPressed()
    {
        for (int i = 0; i < questManager.questDatabase.quests.Length; i++)
        {
            var quest = questManager.questDatabase.quests[i];
            if (quest.giverNPCID == npcID && !questManager.IsQuestAccepted(quest) && questManager.IsQuestUnlocked(quest))
            {
                questManager.SetCurrentQuestIndex(i);
                questManager.AcceptQuest();
                break;
            }
        }
        _questUI.SetActive(false);
        HideAllActionButtons();
        MouseManager.Instance.HideCursorAndEnableInput();
    }

    private void OnDeclineButtonPressed()
    {
        _questUI.SetActive(false);
        HideAllActionButtons();
        MouseManager.Instance.HideCursorAndEnableInput();
    }

    private void OnClaimRewardButtonPressed()
    {
        for (int i = 0; i < questManager.questDatabase.quests.Length; i++)
        {
            var quest = questManager.questDatabase.quests[i];
            if (quest.giverNPCID == npcID && questManager.IsQuestAccepted(quest) && questManager.IsQuestObjectiveMet(quest) && !questManager.IsQuestTrulyCompleted(quest))
            {
                questManager.SetCurrentQuestIndex(i);
                questManager.ClaimReward();
                lastCompletedQuestStatus = questManager.GetQuestStatus(quest);
                isWaitingForContinueDialogue = true;

                // Không cần kiểm tra quest tiếp theo ở đây nữa vì đã xử lý trong CompleteQuest
                break;
            }
        }
        HideAllActionButtons();
    }

    private void OnContinueButtonPressed()
    {
        dialogueManager.DisplayNextSentence();
    }

    private void ContinueAfterCompleteDialogue()
    {
        // Xử lý tiếp đoạn thoại sau khi claim reward
        isWaitingForContinueDialogue = false;
        lastCompletedQuestStatus = null;
        _questUI.SetActive(false);
        MouseManager.Instance.HideCursorAndEnableInput();
    }

    private void OnDialogueCompleted()
    {
        if (!isWaitingForContinueDialogue)
        {
            _questUI.SetActive(false);
            HideAllActionButtons();
            MouseManager.Instance.HideCursorAndEnableInput();
        }
    }

    private void ShowDefaultUI()
    {
        _acceptButton.gameObject.SetActive(true);
        _declineButton.gameObject.SetActive(true);
        _claimRewardButton.gameObject.SetActive(false);
    }

    private void ShowClaimRewardUI()
    {
        _acceptButton.gameObject.SetActive(false);
        _declineButton.gameObject.SetActive(false);
        _claimRewardButton.gameObject.SetActive(true);
    }

    private void HideAllActionButtons()
    {
        _acceptButton.gameObject.SetActive(false);
        _declineButton.gameObject.SetActive(false);
        _claimRewardButton.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            QuestData currentQuest = questManager.GetCurrentQuest();

            if (currentQuest != null)
            {
                bool foundTarget = currentQuest.questType == QuestType.FindNPC &&
                                   Vector3.Distance(currentQuest.questLocation, transform.position) < 2f;

                bool isGiver = currentQuest.giverNPCID == npcID;

                if (foundTarget || isGiver)
                {
                    questManager.TryCompleteQuestByTalk();
                }
            }

            // Hiện panel bất kể nhiệm vụ có hay không
            panelpress.gameObject?.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            _questUI.SetActive(false);
            panelpress.gameObject?.SetActive(false);
            HideAllActionButtons();
            dialogueManager.EndDialogue();
            MouseManager.Instance.HideCursorAndEnableInput();
        }
    }
}

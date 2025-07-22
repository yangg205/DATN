using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject questUI;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;
    [SerializeField] private Button claimRewardButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI panelPressText;

    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private string npcID;

    [Header("Default Dialogues")]
    [TextArea(3, 5)] public string[] defaultGreetingDialogueKeys;
    public AudioClip[] defaultGreetingVoiceClipsEN;
    public AudioClip[] defaultGreetingVoiceClipsVI;

    [TextArea(3, 5)] public string[] noRelevantQuestDialogueKeys;
    public AudioClip[] noRelevantQuestVoiceClipsEN;
    public AudioClip[] noRelevantQuestVoiceClipsVI;

    private bool isPlayerInRange = false;
    private bool isWaitingForContinueDialogue = false;

    private void Start()
    {
        acceptButton.onClick.AddListener(OnAcceptButtonPressed);
        declineButton.onClick.AddListener(OnDeclineButtonPressed);
        claimRewardButton.onClick.AddListener(OnClaimRewardButtonPressed);
        continueButton.onClick.AddListener(OnContinueButtonPressed);

        questUI.SetActive(false);
        panelPressText.gameObject.SetActive(false);
        HideAllActionButtons();
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.C) && !isWaitingForContinueDialogue)
        {
            HandleInteraction();
            MouseManager.Instance?.ShowCursorAndDisableInput();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            questManager.ReportKill();
        }
    }

    private void HandleInteraction()
    {
        questUI.SetActive(true);
        panelPressText.gameObject.SetActive(false);

        // Get all quests related to this NPC
        var relatedQuests = questManager.questDatabase.quests
            .Where(q => q.giverNPCID == npcID)
            .ToList();

        // Find quests in different states
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

        // Handle different cases
        if (questCompletedAwaitingClaim != null)
        {
            // Case 1: Quest ready to claim - show first line of AfterComplete
            ShowQuestUI(); // Hiển thị quest UI
            ShowClaimRewardUI();
            string[] firstLine = new string[] { questCompletedAwaitingClaim.keydialogueAfterComplete[0] };
            AudioClip[] firstClipEN = new AudioClip[] { questCompletedAwaitingClaim.voiceAfterComplete_EN[0] };
            AudioClip[] firstClipVI = new AudioClip[] { questCompletedAwaitingClaim.voiceAfterComplete_VI[0] };

            StartCustomDialogue(questCompletedAwaitingClaim, firstLine, firstClipEN, firstClipVI);
        }
        else if (lastCompletedQuest != null && lastCompletedQuest.keydialogueAfterComplete.Length > 1)
        {
            // Case 2: Quest already claimed - show remaining AfterComplete lines
            string[] remainingLines = lastCompletedQuest.keydialogueAfterComplete.Skip(1).ToArray();
            AudioClip[] remainingClipsEN = lastCompletedQuest.voiceAfterComplete_EN.Skip(1).ToArray();
            AudioClip[] remainingClipsVI = lastCompletedQuest.voiceAfterComplete_VI.Skip(1).ToArray();

            StartCustomDialogue(lastCompletedQuest, remainingLines, remainingClipsEN, remainingClipsVI);
        }
        else if (questToOffer != null)
        {
            // Case 3: New quest to offer
            ShowAcceptDeclineUI();
            StartQuestDialogue(questToOffer, QuestDialogueType.BeforeComplete);
        }
        else if (questInProgress != null)
        {
            // Case 4: Quest in progress
            HideAllActionButtons();
            StartQuestDialogue(questInProgress, QuestDialogueType.ObjectiveMet); // Sử dụng ObjectiveMet thay vì InProgress
        }
        else
        {
            // Case 5: No relevant quest - default dialogue
            HideAllActionButtons();
            dialogueManager.StartDialogue(
                defaultGreetingDialogueKeys,
                defaultGreetingVoiceClipsEN,
                defaultGreetingVoiceClipsVI,
                OnDialogueCompleted);
        }
    }

    private void StartQuestDialogue(QuestData quest, QuestDialogueType dialogueType)
    {
        var keys = quest.GetDialogueKeys(dialogueType);
        var clips = quest.GetDialogueVoiceClips(dialogueType);

        dialogueManager.StartDialogue(
            keys,
            clips,
            clips,
            OnDialogueCompleted);
    }

    private void StartCustomDialogue(QuestData quest, string[] keys, AudioClip[] clipsEN, AudioClip[] clipsVI)
    {
        dialogueManager.StartDialogue(
            keys,
            clipsEN,
            clipsVI,
            OnDialogueCompleted);
    }

    private void OnAcceptButtonPressed()
    {
        var quest = questManager.GetCurrentQuest();
        if (quest != null && quest.giverNPCID == npcID)
        {
            questManager.AcceptQuest();
        }
        HideQuestUI();
    }

    private void OnDeclineButtonPressed()
    {
        HideQuestUI();
    }

    private void OnClaimRewardButtonPressed()
    {
        var quest = questManager.GetCurrentQuest();
        if (quest != null && quest.giverNPCID == npcID)
        {
            questManager.ClaimReward();
            isWaitingForContinueDialogue = true;
        }
        HideAllActionButtons();
    }

    private void OnContinueButtonPressed()
    {
        dialogueManager.DisplayNextSentence();
    }

    private void OnDialogueCompleted()
    {
        if (isWaitingForContinueDialogue)
        {
            isWaitingForContinueDialogue = false;
        }
        HideQuestUI();
    }

    private void ShowAcceptDeclineUI()
    {
        acceptButton.gameObject.SetActive(true);
        declineButton.gameObject.SetActive(true);
        claimRewardButton.gameObject.SetActive(false);
    }

    private void ShowClaimRewardUI()
    {
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        claimRewardButton.gameObject.SetActive(true);
    }

    private void HideAllActionButtons()
    {
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        claimRewardButton.gameObject.SetActive(false);
    }

    private void ShowQuestUI()
    {
        questUI.SetActive(true);
        MouseManager.Instance?.ShowCursorAndDisableInput();
    }

    private void HideQuestUI()
    {
        questUI.SetActive(false);
        MouseManager.Instance?.HideCursorAndEnableInput();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            panelPressText.gameObject.SetActive(true);

            // Check if this NPC is a quest target
            var currentQuest = questManager.GetCurrentQuest();
            if (currentQuest != null)
            {
                bool isTargetNPC = currentQuest.questType == QuestType.FindNPC &&
                                 currentQuest.targetNPCID == npcID;

                bool isQuestGiver = currentQuest.giverNPCID == npcID;

                if (isTargetNPC || isQuestGiver)
                {
                    questManager.TryCompleteQuestByTalk();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            panelPressText.gameObject.SetActive(false);
            HideQuestUI();
            dialogueManager.EndDialogue();
            MouseManager.Instance?.HideCursorAndEnableInput();
        }
    }

}
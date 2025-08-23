using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;

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
    private QuestData currentQuestData;
    private QuestDialogueType currentDialogueType;
    private static NPCInteraction activeNPC;
    private void Start()
    {

        InitializeUIButtons();
        InitializeUIElements();
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChangedHandler;
        Debug.Log($"[NPCInteraction] Initialized for NPC {npcID}");
        questManager.ResetAllQuests();//không muốn reset thì cmd nó lại
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChangedHandler;
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonPressed);
        }
        if (activeNPC == this) activeNPC = null;
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.C) && dialogueManager != null && !dialogueManager.IsDialogueActive)
        {
            Debug.Log($"[NPCInteraction] Player pressed C to interact with NPC {npcID}, _isQuestAccepted={questManager?.uiManager?._isQuestAccepted}");
            HandleInteraction();
        }
        if (Input.GetKeyDown(KeyCode.Q) && questManager != null)
        {
            Debug.Log($"[NPCInteraction] Player pressed Q to report kill for NPC {npcID}");
            questManager.ReportKill();
            
            SimpleInventory.Instance?.AddItem("Shadow Fangs", 5);
            SimpleInventory.Instance?.AddItem("Snow Crystals", 5);
            questManager.CheckItemCollectionProgress();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            questManager.OnBossDefeated(currentQuestData);
        }
    }

    private void InitializeUIButtons()
    {
        if (acceptButton != null) acceptButton.onClick.AddListener(OnAcceptButtonPressed);
        else Debug.LogWarning($"[NPCInteraction] acceptButton not assigned for NPC {npcID}");

        if (declineButton != null) declineButton.onClick.AddListener(OnDeclineButtonPressed);
        else Debug.LogWarning($"[NPCInteraction] declineButton not assigned for NPC {npcID}");

        if (claimRewardButton != null) claimRewardButton.onClick.AddListener(OnClaimRewardButtonPressed);
        else Debug.LogWarning($"[NPCInteraction] claimRewardButton not assigned for NPC {npcID}");

        if (continueButton == null) Debug.LogWarning($"[NPCInteraction] continueButton not assigned for NPC {npcID}");
    }

    private void InitializeUIElements()
    {
        if (questUI != null) questUI.SetActive(false);
        else Debug.LogWarning($"[NPCInteraction] questUI not assigned for NPC {npcID}");

        if (panelPressText != null) panelPressText.gameObject.SetActive(false);
        else Debug.LogWarning($"[NPCInteraction] panelPressText not assigned for NPC {npcID}");

        HideAllActionButtons();
    }

    private void HandleInteraction()
    {
        if (questUI == null)
        {
            Debug.LogWarning($"[NPCInteraction] questUI not assigned for NPC {npcID}");
            return;
        }

        ShowQuestUI();
        UpdateActiveNPC();

        var currentQuest = questManager?.GetCurrentQuest();
        var relatedQuests = GetRelatedQuests();
        Debug.Log($"[NPCInteraction] Found {relatedQuests.Count} quests for NPC {npcID}: {string.Join(", ", relatedQuests.Select(q => q.questName))}");

        bool isTargetNPC = currentQuest != null && currentQuest.questType == QuestType.FindNPC && currentQuest.targetNPCID == npcID;
        bool isQuestInProgress = currentQuest != null && questManager != null && questManager.IsQuestAccepted(currentQuest) && !questManager.IsQuestCompleted(currentQuest);
        bool isQuestObjectiveMet = currentQuest != null && questManager != null && questManager.IsQuestObjectiveMet(currentQuest) && !questManager.IsQuestCompleted(currentQuest);
        bool isGiverNPC = currentQuest != null && currentQuest.giverNPCID == npcID && !questManager.IsQuestCompleted(currentQuest);

        Debug.Log($"[NPCInteraction] Current quest: {(currentQuest != null ? currentQuest.questName : "null")}, isTargetNPC={isTargetNPC}, isQuestInProgress={isQuestInProgress}, isQuestObjectiveMet={isQuestObjectiveMet}, isGiverNPC={isGiverNPC}");

        if (isTargetNPC && isQuestInProgress)
        {
            HandleFindNPCQuest(currentQuest);
        }
        else if (isGiverNPC && isQuestObjectiveMet)
        {
            HandleObjectiveMet(currentQuest);
        }
        else if (isGiverNPC && isQuestInProgress)
        {
            HandleQuestInProgress(currentQuest);
        }
        else if (isGiverNPC && !isQuestInProgress && !isQuestObjectiveMet)
        {
            HandleNewQuestOffer(currentQuest);
        }
        else if (relatedQuests.Any())
        {
            HandleRelatedQuestOffer(relatedQuests);
        }
        else
        {
            ShowNoRelevantQuestDialogue();
        }

    }

    private List<QuestData> GetRelatedQuests()
    {
        return questManager != null && questManager.questDatabase != null && questManager.questDatabase.quests != null
            ? questManager.questDatabase.quests.Where(q => q != null && q.giverNPCID == npcID && questManager.IsQuestUnlocked(q) && !q.isQuestCompleted).ToList()
            : new List<QuestData>();
    }

    private void HandleFindNPCQuest(QuestData quest)
    {
        Debug.Log($"[NPCInteraction] Completing FindNPC quest {quest.questName} for target NPC {npcID}");
        questManager?.OnInteractWithNPC(npcID);

        // Check if the NPC is also a giver for new quests
        var relatedQuests = GetRelatedQuests();
        if (relatedQuests.Any())
        {
            var questToOffer = relatedQuests.FirstOrDefault(q => questManager != null && questManager.IsQuestUnlocked(q));
            if (questToOffer != null)
            {
                OfferQuest(questToOffer);
            }
            else
            {
                ShowNoRelevantQuestDialogue();
            }
        }
        else
        {
            ShowNoRelevantQuestDialogue();
        }
    }

    private void HandleObjectiveMet(QuestData quest)
    {
        Debug.Log($"[NPCInteraction] Starting ObjectiveMet dialogue for quest {quest.questName} (awaiting reward)");
        ShowQuestUI();
        ShowClaimRewardUI();
        currentQuestData = quest;
        currentDialogueType = QuestDialogueType.ObjectiveMet;
        StartQuestDialogue(currentQuestData, currentDialogueType);
    }

    private void HandleQuestInProgress(QuestData quest)
    {
        Debug.Log($"[NPCInteraction] Starting ObjectiveMet dialogue for quest {quest.questName} (in progress)");
        ShowQuestUI();
        ShowContinueUI();
        currentQuestData = quest;
        currentDialogueType = QuestDialogueType.ObjectiveMet;
        StartQuestDialogue(currentQuestData, currentDialogueType);
    }

    private void HandleNewQuestOffer(QuestData quest)
    {
        Debug.Log($"[NPCInteraction] Starting BeforeComplete dialogue for unaccepted quest {quest.questName}");
        ShowQuestUI();
        ShowAcceptDeclineUI();
        currentQuestData = quest;
        currentDialogueType = QuestDialogueType.BeforeComplete;
        StartQuestDialogue(currentQuestData, currentDialogueType);
    }

    private void HandleRelatedQuestOffer(List<QuestData> relatedQuests)
    {
        var questToOffer = relatedQuests.FirstOrDefault(q => questManager != null && questManager.IsQuestUnlocked(q));
        if (questToOffer != null)
        {
            OfferQuest(questToOffer);
        }
        else
        {
            ShowNoRelevantQuestDialogue();
        }
    }

    private void OfferQuest(QuestData quest)
    {
        var beforeCompleteKeys = quest.GetDialogueKeys(QuestDialogueType.BeforeComplete);
        if (beforeCompleteKeys == null || beforeCompleteKeys.Length == 0)
        {
            Debug.Log($"[NPCInteraction] No BeforeComplete dialogue for {quest.questName}, auto-accepting");
            questManager?.AcceptQuest(quest);
            HideQuestUI();
            ResetActiveNPC();
        }
        else
        {
            Debug.Log($"[NPCInteraction] Starting BeforeComplete dialogue for quest {quest.questName}");
            ShowQuestUI();
            ShowAcceptDeclineUI();
            currentQuestData = quest;
            currentDialogueType = QuestDialogueType.BeforeComplete;
            StartQuestDialogue(currentQuestData, currentDialogueType);
        }
    }

    private void ShowNoRelevantQuestDialogue()
    {
        HideAllActionButtons();
        ShowContinueUI();
        currentQuestData = null;
        currentDialogueType = QuestDialogueType.BeforeComplete;

        var keys = defaultGreetingDialogueKeys?.Length > 0 ? defaultGreetingDialogueKeys : noRelevantQuestDialogueKeys ?? Array.Empty<string>();
        var clipsEN = defaultGreetingVoiceClipsEN?.Length > 0 ? defaultGreetingVoiceClipsEN : noRelevantQuestVoiceClipsEN ?? Array.Empty<AudioClip>();
        var clipsVI = defaultGreetingVoiceClipsVI?.Length > 0 ? defaultGreetingVoiceClipsVI : noRelevantQuestVoiceClipsVI ?? Array.Empty<AudioClip>();

        if (keys.Length == 0)
        {
            Debug.LogWarning($"[NPCInteraction] No valid dialogue keys for NPC {npcID}");
            dialogueManager?.EndDialogue();
            HideQuestUI();
            ResetActiveNPC();
            return;
        }

        Debug.Log($"[NPCInteraction] Starting default dialogue with {keys.Length} lines: {string.Join(", ", keys)}");
        dialogueManager?.StartQuestDialogue(null, QuestDialogueType.BeforeComplete, OnDialogueCompleted, keys, clipsEN, clipsVI);
    }

    private void OnLanguageChangedHandler(Locale newLocale)
    {
        if (isPlayerInRange && questUI != null && questUI.activeSelf && dialogueManager != null && dialogueManager.IsDialogueActive && activeNPC == this)
        {
            Debug.Log($"[NPCInteraction] Language changed to {newLocale.Identifier.Code}, refreshing dialogue for NPC {npcID}");
            if (currentQuestData != null)
            {
                StartQuestDialogue(currentQuestData, currentDialogueType);
            }
            else
            {
                ShowNoRelevantQuestDialogue();
            }
        }
    }

    private void StartQuestDialogue(QuestData quest, QuestDialogueType dialogueType, Action onDialogueCompleted = null)
    {
        var keys = quest != null ? quest.GetDialogueKeys(dialogueType) : Array.Empty<string>();
        var clipsEN = quest != null ? dialogueType switch
        {
            QuestDialogueType.BeforeComplete => quest.voiceBeforeComplete_EN ?? Array.Empty<AudioClip>(),
            QuestDialogueType.AfterComplete => quest.voiceAfterComplete_EN ?? Array.Empty<AudioClip>(),
            QuestDialogueType.ObjectiveMet => quest.voiceObjectiveMet_EN ?? Array.Empty<AudioClip>(),
            _ => Array.Empty<AudioClip>()
        } : Array.Empty<AudioClip>();
        var clipsVI = quest != null ? dialogueType switch
        {
            QuestDialogueType.BeforeComplete => quest.voiceBeforeComplete_VI ?? Array.Empty<AudioClip>(),
            QuestDialogueType.AfterComplete => quest.voiceAfterComplete_VI ?? Array.Empty<AudioClip>(),
            QuestDialogueType.ObjectiveMet => quest.voiceObjectiveMet_VI ?? Array.Empty<AudioClip>(),
            _ => Array.Empty<AudioClip>()
        } : Array.Empty<AudioClip>();

        if (keys.Length == 0)
        {
            Debug.LogWarning($"[NPCInteraction] No dialogue keys found for {dialogueType} in QuestData: {(quest != null ? quest.questName : "null")} for NPC {npcID}");
            dialogueManager?.EndDialogue();
            onDialogueCompleted?.Invoke();
            HideQuestUI();
            ResetActiveNPC();
            return;
        }

        Debug.Log($"[NPCInteraction] Starting QuestDialogue for {dialogueType} with {keys.Length} lines: {string.Join(", ", keys)} for NPC {npcID}");
        dialogueManager?.StartQuestDialogue(quest, dialogueType, onDialogueCompleted, keys, clipsEN, clipsVI);
    }

    private void OnAcceptButtonPressed()
    {
        if (currentQuestData == null)
        {
            Debug.LogWarning($"[NPCInteraction] No current quest data to accept for NPC {npcID}");
            return;
        }
        Debug.Log($"[NPCInteraction] Accepting quest {currentQuestData.questName} for NPC {npcID}");
        questManager?.AcceptQuest(currentQuestData);
        StartCoroutine(UpdateUIAfterAcceptCoroutine());
        HideQuestUI();
        ResetActiveNPC();
    }

    private IEnumerator UpdateUIAfterAcceptCoroutine()
    {
        if (questManager?.uiManager != null)
        {
            yield return StartCoroutine(questManager.uiManager.RefreshCombinedQuestInfoUICoroutine());
        }
    }

    private void OnDeclineButtonPressed()
    {
        if (currentQuestData == null)
        {
            Debug.LogWarning($"[NPCInteraction] No current quest data to decline for NPC {npcID}");
            return;
        }
        Debug.Log($"[NPCInteraction] Declining quest {currentQuestData.questName} for NPC {npcID}");
        questManager?.DeclineQuest(currentQuestData);
        HideQuestUI();
        ResetActiveNPC();
    }

    private void OnClaimRewardButtonPressed()
    {
        if (currentQuestData == null)
        {
            Debug.LogWarning($"[NPCInteraction] No current quest data to claim reward for NPC {npcID}");
            return;
        }
        Debug.Log($"[NPCInteraction] Claiming reward for quest {currentQuestData.questName} for NPC {npcID}");
        StartCoroutine(ClaimRewardCoroutine(currentQuestData));
    }

    private IEnumerator ClaimRewardCoroutine(QuestData quest)
    {
        yield return StartCoroutine(questManager.ClaimRewardCoroutine(quest));
    }

    private void OnContinueButtonPressed()
    {
        if (activeNPC != this)
        {
            Debug.Log($"[NPCInteraction] Continue button pressed, but NPC {npcID} is not the active NPC. Ignoring.");
            return;
        }

        Debug.Log($"[NPCInteraction] Continue button pressed for NPC {npcID}, DialogueActive: {dialogueManager?.IsDialogueActive}, DialogueLinesCount: {dialogueManager?.DialogueLinesCount}, IsTyping: {dialogueManager?.IsTyping}");

        if (dialogueManager != null && dialogueManager.IsDialogueActive)
        {
            dialogueManager.DisplayNextSentence();
        }
        else
        {
            Debug.Log($"[NPCInteraction] Ending dialogue for NPC {npcID}");
            dialogueManager?.EndDialogue();
            HideQuestUI();
            ResetActiveNPC();
        }
    }

    private void OnDialogueCompleted()
    {
        Debug.Log($"[NPCInteraction] Dialogue completed for NPC {npcID}, _isQuestAccepted={questManager?.uiManager?._isQuestAccepted}, _currentQuestTitleKey={questManager?.uiManager?._currentQuestTitleKey}");
        dialogueManager?.EndDialogue();
        HideQuestUI();
        ResetActiveNPC();
    }

    private void ShowAcceptDeclineUI()
    {
        SetButtonActive(acceptButton, true);
        SetButtonActive(declineButton, true);
        SetButtonActive(claimRewardButton, false);
        SetButtonActive(continueButton, true);
        Debug.Log("[NPCInteraction] Showing Accept/Decline UI");
    }

    private void ShowClaimRewardUI()
    {
        SetButtonActive(acceptButton, false);
        SetButtonActive(declineButton, false);
        SetButtonActive(claimRewardButton, true);
        SetButtonActive(continueButton, true);
        Debug.Log("[NPCInteraction] Showing Claim Reward UI");
    }

    private void ShowContinueUI()
    {
        SetButtonActive(acceptButton, false);
        SetButtonActive(declineButton, false);
        SetButtonActive(claimRewardButton, false);
        SetButtonActive(continueButton, true);
        Debug.Log("[NPCInteraction] Showing Continue UI");
    }

    private void HideAllActionButtons()
    {
        SetButtonActive(acceptButton, false);
        SetButtonActive(declineButton, false);
        SetButtonActive(claimRewardButton, false);
        Debug.Log("[NPCInteraction] Hiding all action buttons");
    }

    private void SetButtonActive(Button button, bool isActive)
    {
        if (button != null) button.gameObject.SetActive(isActive);
    }

    private void ShowQuestUI()
    {
        if (questUI != null)
        {
            questUI.SetActive(true);
            MouseManager.Instance?.ShowCursorAndDisableInput();
            Debug.Log("[NPCInteraction] Showing Quest UI");
        }
    }

    private void HideQuestUI()
    {
        if (questUI != null)
        {
            questUI.SetActive(false);
            SetButtonActive(continueButton, false);
            MouseManager.Instance?.HideCursorAndEnableInput();
            Debug.Log("[NPCInteraction] Hiding Quest UI");
        }
    }

    private void UpdateActiveNPC()
    {
        if (activeNPC != this)
        {
            if (activeNPC != null && activeNPC.continueButton != null)
            {
                activeNPC.continueButton.onClick.RemoveListener(activeNPC.OnContinueButtonPressed);
                Debug.Log($"[NPCInteraction] Removed listener from previous NPC {activeNPC.npcID}");
            }
            activeNPC = this;
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnContinueButtonPressed);
                Debug.Log($"[NPCInteraction] Registered listener for continueButton of NPC {npcID}");
            }
        }
        //if (panelPressText != null) panelPressText.gameObject.SetActive(false);
    }

    private void ResetActiveNPC()
    {
        activeNPC = null;
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonPressed);
            Debug.Log($"[NPCInteraction] Removed listener for NPC {npcID}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"[NPCInteraction] Player collided with NPC {npcID}");
            isPlayerInRange = true;
            if (panelPressText != null) panelPressText.gameObject.SetActive(true);
            UpdateActiveNPC();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"[NPCInteraction] Player stopped colliding with NPC {npcID}");
            isPlayerInRange = false;
            if (panelPressText != null) panelPressText.gameObject.SetActive(false);
            HideQuestUI();
            dialogueManager?.EndDialogue();
            MouseManager.Instance?.HideCursorAndEnableInput();
            if (activeNPC == this) ResetActiveNPC();
        }
    }

}
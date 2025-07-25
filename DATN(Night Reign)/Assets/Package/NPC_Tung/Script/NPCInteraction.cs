using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using static UnityEditor.Timeline.Actions.MenuPriority;

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
    private QuestData currentQuestData = null;
    private QuestDialogueType currentDialogueType;

    private void Start()
    {
        if (acceptButton != null) acceptButton.onClick.AddListener(OnAcceptButtonPressed);
        else Debug.LogWarning($"[NPCInteraction] acceptButton chưa được gán cho NPC {npcID}");

        if (declineButton != null) declineButton.onClick.AddListener(OnDeclineButtonPressed);
        else Debug.LogWarning($"[NPCInteraction] declineButton chưa được gán cho NPC {npcID}");

        if (claimRewardButton != null) claimRewardButton.onClick.AddListener(OnClaimRewardButtonPressed);
        else Debug.LogWarning($"[NPCInteraction] claimRewardButton chưa được gán cho NPC {npcID}");

        if (continueButton != null) continueButton.onClick.AddListener(OnContinueButtonPressed);
        else Debug.LogWarning($"[NPCInteraction] continueButton chưa được gán cho NPC {npcID}");

        if (questUI != null) questUI.SetActive(false);
        else Debug.LogWarning($"[NPCInteraction] questUI chưa được gán cho NPC {npcID}");

        if (panelPressText != null) panelPressText.gameObject.SetActive(false);
        else Debug.LogWarning($"[NPCInteraction] panelPressText chưa được gán cho NPC {npcID}");

        HideAllActionButtons();
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChangedHandler;
        Debug.Log($"[NPCInteraction] Khởi tạo cho NPC {npcID}");
        questManager.ResetAllQuests(); // Đặt lại tất cả nhiệm vụ khi NPC được khởi tạo
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChangedHandler;
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.C) && !isWaitingForContinueDialogue && dialogueManager != null && !dialogueManager.IsDialogueActive)
        {
            Debug.Log($"[NPCInteraction] Người chơi nhấn C để tương tác với NPC {npcID}, _isQuestAccepted={questManager?.uiManager?._isQuestAccepted}");
            HandleInteraction();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log($"[NPCInteraction] Người chơi nhấn Q để báo cáo tiêu diệt cho NPC {npcID}");
            if (questManager != null)
            {
                questManager.ReportKill();
                SimpleInventory.Instance.AddItem("Shadow Fangs", 5); 
                questManager.CheckItemCollectionProgress();
            }
            else Debug.LogWarning($"[NPCInteraction] questManager chưa được gán cho NPC {npcID}");
        }
    }

    private void HandleInteraction()
    {
        if (questUI == null)
        {
            Debug.LogWarning($"[NPCInteraction] questUI chưa được gán cho NPC {npcID}");
            return;
        }
        questUI.SetActive(true);
        if (MouseManager.Instance != null) MouseManager.Instance.ShowCursorAndDisableInput();

        if (panelPressText != null) panelPressText.gameObject.SetActive(false);

        var currentQuest = questManager?.GetCurrentQuest();
        var relatedQuests = questManager != null && questManager.questDatabase != null && questManager.questDatabase.quests != null
            ? questManager.questDatabase.quests.Where(q => q.giverNPCID == npcID && questManager.IsQuestUnlocked(q) && !q.isQuestCompleted).ToList()
            : new List<QuestData>();
        Debug.Log($"[NPCInteraction] Tìm thấy {relatedQuests.Count} nhiệm vụ cho NPC {npcID}: {string.Join(", ", relatedQuests.Select(q => q.questName))}");

        bool isTargetNPC = currentQuest != null && currentQuest.questType == QuestType.FindNPC && currentQuest.targetNPCID == npcID;
        bool isQuestInProgress = currentQuest != null && questManager != null && questManager.IsQuestAccepted(currentQuest) && !questManager.IsQuestCompleted(currentQuest);
        bool isQuestObjectiveMet = currentQuest != null && questManager != null && questManager.IsQuestObjectiveMet(currentQuest) && !questManager.IsQuestCompleted(currentQuest);
        bool isGiverNPC = currentQuest != null && currentQuest.giverNPCID == npcID;

        Debug.Log($"[NPCInteraction] Nhiệm vụ hiện tại: {(currentQuest != null ? currentQuest.questName : "null")}, isTargetNPC={isTargetNPC}, isQuestInProgress={isQuestInProgress}, isQuestObjectiveMet={isQuestObjectiveMet}, isGiverNPC={isGiverNPC}");

        if (currentQuest != null && isTargetNPC && isQuestInProgress)
        {
            Debug.Log($"[NPCInteraction] Hoàn thành nhiệm vụ FindNPC {currentQuest.questName} cho NPC mục tiêu {npcID}");
            if (questManager != null) questManager.OnInteractWithNPC(npcID);
            relatedQuests = questManager != null && questManager.questDatabase != null && questManager.questDatabase.quests != null
                ? questManager.questDatabase.quests.Where(q => q.giverNPCID == npcID && questManager.IsQuestUnlocked(q) && !q.isQuestCompleted).ToList()
                : new List<QuestData>();
            if (relatedQuests.Any())
            {
                var questToOffer = relatedQuests.First(q => questManager != null && questManager.IsQuestUnlocked(q));
                bool isDialogueEmpty = questToOffer.GetDialogueKeys(QuestDialogueType.BeforeComplete).Length == 0 &&
                                      questToOffer.GetDialogueKeys(QuestDialogueType.AfterComplete).Length == 0 &&
                                      questToOffer.GetDialogueKeys(QuestDialogueType.ObjectiveMet).Length == 0;

                if ((questToOffer.questType == QuestType.FindNPC || (questToOffer.questType == QuestType.KillEnemies && isDialogueEmpty)))
                {
                    Debug.Log($"[NPCInteraction] Tự động chấp nhận nhiệm vụ {(questToOffer.questType == QuestType.FindNPC ? "FindNPC" : "KillEnemies")} {questToOffer.questName} không có hội thoại");
                    if (questManager != null) questManager.AcceptQuest(questToOffer);
                    HideQuestUI();
                    if (MouseManager.Instance != null) MouseManager.Instance.HideCursorAndEnableInput();
                }
                else
                {
                    Debug.Log($"[NPCInteraction] Bắt đầu hội thoại BeforeComplete cho nhiệm vụ {questToOffer.questName}");
                    ShowQuestUI();
                    ShowAcceptDeclineUI();
                    currentQuestData = questToOffer;
                    currentDialogueType = QuestDialogueType.BeforeComplete;
                    StartQuestDialogue(currentQuestData, currentDialogueType);
                }
            }
            else
            {
                Debug.Log($"[NPCInteraction] Không có nhiệm vụ mới để cung cấp từ NPC {npcID}, hiển thị defaultGreetingDialogueKeys");
                ShowNoRelevantQuestDialogue();
            }
        }
        else
        {
            if (currentQuest != null && isGiverNPC && isQuestObjectiveMet)
            {
                Debug.Log($"[NPCInteraction] Bắt đầu hội thoại ObjectiveMet cho nhiệm vụ {currentQuest.questName} (đang chờ nhận thưởng)");
                ShowQuestUI();
                ShowClaimRewardUI();
                currentQuestData = currentQuest;
                currentDialogueType = QuestDialogueType.ObjectiveMet;
                StartQuestDialogue(currentQuestData, currentDialogueType);
            }
            else
            {
                if (currentQuest != null && isGiverNPC && isQuestInProgress)
                {
                    Debug.Log($"[NPCInteraction] Bắt đầu hội thoại ObjectiveMet cho nhiệm vụ {currentQuest.questName} (đang trong tiến trình)");
                    ShowQuestUI();
                    ShowContinueUI();
                    currentQuestData = currentQuest;
                    currentDialogueType = QuestDialogueType.ObjectiveMet;
                    StartQuestDialogue(currentQuestData, currentDialogueType);
                }
                else
                {
                    if (relatedQuests.Any() && (currentQuest == null || (questManager != null && !questManager.IsQuestAccepted(currentQuest))))
                    {
                        var questToOffer = relatedQuests.First(q => questManager != null && questManager.IsQuestUnlocked(q));
                        bool isDialogueEmpty = questToOffer.GetDialogueKeys(QuestDialogueType.BeforeComplete).Length == 0 &&
                                              questToOffer.GetDialogueKeys(QuestDialogueType.AfterComplete).Length == 0 &&
                                              questToOffer.GetDialogueKeys(QuestDialogueType.ObjectiveMet).Length == 0;

                        if ((questToOffer.questType == QuestType.FindNPC || (questToOffer.questType == QuestType.KillEnemies && isDialogueEmpty)))
                        {
                            Debug.Log($"[NPCInteraction] Tự động chấp nhận nhiệm vụ {(questToOffer.questType == QuestType.FindNPC ? "FindNPC" : "KillEnemies")} {questToOffer.questName} không có hội thoại");
                            if (questManager != null) questManager.AcceptQuest(questToOffer);
                            HideQuestUI();
                            if (MouseManager.Instance != null) MouseManager.Instance.HideCursorAndEnableInput();
                        }
                        else
                        {
                            Debug.Log($"[NPCInteraction] Bắt đầu hội thoại BeforeComplete cho nhiệm vụ {questToOffer.questName}");
                            ShowQuestUI();
                            ShowAcceptDeclineUI();
                            currentQuestData = questToOffer;
                            currentDialogueType = QuestDialogueType.BeforeComplete;
                            StartQuestDialogue(currentQuestData, currentDialogueType);
                        }
                    }
                    else
                    {
                        Debug.Log($"[NPCInteraction] NPC {npcID} không liên quan đến nhiệm vụ hiện tại hoặc không có nhiệm vụ chưa hoàn thành, hiển thị defaultGreetingDialogueKeys");
                        ShowNoRelevantQuestDialogue();
                    }
                }
            }
        }
    }

    private void ShowNoRelevantQuestDialogue()
    {
        HideAllActionButtons();
        ShowContinueUI();
        currentQuestData = null;
        currentDialogueType = QuestDialogueType.BeforeComplete;

        if (defaultGreetingDialogueKeys == null || defaultGreetingDialogueKeys.Length == 0)
        {
            Debug.LogWarning($"[NPCInteraction] defaultGreetingDialogueKeys trống cho NPC {npcID}, chuyển sang noRelevantQuestDialogueKeys");
            var keys = noRelevantQuestDialogueKeys;
            var clipsEN = noRelevantQuestVoiceClipsEN;
            var clipsVI = noRelevantQuestVoiceClipsVI;

            if (keys == null || keys.Length == 0)
            {
                Debug.LogWarning($"[NPCInteraction] Không có khóa hội thoại hợp lệ cho NPC {npcID}");
                if (dialogueManager != null) dialogueManager.EndDialogue();
                HideQuestUI();
                if (MouseManager.Instance != null) MouseManager.Instance.HideCursorAndEnableInput();
                return;
            }

            Debug.Log($"[NPCInteraction] Bắt đầu noRelevantQuestDialogueKeys với {keys.Length} dòng: {string.Join(", ", keys)}");
            if (dialogueManager != null)
            {
                dialogueManager.StartQuestDialogue(null, QuestDialogueType.BeforeComplete, () =>
                {
                    OnDialogueCompleted();
                }, keys, clipsEN, clipsVI);
            }
        }
        else
        {
            Debug.Log($"[NPCInteraction] Bắt đầu defaultGreetingDialogueKeys với {defaultGreetingDialogueKeys.Length} dòng: {string.Join(", ", defaultGreetingDialogueKeys)}");
            if (dialogueManager != null)
            {
                dialogueManager.StartQuestDialogue(null, QuestDialogueType.BeforeComplete, () =>
                {
                    OnDialogueCompleted();
                }, defaultGreetingDialogueKeys, defaultGreetingVoiceClipsEN, defaultGreetingVoiceClipsVI);
            }
        }
    }

    private void OnLanguageChangedHandler(Locale newLocale)
    {
        if (isPlayerInRange && questUI != null && questUI.activeSelf && dialogueManager != null && dialogueManager.IsDialogueActive)
        {
            Debug.Log($"[NPCInteraction] Ngôn ngữ thay đổi thành {newLocale.Identifier.Code}, làm mới dòng hội thoại cho NPC {npcID}");
            if (currentQuestData != null)
            {
                dialogueManager.StartQuestDialogue(currentQuestData, currentDialogueType, OnDialogueCompleted);
            }
            else
            {
                var keys = defaultGreetingDialogueKeys.Length > 0 ? defaultGreetingDialogueKeys : noRelevantQuestDialogueKeys;
                var clipsEN = defaultGreetingVoiceClipsEN.Length > 0 ? defaultGreetingVoiceClipsEN : noRelevantQuestVoiceClipsEN;
                var clipsVI = defaultGreetingVoiceClipsVI.Length > 0 ? defaultGreetingVoiceClipsVI : noRelevantQuestVoiceClipsVI;

                dialogueManager.StartQuestDialogue(null, QuestDialogueType.BeforeComplete, () =>
                {
                    OnDialogueCompleted();
                }, keys, clipsEN, clipsVI);
            }
        }
    }

    private void StartQuestDialogue(QuestData quest, QuestDialogueType dialogueType, Action onDialogueCompleted = null)
    {
        var keys = quest != null ? quest.GetDialogueKeys(dialogueType) : Array.Empty<string>();
        var clipsEN = quest != null ? dialogueType switch
        {
            QuestDialogueType.BeforeComplete => quest.voiceBeforeComplete_EN,
            QuestDialogueType.AfterComplete => quest.voiceAfterComplete_EN,
            QuestDialogueType.ObjectiveMet => quest.voiceObjectiveMet_EN,
            _ => Array.Empty<AudioClip>(),
        } : Array.Empty<AudioClip>();
        var clipsVI = quest != null ? dialogueType switch
        {
            QuestDialogueType.BeforeComplete => quest.voiceBeforeComplete_VI,
            QuestDialogueType.AfterComplete => quest.voiceAfterComplete_VI,
            QuestDialogueType.ObjectiveMet => quest.voiceObjectiveMet_VI,
            _ => Array.Empty<AudioClip>(),
        } : Array.Empty<AudioClip>();

        if (keys == null || keys.Length == 0)
        {
            Debug.LogWarning($"[NPCInteraction] Không tìm thấy khóa hội thoại cho {dialogueType} trong QuestData: {(quest != null ? quest.questName : "null")} cho NPC {npcID}");
            if (dialogueManager != null) dialogueManager.EndDialogue();
            onDialogueCompleted?.Invoke();
            HideQuestUI();
            if (MouseManager.Instance != null) MouseManager.Instance.HideCursorAndEnableInput();
            return;
        }

        Debug.Log($"[NPCInteraction] Bắt đầu hội thoại QuestDialogue cho {dialogueType} với {keys.Length} dòng: {string.Join(", ", keys)} cho NPC {npcID}");
        if (dialogueManager != null)
        {
            dialogueManager.StartQuestDialogue(quest, dialogueType, () =>
            {
                onDialogueCompleted?.Invoke();
            }, keys, clipsEN, clipsVI);
        }
    }

    private void OnAcceptButtonPressed()
    {
        if (currentQuestData == null)
        {
            Debug.LogWarning($"[NPCInteraction] Không có dữ liệu nhiệm vụ hiện tại để chấp nhận cho NPC {npcID}");
            return;
        }
        Debug.Log($"[NPCInteraction] Chấp nhận nhiệm vụ {currentQuestData.questName} cho NPC {npcID}");
        if (questManager != null)
        {
            questManager.AcceptQuest(currentQuestData);
#pragma warning disable CS4014
            questManager.uiManager?.RefreshCombinedQuestInfoUIAsync();
#pragma warning restore CS4014
        }
        HideQuestUI();
        if (MouseManager.Instance != null) MouseManager.Instance.HideCursorAndEnableInput();
    }

    private void OnDeclineButtonPressed()
    {
        if (currentQuestData == null)
        {
            Debug.LogWarning($"[NPCInteraction] Không có dữ liệu nhiệm vụ hiện tại để từ chối cho NPC {npcID}");
            return;
        }
        Debug.Log($"[NPCInteraction] Từ chối nhiệm vụ {currentQuestData.questName} cho NPC {npcID}");
        if (questManager != null) questManager.DeclineQuest(currentQuestData);
        HideQuestUI();
        if (MouseManager.Instance != null) MouseManager.Instance.HideCursorAndEnableInput();
    }

    private async void OnClaimRewardButtonPressed()
    {
        if (currentQuestData == null)
        {
            Debug.LogWarning($"[NPCInteraction] Không có dữ liệu nhiệm vụ hiện tại để nhận thưởng cho NPC {npcID}");
            return;
        }
        Debug.Log($"[NPCInteraction] Nhận thưởng cho nhiệm vụ {currentQuestData.questName} cho NPC {npcID}");
        if (questManager != null)
        {
            await questManager.ClaimReward(currentQuestData);
            isWaitingForContinueDialogue = true;
        }
    }

    private void OnContinueButtonPressed()
    {
        Debug.Log($"[NPCInteraction] Nút Tiếp tục được nhấn cho NPC {npcID}");
        if (dialogueManager != null) dialogueManager.DisplayNextSentence();
    }

    private void OnDialogueCompleted()
    {
        if (isWaitingForContinueDialogue)
        {
            Debug.Log($"[NPCInteraction] Hội thoại hoàn thành, đặt lại isWaitingForContinueDialogue cho NPC {npcID}");
            isWaitingForContinueDialogue = false;
        }
        if (dialogueManager != null) dialogueManager.EndDialogue();
        HideQuestUI();
        if (MouseManager.Instance != null) MouseManager.Instance.HideCursorAndEnableInput();
        Debug.Log($"[NPCInteraction] Hội thoại hoàn thành cho NPC {npcID}, _isQuestAccepted={questManager?.uiManager?._isQuestAccepted}, _currentQuestTitleKey={questManager?.uiManager?._currentQuestTitleKey}");
    }

    private void ShowAcceptDeclineUI()
    {
        if (acceptButton != null) acceptButton.gameObject.SetActive(true);
        if (declineButton != null) declineButton.gameObject.SetActive(true);
        if (claimRewardButton != null) claimRewardButton.gameObject.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(true);
        Debug.Log("[NPCInteraction] Gọi ShowAcceptDeclineUI, bật nút Chấp nhận/Từ chối và Tiếp tục");
    }

    private void ShowClaimRewardUI()
    {
        if (acceptButton != null) acceptButton.gameObject.SetActive(false);
        if (declineButton != null) declineButton.gameObject.SetActive(false);
        if (claimRewardButton != null) claimRewardButton.gameObject.SetActive(true);
        if (continueButton != null) continueButton.gameObject.SetActive(true);
        Debug.Log("[NPCInteraction] Gọi ShowClaimRewardUI, bật nút Nhận thưởng và Tiếp tục");
    }

    private void ShowContinueUI()
    {
        if (acceptButton != null) acceptButton.gameObject.SetActive(false);
        if (declineButton != null) declineButton.gameObject.SetActive(false);
        if (claimRewardButton != null) claimRewardButton.gameObject.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(true);
        Debug.Log("[NPCInteraction] Gọi ShowContinueUI, bật nút Tiếp tục");
    }

    private void HideAllActionButtons()
    {
        if (acceptButton != null) acceptButton.gameObject.SetActive(false);
        if (declineButton != null) declineButton.gameObject.SetActive(false);
        if (claimRewardButton != null) claimRewardButton.gameObject.SetActive(false);
        Debug.Log("[NPCInteraction] Gọi HideAllActionButtons, chỉ ẩn nút Chấp nhận/Từ chối/Nhận thưởng");
    }

    private void ShowQuestUI()
    {
        if (questUI != null)
        {
            questUI.SetActive(true);
            if (MouseManager.Instance != null) MouseManager.Instance.ShowCursorAndDisableInput();
            Debug.Log("[NPCInteraction] Gọi ShowQuestUI, hiển thị con trỏ và vô hiệu hóa đầu vào");
        }
    }

    private void HideQuestUI()
    {
        if (questUI != null) questUI.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (MouseManager.Instance != null) MouseManager.Instance.HideCursorAndEnableInput();
        Debug.Log("[NPCInteraction] Gọi HideQuestUI, ẩn con trỏ và kích hoạt đầu vào, ẩn nút Tiếp tục");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[NPCInteraction] Người chơi vào phạm vi trigger của NPC {npcID}");
            isPlayerInRange = true;
            if (panelPressText != null) panelPressText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[NPCInteraction] Người chơi rời phạm vi trigger của NPC {npcID}");
            isPlayerInRange = false;
            if (panelPressText != null) panelPressText.gameObject.SetActive(false);
            HideQuestUI();
            if (dialogueManager != null) dialogueManager.EndDialogue();
            if (MouseManager.Instance != null) MouseManager.Instance.HideCursorAndEnableInput();
        }
    }
}
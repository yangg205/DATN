using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using UnityEngine.SocialPlatforms;
using ND;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("References")]
    public QuestDatabase questDatabase;
    public PlayerStats playerStats;
    public UIManager uiManager;
    public WaypointManager waypointManager;

    [Header("UI References")]
    public GameObject questUI;
    public GameObject acceptButton;
    public GameObject declineButton;
    public GameObject claimRewardButton;

    private Dictionary<QuestData, CurrentQuestStatus> activeQuests = new Dictionary<QuestData, CurrentQuestStatus>();
    private int currentQuestIndex = 0;
    private bool isPlayingAfterCompleteDialogue = false;

    public class CurrentQuestStatus
    {
        public QuestData questData;
        public int currentProgress;
        public bool isObjectiveMet;
        public bool isCompleted;
        public string waypointId;

        public CurrentQuestStatus(QuestData data)
        {
            questData = data;
            currentProgress = 0;
            isObjectiveMet = false;
            isCompleted = false;
            waypointId = null;
        }

        public int GetRequiredProgress()
        {
            return questData.questType switch
            {
                QuestType.KillEnemies => questData.requiredKills,
                QuestType.CollectItem => questData.requiredItemCount,
                QuestType.FindNPC => 1,
                _ => 0,
            };
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (questDatabase == null || questDatabase.quests == null)
        {
            Debug.LogError("[QuestManager] QuestDatabase or quests not assigned!");
            return;
        }

        LoadQuestCompletionStatus();
        LoadQuestProgress();
        LoadCurrentQuestIndex();
        UpdateQuestIndex();
        Debug.Log($"[QuestManager] Initialized with currentQuestIndex={currentQuestIndex}, activeQuests={activeQuests.Count}");

        if (uiManager == null)
        {
            Debug.LogWarning("[QuestManager] UIManager is not assigned in Inspector!");
        }
        else
        {
            uiManager.HideQuestProgress();
            uiManager.SetQuestAccepted(false);
        }
        Debug.Log("[QuestManager] UI initialized with no active quest displayed.");
    }

    private void Start()
    {
        StartCoroutine(InitializeQuestUICoroutine());
    }

    private IEnumerator InitializeQuestUICoroutine()
    {
        yield return new WaitForEndOfFrame();
        if (activeQuests.Count > 0)
        {
            var activeQuest = GetActiveQuest();
            if (activeQuest != null)
            {
                yield return StartCoroutine(UpdateQuestUICoroutine(activeQuest));
                if (uiManager != null)
                {
                    uiManager.SetQuestAccepted(true);
                    yield return StartCoroutine(uiManager.RefreshCombinedQuestInfoUICoroutine());
                }
                Debug.Log($"[QuestManager] UI updated for quest {activeQuest.questName} in coroutine.");
            }
        }
        else
        {
            Debug.Log("[QuestManager] No active quests to initialize UI.");
        }
    }

    private void LoadQuestCompletionStatus()
    {
        foreach (var quest in questDatabase.quests)
        {
            string key = $"QuestCompleted_{quest.questName}";
            quest.isQuestCompleted = PlayerPrefs.GetInt(key, 0) == 1;
            Debug.Log($"[QuestManager] Loaded quest {quest.questName} completed status: {quest.isQuestCompleted}");
        }
    }

    private void SaveQuestCompletionStatus(QuestData quest)
    {
        if (quest == null) return;
        string key = $"QuestCompleted_{quest.questName}";
        PlayerPrefs.SetInt(key, quest.isQuestCompleted ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"[QuestManager] Saved completion status for quest {quest.questName}: {quest.isQuestCompleted}");
    }

    private void LoadQuestProgress()
    {
        activeQuests.Clear();
        string activeQuestNames = PlayerPrefs.GetString("ActiveQuests", "");
        if (!string.IsNullOrEmpty(activeQuestNames))
        {
            var questNames = activeQuestNames.Split(';');
            foreach (var questName in questNames)
            {
                if (string.IsNullOrEmpty(questName)) continue;
                var quest = questDatabase.quests.FirstOrDefault(q => q.questName == questName);
                if (quest != null && !quest.isQuestCompleted && IsQuestUnlocked(quest))
                {
                    var status = new CurrentQuestStatus(quest);
                    status.currentProgress = PlayerPrefs.GetInt($"QuestProgress_{questName}", 0);
                    status.isObjectiveMet = PlayerPrefs.GetInt($"QuestObjectiveMet_{questName}", 0) == 1;
                    status.isCompleted = PlayerPrefs.GetInt($"QuestCompletedStatus_{questName}", 0) == 1;
                    status.waypointId = PlayerPrefs.GetString($"QuestWaypointId_{questName}", null);
                    activeQuests.Add(quest, status);
                    if (!string.IsNullOrEmpty(status.waypointId) && waypointManager != null)
                    {
                        StartCoroutine(AddWaypointCoroutine(quest, status));
                    }
                    Debug.Log($"[QuestManager] Loaded quest {questName}: Progress={status.currentProgress}, ObjectiveMet={status.isObjectiveMet}, Completed={status.isCompleted}, WaypointId={status.waypointId}");
                }
            }
        }
        SaveQuestProgress();
    }

    private async Task AddWaypointAsync(QuestData quest, CurrentQuestStatus questStatus)
    {
        try
        {
            string questName = await quest.GetQuestNameLocalizedAsync();
            if (waypointManager != null)
            {
                waypointManager.AddWaypoint(new Waypoint(
                    questStatus.waypointId,
                    questName,
                    quest.questLocation,
                    WaypointType.QuestLocation,
                    quest.questLocationIcon
                ), true);
                Debug.Log($"[QuestManager] Added waypoint {questStatus.waypointId} for quest {quest.questName}");
            }
            else
            {
                Debug.LogWarning("[QuestManager] WaypointManager is null, cannot add waypoint.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[QuestManager] Error adding waypoint for quest {quest.questName}: {ex.Message}");
        }
    }

    private IEnumerator AddWaypointCoroutine(QuestData quest, CurrentQuestStatus questStatus)
    {
        yield return AddWaypointAsync(quest, questStatus);
    }

    private void SaveQuestProgress()
    {
        string questNames = string.Join(";", activeQuests.Keys.Select(q => q.questName));
        foreach (var quest in activeQuests.Keys)
        {
            var status = activeQuests[quest];
            PlayerPrefs.SetInt($"QuestProgress_{quest.questName}", status.currentProgress);
            PlayerPrefs.SetInt($"QuestObjectiveMet_{quest.questName}", status.isObjectiveMet ? 1 : 0);
            PlayerPrefs.SetInt($"QuestCompletedStatus_{quest.questName}", status.isCompleted ? 1 : 0);
            PlayerPrefs.SetString($"QuestWaypointId_{quest.questName}", status.waypointId ?? "");
        }
        PlayerPrefs.SetString("ActiveQuests", questNames);
        PlayerPrefs.Save();
        Debug.Log($"[QuestManager] Saved active quests: {questNames}");
    }

    private void LoadCurrentQuestIndex()
    {
        currentQuestIndex = PlayerPrefs.GetInt("CurrentQuestIndex", 0);
        Debug.Log($"[QuestManager] Loaded currentQuestIndex: {currentQuestIndex}");
    }

    private void SaveCurrentQuestIndex()
    {
        PlayerPrefs.SetInt("CurrentQuestIndex", currentQuestIndex);
        PlayerPrefs.Save();
        Debug.Log($"[QuestManager] Saved currentQuestIndex: {currentQuestIndex}");
    }

    public void ResetAllQuests()
    {
        foreach (var quest in questDatabase.quests)
        {
            quest.isQuestCompleted = false;
            string key = $"QuestCompleted_{quest.questName}";
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.DeleteKey($"QuestProgress_{quest.questName}");
            PlayerPrefs.DeleteKey($"QuestObjectiveMet_{quest.questName}");
            PlayerPrefs.DeleteKey($"QuestCompletedStatus_{quest.questName}");
            PlayerPrefs.DeleteKey($"QuestWaypointId_{quest.questName}");
            Debug.Log($"[QuestManager] Reset quest {quest.questName}");
        }
        activeQuests.Clear();
        PlayerPrefs.DeleteKey("ActiveQuests");
        PlayerPrefs.DeleteKey("CurrentQuestIndex");
        PlayerPrefs.Save();
        currentQuestIndex = 0;
        if (uiManager != null)
        {
            uiManager.HideQuestProgress();
            uiManager.SetQuestAccepted(false);
            uiManager.ShowNotice("Quest_Reset_All", 2.5f);
        }
        MouseManager.Instance?.HideCursorAndEnableInput();
        Debug.Log("[QuestManager] All quests reset, activeQuests cleared, currentQuestIndex set to 0");
    }

    private void UpdateQuestIndex()
    {
        for (int i = 0; i < questDatabase.quests.Length; i++)
        {
            var quest = questDatabase.quests[i];
            if (!quest.isQuestCompleted && IsQuestUnlocked(quest))
            {
                currentQuestIndex = i;
                SaveCurrentQuestIndex();
                Debug.Log($"[QuestManager] Updated currentQuestIndex to {currentQuestIndex} for quest {quest.questName}");
                return;
            }
        }
        currentQuestIndex = questDatabase.quests.Length;
        SaveCurrentQuestIndex();
        if (uiManager != null)
        {
            uiManager.HideQuestProgress();
            uiManager.SetQuestAccepted(false);
        }
        MouseManager.Instance?.HideCursorAndEnableInput();
        Debug.Log("[QuestManager] No active quest found, currentQuestIndex set to end");
    }

    public QuestData GetCurrentQuest()
    {
        if (questDatabase == null || questDatabase.quests == null || currentQuestIndex >= questDatabase.quests.Length)
        {
            Debug.LogWarning("[QuestManager] No current quest or invalid questDatabase.");
            return null;
        }
        var quest = questDatabase.quests[currentQuestIndex];
        if (!IsQuestUnlocked(quest))
        {
            Debug.LogWarning($"[QuestManager] Quest {quest.questName} is not unlocked yet.");
            return null;
        }
        Debug.Log($"[QuestManager] GetCurrentQuest: Returning quest {quest.questName} at index {currentQuestIndex}");
        return quest;
    }

    public int GetCurrentQuestIndex()
    {
        return currentQuestIndex;
    }

    public async void AcceptQuest(QuestData quest)
    {
        if (quest == null || activeQuests.ContainsKey(quest) || quest.isQuestCompleted)
        {
            Debug.LogWarning($"[QuestManager] Invalid quest, already accepted or completed: {(quest?.questName ?? "null")}");
            return;
        }

        if (!IsQuestUnlocked(quest))
        {
            Debug.LogWarning($"[QuestManager] Quest {quest.questName} is not unlocked, cannot accept.");
            return;
        }

        var status = new CurrentQuestStatus(quest);
        activeQuests.Add(quest, status);
        SaveQuestProgress();
        Debug.Log($"[QuestManager] Accepted quest {quest.questName} into activeQuests");

        if (quest.hasQuestLocation && waypointManager != null)
        {
            string waypointId = $"QuestWaypoint_{quest.questName}_{System.Guid.NewGuid()}";
            status.waypointId = waypointId;
            StartCoroutine(AddWaypointCoroutine(quest, status));
        }

        try
        {
            await UpdateQuestUI(quest);
            if (uiManager != null)
            {
                uiManager.SetQuestAccepted(true);
                await uiManager.RefreshCombinedQuestInfoUIAsync();
            }
            Debug.Log($"[QuestManager] Accepted quest: {quest.questName}, _isQuestAccepted={uiManager?._isQuestAccepted}, _currentQuestTitleKey={uiManager?._currentQuestTitleKey}, _currentProgressCount={uiManager?._currentProgressCount}/{uiManager?._totalProgressCount}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[QuestManager] Error accepting quest {quest.questName}: {ex.Message}");
        }

        for (int i = 0; i < questDatabase.quests.Length; i++)
        {
            if (questDatabase.quests[i] == quest)
            {
                currentQuestIndex = i;
                SaveCurrentQuestIndex();
                Debug.Log($"[QuestManager] Updated currentQuestIndex to {currentQuestIndex} for accepted quest {quest.questName}");
                break;
            }
        }
    }

    private async Task UpdateQuestUI(QuestData quest)
    {
        if (uiManager == null)
        {
            Debug.LogWarning("[QuestManager] UIManager is null, cannot update quest UI");
            return;
        }

        try
        {
            uiManager.UpdateQuestProgressText(quest.questName);
            uiManager.UpdateCurrentQuestObjective(quest.description);
            var status = GetQuestStatus(quest);
            int totalProgress = quest.questType == QuestType.FindNPC ? 1 : (quest.questType == QuestType.CollectItem ? quest.requiredItemCount : quest.requiredKills);
            uiManager.UpdateQuestProgress(status?.currentProgress ?? 0, totalProgress);
            uiManager.SetQuestAccepted(true);
            await uiManager.RefreshCombinedQuestInfoUIAsync();
            Debug.Log($"[QuestManager] Updated UI for quest {quest.questName}: _currentQuestTitleKey={quest.questName}, _currentObjectiveKey={quest.description}, _currentProgressCount={uiManager?._currentProgressCount}/{uiManager?._totalProgressCount}, _isQuestAccepted={uiManager?._isQuestAccepted}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[QuestManager] Error updating quest UI for {quest.questName}: {ex.Message}");
        }
    }

    private IEnumerator UpdateQuestUICoroutine(QuestData quest)
    {
        if (uiManager == null)
        {
            Debug.LogWarning("[QuestManager] UIManager is null, cannot update quest UI");
            yield break;
        }

        // Tách logic cập nhật UI ra khỏi try-catch để sử dụng yield
        uiManager.UpdateQuestProgressText(quest.questName);
        uiManager.UpdateCurrentQuestObjective(quest.description);
        var status = GetQuestStatus(quest);
        int totalProgress = quest.questType == QuestType.FindNPC ? 1 : (quest.questType == QuestType.CollectItem ? quest.requiredItemCount : quest.requiredKills);
        uiManager.UpdateQuestProgress(status?.currentProgress ?? 0, totalProgress);
        uiManager.SetQuestAccepted(true);

        try
        {
            // Không có yield trong try-catch
            Debug.Log($"[QuestManager] Preparing to update UI for quest {quest.questName}: _currentQuestTitleKey={quest.questName}, _currentObjectiveKey={quest.description}, _currentProgressCount={uiManager?._currentProgressCount}/{uiManager?._totalProgressCount}, _isQuestAccepted={uiManager?._isQuestAccepted}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[QuestManager] Error updating quest UI for {quest.questName}: {ex.Message}");
        }

        // Di chuyển yield ra ngoài try-catch
        yield return StartCoroutine(uiManager.RefreshCombinedQuestInfoUICoroutine());
    }

    public void ReportKill()
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);

        if (quest == null || status == null || status.isCompleted || quest.questType != QuestType.KillEnemies) return;

        status.currentProgress++;
        if (status.currentProgress >= status.GetRequiredProgress())
        {
            status.isObjectiveMet = true;
            uiManager?.SetReturnToNPCInfo(quest.giverNPCID, true);
        }
        uiManager?.UpdateQuestProgress(status.currentProgress, status.GetRequiredProgress());
        SaveQuestProgress();
        Debug.Log($"[QuestManager] Reported kill for quest {quest.questName}. Progress: {status.currentProgress}/{status.GetRequiredProgress()}, _currentProgressCount={uiManager?._currentProgressCount}/{uiManager?._totalProgressCount}");
    }

    public void CheckItemCollectionProgress()
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);

        if (quest == null || status == null || status.isCompleted || quest.questType != QuestType.CollectItem) return;

        int count = SimpleInventory.Instance.GetItemCount(quest.targetItemID);
        status.currentProgress = count;
        if (status.currentProgress >= quest.requiredItemCount)
        {
            status.isObjectiveMet = true;
            uiManager?.SetReturnToNPCInfo(quest.giverNPCID, true);
        }
        uiManager?.UpdateQuestProgress(count, quest.requiredItemCount);
        SaveQuestProgress();
        Debug.Log($"[QuestManager] Checked item collection for quest {quest.questName}. Progress: {count}/{quest.requiredItemCount}, _currentProgressCount={uiManager?._currentProgressCount}/{uiManager?._totalProgressCount}");
    }

    public async void OnInteractWithNPC(string npcId)
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);

        if (quest == null || status == null || status.isCompleted || quest.questType != QuestType.FindNPC) return;

        if (quest.targetNPCID == npcId)
        {
            Debug.Log($"[QuestManager] Interacted with target NPC {npcId} for quest {quest.questName}");
            await CompleteFindNPCQuest(quest, status, npcId);
        }
        else
        {
            Debug.LogWarning($"[QuestManager] NPC {npcId} is not the target for quest {quest.questName}");
        }
    }

    private async Task CompleteFindNPCQuest(QuestData quest, CurrentQuestStatus status, string npcId)
    {
        if (quest == null || status == null || status.isCompleted || quest.questType != QuestType.FindNPC) return;

        try
        {
            status.currentProgress = 1;
            status.isObjectiveMet = true;
            status.isCompleted = true;
            quest.isQuestCompleted = true;
            SaveQuestCompletionStatus(quest);

            if (uiManager != null)
            {
                uiManager.UpdateQuestProgressText(quest.questName);
                uiManager.UpdateQuestProgress(1, 1);
                uiManager.ShowNotice("Quest_Completed_Notice_Key", 2.5f);
                uiManager.SetReturnToNPCInfo("", false);
                uiManager.SetQuestAccepted(false);
                await uiManager.RefreshCombinedQuestInfoUIAsync();
            }

            if (!string.IsNullOrEmpty(status.waypointId))
                waypointManager?.RemoveWaypoint(status.waypointId);

            activeQuests.Remove(quest);
            SaveQuestProgress();
            UpdateQuestIndex();

            playerStats.currentEXP += quest.rewardExp;
            if (!string.IsNullOrEmpty(quest.rewardItemID))
                SimpleInventory.Instance?.AddItem(quest.rewardItemID, quest.rewardItemCount);

            uiManager?.ShowRewardPopup(quest.rewardCoin, quest.rewardExp);

            Debug.Log($"[QuestManager] Completed FindNPC quest {quest.questName}. _isQuestAccepted={uiManager?._isQuestAccepted}, _currentQuestTitleKey={uiManager?._currentQuestTitleKey}, _currentProgressCount={uiManager?._currentProgressCount}/{uiManager?._totalProgressCount}");

            await OfferNextQuestIfGiver(npcId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[QuestManager] Error completing FindNPC quest {quest.questName}: {ex.Message}");
        }
    }

    private async Task OfferNextQuestIfGiver(string npcId)
    {
        var nextQuest = GetCurrentQuest();
        if (nextQuest != null && nextQuest.giverNPCID == npcId && IsQuestUnlocked(nextQuest) && !nextQuest.isQuestCompleted)
        {
            Debug.Log($"[QuestManager] Offering next quest {nextQuest.questName} by NPC {npcId}");
            try
            {
                if (nextQuest.questType == QuestType.FindNPC)
                {
                    AcceptQuest(nextQuest);
                    await UpdateQuestUI(nextQuest);
                    if (uiManager != null)
                    {
                        await uiManager.RefreshCombinedQuestInfoUIAsync();
                    }
                    Debug.Log($"[QuestManager] Automatically accepted FindNPC quest {nextQuest.questName} without dialogue");
                }
                else
                {
                    DialogueManager.Instance.StartQuestDialogue(nextQuest, QuestDialogueType.BeforeComplete, async () =>
                    {
                        HideQuestUI();
                        MouseManager.Instance?.HideCursorAndEnableInput();
                        Debug.Log($"[QuestManager] Completed BeforeComplete dialogue for quest {nextQuest.questName}");
                        if (uiManager != null)
                        {
                            await uiManager.RefreshCombinedQuestInfoUIAsync();
                        }
                    }, nextQuest.GetDialogueKeys(QuestDialogueType.BeforeComplete), nextQuest.voiceBeforeComplete_EN, nextQuest.voiceBeforeComplete_VI);
                    ShowQuestUI();
                    acceptButton?.SetActive(true);
                    declineButton?.SetActive(true);
                    claimRewardButton?.SetActive(false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QuestManager] Error offering next quest {nextQuest.questName}: {ex.Message}");
            }
        }
        else
        {
            Debug.Log($"[QuestManager] No valid next quest for NPC {npcId}, hiding UI");
            if (uiManager != null)
            {
                uiManager.HideQuestProgress();
                uiManager.SetQuestAccepted(false);
            }
            HideQuestUI();
            MouseManager.Instance?.HideCursorAndEnableInput();
        }
    }

    public bool IsQuestUnlocked(QuestData quest)
    {
        if (questDatabase == null || questDatabase.quests == null)
        {
            Debug.LogError("[QuestManager] QuestDatabase or quests array is null.");
            return false;
        }

        if (string.IsNullOrEmpty(quest.prerequisiteQuestName))
        {
            Debug.Log($"[QuestManager] Quest {quest.questName} has no prerequisite, unlocked");
            return true;
        }

        var prerequisiteQuest = questDatabase.quests.FirstOrDefault(q => q.questName == quest.prerequisiteQuestName);
        if (prerequisiteQuest == null)
        {
            Debug.LogWarning($"[QuestManager] Prerequisite quest {quest.prerequisiteQuestName} not found for {quest.questName}. Assuming locked.");
            return false;
        }

        bool isUnlocked = prerequisiteQuest.isQuestCompleted;
        Debug.Log($"[QuestManager] Checking if quest {quest.questName} is unlocked. Prerequisite: {quest.prerequisiteQuestName}, Completed: {isUnlocked}");
        return isUnlocked;
    }

    public void CompleteQuest(QuestData quest)
    {
        var status = GetQuestStatus(quest);
        if (quest == null || status == null || !status.isObjectiveMet || status.isCompleted)
        {
            Debug.LogWarning($"[QuestManager] Cannot complete quest {(quest != null ? quest.questName : "null")}. Status: {(status != null ? $"ObjectiveMet={status.isObjectiveMet}, Completed={status.isCompleted}" : "null")}");
            return;
        }

        status.isCompleted = true;
        quest.isQuestCompleted = true;
        SaveQuestCompletionStatus(quest);

        if (uiManager != null)
        {
            uiManager.UpdateQuestProgressText(quest.questName);
            uiManager.UpdateQuestProgress(status.currentProgress, status.GetRequiredProgress());
            uiManager.ShowNotice("Quest_Completed_Notice_Key", 2.5f);
            uiManager.SetReturnToNPCInfo("", false);
            uiManager.SetQuestAccepted(false);
        }

        playerStats.currentEXP += quest.rewardExp;
        if (!string.IsNullOrEmpty(quest.rewardItemID))
            SimpleInventory.Instance?.AddItem(quest.rewardItemID, quest.rewardItemCount);

        uiManager?.ShowRewardPopup(quest.rewardCoin, quest.rewardExp);

        if (quest.questType == QuestType.CollectItem)
            SimpleInventory.Instance?.AddItem(quest.targetItemID, -quest.requiredItemCount);

        if (!string.IsNullOrEmpty(status.waypointId))
            waypointManager?.RemoveWaypoint(status.waypointId);

        activeQuests.Remove(quest);
        SaveQuestProgress();
        UpdateQuestIndex();
        MouseManager.Instance?.HideCursorAndEnableInput();
        Debug.Log($"[QuestManager] Completed quest {quest.questName}, _isQuestAccepted={uiManager?._isQuestAccepted}, _currentQuestTitleKey={uiManager?._currentQuestTitleKey}, _currentProgressCount={uiManager?._currentProgressCount}/{uiManager?._totalProgressCount}");
    }

    public async Task ClaimReward(QuestData quest)
    {
        var status = GetQuestStatus(quest);
        if (quest == null || status == null || !status.isObjectiveMet || status.isCompleted)
        {
            Debug.LogWarning($"[QuestManager] Cannot claim reward for quest {(quest != null ? quest.questName : "null")}");
            return;
        }

        CompleteQuest(quest);

        string[] afterCompleteKeys = quest.GetDialogueKeys(QuestDialogueType.AfterComplete);
        AudioClip[] afterCompleteClipsEN = quest.voiceAfterComplete_EN;
        AudioClip[] afterCompleteClipsVI = quest.voiceAfterComplete_VI;

        if (afterCompleteKeys.Length > 0 && quest.questType != QuestType.FindNPC)
        {
            isPlayingAfterCompleteDialogue = true;
            ShowQuestUI();
            HideAllActionButtons();

            DialogueManager.Instance.StartQuestDialogue(quest, QuestDialogueType.AfterComplete, async () =>
            {
                isPlayingAfterCompleteDialogue = false;
                HideQuestUI();
                MouseManager.Instance?.HideCursorAndEnableInput();
                await OfferNextQuestIfGiver(quest.giverNPCID);
            }, afterCompleteKeys, afterCompleteClipsEN, afterCompleteClipsVI);
        }
        else
        {
            isPlayingAfterCompleteDialogue = false;
            HideQuestUI();
            MouseManager.Instance?.HideCursorAndEnableInput();
            await OfferNextQuestIfGiver(quest.giverNPCID);
        }
    }

    public void DeclineQuest(QuestData quest)
    {
        if (quest == null || !activeQuests.ContainsKey(quest))
        {
            Debug.LogWarning($"[QuestManager] Cannot decline quest {(quest != null ? quest.questName : "null")} - not active or invalid.");
            return;
        }

        var status = GetQuestStatus(quest);

        if (status != null && !string.IsNullOrEmpty(status.waypointId))
            waypointManager?.RemoveWaypoint(status.waypointId);

        activeQuests.Remove(quest);
        SaveQuestProgress();
        if (uiManager != null)
        {
            uiManager.HideQuestProgress();
            uiManager.SetQuestAccepted(false);
        }
        HideQuestUI();
        MouseManager.Instance?.HideCursorAndEnableInput();
        Debug.Log($"[QuestManager] Declined quest {quest.questName}, _isQuestAccepted={uiManager?._isQuestAccepted}, _currentQuestTitleKey={uiManager?._currentQuestTitleKey}");
    }

    public CurrentQuestStatus GetQuestStatus(QuestData quest)
    {
        activeQuests.TryGetValue(quest, out var status);
        return status;
    }

    public bool IsQuestObjectiveMet(QuestData quest)
    {
        return GetQuestStatus(quest)?.isObjectiveMet ?? false;
    }

    public bool IsQuestCompleted(QuestData quest)
    {
        return quest != null && quest.isQuestCompleted;
    }

    public bool IsQuestAccepted(QuestData quest)
    {
        return activeQuests.ContainsKey(quest) && !IsQuestCompleted(quest);
    }

    public void ShowQuestUI()
    {
        questUI?.SetActive(true);
        MouseManager.Instance?.ShowCursorAndDisableInput();
        Debug.Log("[QuestManager] ShowQuestUI called, cursor shown and input disabled");
    }

    public void HideQuestUI()
    {
        if (isPlayingAfterCompleteDialogue) return;
        questUI?.SetActive(false);
        MouseManager.Instance?.HideCursorAndEnableInput();
        Debug.Log("[QuestManager] HideQuestUI called, cursor hidden and input enabled");
    }

    public void HideAllActionButtons()
    {
        acceptButton?.SetActive(false);
        declineButton?.SetActive(false);
        claimRewardButton?.SetActive(false);
        Debug.Log("[QuestManager] HideAllActionButtons called");
    }

    public void SetCurrentQuestIndex(int index)
    {
        if (index >= 0 && index < (questDatabase?.quests?.Length ?? 0))
        {
            currentQuestIndex = index;
            SaveCurrentQuestIndex();
            Debug.Log($"[QuestManager] Set currentQuestIndex to {currentQuestIndex}");
        }
    }

    public QuestData GetActiveQuest()
    {
        if (activeQuests.Count > 0)
        {
            foreach (var pair in activeQuests)
            {
                if (!pair.Value.isCompleted)
                {
                    Debug.Log($"[QuestManager] GetActiveQuest: Returning quest {pair.Key.questName}");
                    return pair.Key;
                }
            }
        }
        Debug.Log("[QuestManager] GetActiveQuest: No active quest found");
        return null;
    }
}
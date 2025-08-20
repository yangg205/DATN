using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Boss helpers (scene-wide)")]
    // Optional: a fallback BossZone prefab or a common BossZone controller if you want
    public BossZoneController commonBossZoneController;

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

        // Boss related
        public GameObject bossInstance;
        public BossZoneController bossZone;
        public int bossDeathCount; // số lần player chết trong boss fight (cộng dồn)

        public CurrentQuestStatus(QuestData data)
        {
            questData = data;
            currentProgress = 0;
            isObjectiveMet = false;
            isCompleted = false;
            waypointId = null;
            bossInstance = null;
            bossZone = null;
            bossDeathCount = 0;
        }

        public int GetRequiredProgress()
        {
            return questData.questType switch
            {
                QuestType.KillEnemies => questData.requiredKills,
                QuestType.CollectItem => questData.requiredItemCount,
                // treat KillBoss as 1
                QuestType.FindNPC => 1,
                // if you added KillBoss enum, handle it:
                // QuestType.KillBoss => 1,
                _ => 0,
            };
        }
    }

    #region Unity lifecycle & init (giữ nguyên, chỉ thêm load boss death count)
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
        var currentQuest = GetCurrentQuest();
        if (currentQuest != null && IsQuestUnlocked(currentQuest))
        {
            yield return StartCoroutine(UpdateQuestUICoroutine(currentQuest));
            if (uiManager != null)
            {
                uiManager.SetQuestAccepted(activeQuests.ContainsKey(currentQuest));
                yield return StartCoroutine(uiManager.RefreshCombinedQuestInfoUICoroutine());
            }
            Debug.Log($"[QuestManager] UI updated for quest {currentQuest.questName}, isAccepted={activeQuests.ContainsKey(currentQuest)}");
        }
        else
        {
            Debug.Log("[QuestManager] No valid quest to initialize UI.");
            if (uiManager != null)
            {
                uiManager.HideQuestProgress();
                uiManager.SetQuestAccepted(false);
            }
        }
    }
    #endregion

    #region Save / Load (thêm bossDeathCount)
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
                    status.bossDeathCount = PlayerPrefs.GetInt($"QuestBossDeathCount_{questName}", 0);
                    activeQuests.Add(quest, status);
                    if (!string.IsNullOrEmpty(status.waypointId) && waypointManager != null)
                    {
                        StartCoroutine(AddWaypointCoroutine(quest, status));
                    }
                    Debug.Log($"[QuestManager] Loaded quest {questName}: Progress={status.currentProgress}, ObjectiveMet={status.isObjectiveMet}, Completed={status.isCompleted}, WaypointId={status.waypointId}, BossDeaths={status.bossDeathCount}");
                }
            }
        }
        SaveQuestProgress();
    }

    private IEnumerator AddWaypointCoroutine(QuestData quest, CurrentQuestStatus questStatus)
    {
        string questName = quest.questName;
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
        yield return null;
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
            PlayerPrefs.SetInt($"QuestBossDeathCount_{quest.questName}", status.bossDeathCount);
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
    #endregion

    #region Quest accept / UI (giữ nguyên)
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
            PlayerPrefs.DeleteKey($"QuestBossDeathCount_{quest.questName}");
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

    public void AcceptQuest(QuestData quest)
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

        StartCoroutine(AcceptQuestCoroutine(quest));

        // Nếu đây là KillBoss và spawnBossOnEnter == false (spawn khi accept), spawn ngay
        if (quest.questType == QuestType.KillEnemies || quest.questType.ToString() == "KillBoss")
        {
            // if you use separate enum value KillBoss, prefer: if (quest.questType == QuestType.KillBoss)
            // spawn on accept if configured (backwards-compatible)
            if (quest.spawnBossOnEnter == false && quest.bossPrefab != null)
            {
                // Spawn immediately at questLocation
                SpawnBossForQuest(quest);
            }
        }
    }

    private IEnumerator AcceptQuestCoroutine(QuestData quest)
    {
        yield return StartCoroutine(UpdateQuestUICoroutine(quest));
        if (uiManager != null)
        {
            uiManager.SetQuestAccepted(true);
            yield return StartCoroutine(uiManager.RefreshCombinedQuestInfoUICoroutine());
        }
        Debug.Log($"[QuestManager] Accepted quest: {quest.questName}, _isQuestAccepted={uiManager?._isQuestAccepted}, _currentQuestTitleKey={uiManager?._currentQuestTitleKey}, _currentProgressCount={uiManager?._currentProgressCount}/{uiManager?._totalProgressCount}");

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

    private IEnumerator UpdateQuestUICoroutine(QuestData quest)
    {
        if (uiManager == null)
        {
            Debug.LogWarning("[QuestManager] UIManager is null, cannot update quest UI");
            yield break;
        }

        uiManager.UpdateQuestProgressText(quest.questName);
        uiManager.UpdateCurrentQuestObjective(quest.description);
        var status = GetQuestStatus(quest);
        int totalProgress = quest.questType == QuestType.FindNPC ? 1 : (quest.questType == QuestType.CollectItem ? quest.requiredItemCount : quest.requiredKills);
        // If KillBoss (if you added enum), make totalProgress = 1
        if (quest.questType.ToString() == "KillBoss") totalProgress = 1;

        uiManager.UpdateQuestProgress(status?.currentProgress ?? 0, totalProgress);
        uiManager.SetQuestAccepted(activeQuests.ContainsKey(quest));
        yield return StartCoroutine(uiManager.RefreshCombinedQuestInfoUICoroutine());
        Debug.Log($"[QuestManager] Updated UI for quest {quest.questName}: _currentQuestTitleKey={quest.questName}, _currentObjectiveKey={quest.description}, _currentProgressCount={uiManager?._currentProgressCount}/{uiManager?._totalProgressCount}, _isQuestAccepted={uiManager?._isQuestAccepted}");
    }
    #endregion

    #region Kill logic (giữ ReportKill, thêm boss flow)
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
            // Update waypoint to point to giverNPCTransform
            UpdateWaypointToGiverNPC(quest, status);
        }
        uiManager?.UpdateQuestProgress(status.currentProgress, status.GetRequiredProgress());
        SaveQuestProgress();
        Debug.Log($"[QuestManager] Reported kill for quest {quest.questName}. Progress: {status.currentProgress}/{status.GetRequiredProgress()}, _currentProgressCount={uiManager?._currentProgressCount}/{uiManager?._totalProgressCount}");
    }

    // NEW: gọi khi player đi vào quest location (trigger)
    public void OnPlayerEnterQuestLocation(QuestData quest)
    {
        if (quest == null) return;

        // nếu quest chưa accept thì auto accept (tùy game design)
        if (!IsQuestAccepted(quest))
        {
            AcceptQuest(quest);
        }

        // Nếu quest có boss và cấu hình spawn khi vào zone
        if (quest.bossPrefab != null && quest.spawnBossOnEnter)
        {
            SpawnBossForQuest(quest);
        }
        else
        {
            // (còn nếu là KillEnemies bạn có thể spawn wave quái ở đây nếu cần)
            // bạn có thể gắn QuestEnemySpawner lên khu vực map để xử lý spawn triệu quái riêng.
            Debug.Log($"[QuestManager] Player entered quest location for {quest.questName}, no boss spawn config or bossPrefab null");
        }
    }

    // Tạo/ spawn boss cho quest -> attach BossController và BossZone nếu có
    private void SpawnBossForQuest(QuestData quest)
    {
        if (quest == null) return;
        var status = GetQuestStatus(quest);
        if (status == null)
        {
            // nếu chưa accept, accept trước
            AcceptQuest(quest);
            status = GetQuestStatus(quest);
            if (status == null) return;
        }

        // Nếu boss đã tồn tại thì không spawn thêm
        if (status.bossInstance != null)
        {
            Debug.Log($"[QuestManager] Boss already spawned for quest {quest.questName}");
            return;
        }

        if (quest.bossPrefab == null)
        {
            Debug.LogWarning($"[QuestManager] No bossPrefab assigned for quest {quest.questName}");
            return;
        }

        // Instantiate boss at quest.questLocation
        Vector3 spawnPos = quest.questLocation;
        var bossGO = Instantiate(quest.bossPrefab, spawnPos, Quaternion.identity);
        status.bossInstance = bossGO;


        // Spawn/activate zone if provided
        if (quest.bossZonePrefab != null)
        {
            var zoneGO = Instantiate(quest.bossZonePrefab, spawnPos, Quaternion.identity);
            var zoneController = zoneGO.GetComponent<BossZoneController>();
            if (zoneController == null) zoneController = zoneGO.AddComponent<BossZoneController>();
            status.bossZone = zoneController;
            if (quest.activateBossZoneOnSpawn) zoneController.ActivateZone();
        }
        else if (quest.activateBossZoneOnSpawn && commonBossZoneController != null)
        {
            // fallback: enable common zone
            status.bossZone = commonBossZoneController;
            commonBossZoneController.ActivateZone();
        }

        // Start the Fight UI / timer via BattleBoss (find in scene)
        var battleBoss = FindObjectOfType<BattleBoss>();
        if (battleBoss != null)
        {
            // Pass initial death count from saved status
            battleBoss.StartFight(playerStats?.GetPlayerCharacterId() ?? 0, quest.bossId, quest.bossMaxTime, status.bossDeathCount);
        }

        SaveQuestProgress();
        Debug.Log($"[QuestManager] Spawned boss for quest {quest.questName} at {spawnPos}. BossId={quest.bossId}");
    }

    // Khi boss báo chết (BossController gọi)
    public void OnBossDefeated(QuestData quest)
    {
        if (quest == null) return;
        var status = GetQuestStatus(quest);
        if (status == null) return;
        // mark objective met
        status.isObjectiveMet = true;
        status.isCompleted = true;
        quest.isQuestCompleted = true;
        status.currentProgress++;
        // deactivate zone
        status.bossZone?.DeactivateZone();

        // remove boss instance
        if (status.bossInstance != null)
        {
            Destroy(status.bossInstance);
            status.bossInstance = null;
        }

        SaveQuestCompletionStatus(quest);
        SaveQuestProgress();

        // give rewards / UI
        playerStats.currentEXP += quest.rewardExp;
        if (!string.IsNullOrEmpty(quest.rewardItemID))
            SimpleInventory.Instance?.AddItem(quest.rewardItemID, quest.rewardItemCount);

        uiManager?.ShowRewardPopup(quest.rewardCoin, quest.rewardExp);
        uiManager?.ShowNotice("Quest_Completed_Notice_Key", 2.5f);
        uiManager?.SetReturnToNPCInfo("", false);
        uiManager?.SetQuestAccepted(false);

        // Remove waypoint etc
        if (!string.IsNullOrEmpty(status.waypointId))
            waypointManager?.RemoveWaypoint(status.waypointId);

        // remove from activeQuests and update index
        activeQuests.Remove(quest);
        SaveQuestProgress();
        UpdateQuestIndex();
        MouseManager.Instance?.HideCursorAndEnableInput();

        Debug.Log($"[QuestManager] Boss quest {quest.questName} defeated and completed. Deaths during fight: {status.bossDeathCount}");
    }

    // Gọi khi player chết trong boss fight: tăng death count, respawn player, respawn boss, reset timer
    // (BattleBoss.OnPlayerDeath nên gọi QuestManager.Instance.OnPlayerDeathDuringBossFight();)
    public void OnPlayerDeathDuringBossFight()
    {
        var quest = GetActiveQuest();
        if (quest == null) return;
        var status = GetQuestStatus(quest);
        if (status == null) return;

        // Only care if this is a boss quest
        if (quest.bossPrefab == null) return;

        status.bossDeathCount++;
        SaveQuestProgress();
        Debug.Log($"[QuestManager] Player died in boss fight for {quest.questName}. Deaths now: {status.bossDeathCount}");

        // Respawn player to quest location
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = quest.questLocation;
            playerStats.RespawnAt(quest.questLocation);
        }

        // Reset boss: destroy old instance and spawn new one, and reset timer
        if (status.bossInstance != null)
        {
            Destroy(status.bossInstance);
            status.bossInstance = null;
        }

        // If zone exists, ensure it is active (block exit)
        status.bossZone?.ActivateZone();

        // respawn boss
        SpawnBossForQuest(quest);

        // ensure BattleBoss UI restarts with reset timer but deathCount maintained
        var battleBoss = FindObjectOfType<BattleBoss>();
        if (battleBoss != null)
        {
            battleBoss.StartFight(playerStats?.GetPlayerCharacterId() ?? 0, quest.bossId, quest.bossMaxTime, status.bossDeathCount);
        }
    }
    #endregion

    #region item / find npc / waypoint (giữ nguyên)
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
            // Update waypoint to point to giverNPCTransform
            UpdateWaypointToGiverNPC(quest, status);
        }
        uiManager?.UpdateQuestProgress(count, quest.requiredItemCount);
        SaveQuestProgress();
        Debug.Log($"[QuestManager] Checked item collection for quest {quest.questName}. Progress: {count}/{quest.requiredItemCount}, _currentProgressCount={uiManager?._currentProgressCount}/{uiManager?._totalProgressCount}");
    }
    private void UpdateWaypointToGiverNPC(QuestData quest, CurrentQuestStatus questStatus)
    {
        if (quest == null || questStatus == null || waypointManager == null) return;

        // Remove existing waypoint if it exists
        if (!string.IsNullOrEmpty(questStatus.waypointId))
        {
            waypointManager.RemoveWaypoint(questStatus.waypointId);
            Debug.Log($"[QuestManager] Removed previous waypoint {questStatus.waypointId} for quest {quest.questName}");
        }

        // Set new waypoint to giverNPCTransform
        if (quest.hasQuestLocation)
        {
            string waypointId = $"QuestWaypoint_Return_{quest.questName}_{System.Guid.NewGuid()}";
            questStatus.waypointId = waypointId;
            waypointManager.AddWaypoint(new Waypoint(
                waypointId,
                quest.questName,
                quest.giverNPCTransform, // Use giverNPCTransform for return location
                WaypointType.QuestLocation,
                quest.questLocationIcon
            ), true);
            Debug.Log($"[QuestManager] Added waypoint {waypointId} for quest {quest.questName} at giverNPCTransform {quest.giverNPCTransform}");
        }
        else
        {
            Debug.Log($"[QuestManager] Quest {quest.questName} has no quest location, skipping waypoint update.");
        }

        SaveQuestProgress();
    }
    #endregion

    #region FindNPC flow (giữ nguyên)
    public void OnInteractWithNPC(string npcId)
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);

        if (quest == null || status == null || status.isCompleted || quest.questType != QuestType.FindNPC) return;

        if (quest.targetNPCID == npcId)
        {
            Debug.Log($"[QuestManager] Interacted with target NPC {npcId} for quest {quest.questName}");
            StartCoroutine(CompleteFindNPCQuestCoroutine(quest, status, npcId));
        }
        else
        {
            Debug.LogWarning($"[QuestManager] NPC {npcId} is not the target for quest {quest.questName}");
        }
    }

    private IEnumerator CompleteFindNPCQuestCoroutine(QuestData quest, CurrentQuestStatus status, string npcId)
    {
        if (quest == null || status == null || status.isCompleted || quest.questType != QuestType.FindNPC) yield break;

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
            yield return StartCoroutine(uiManager.RefreshCombinedQuestInfoUICoroutine());
        }

        if (!string.IsNullOrEmpty(status.waypointId))
            waypointManager?.RemoveWaypoint(status.waypointId);

        activeQuests.Remove(quest);
        SaveQuestProgress();
        UpdateQuestIndex(); // Cập nhật currentQuestIndex để lấy nhiệm vụ mới

        playerStats.currentEXP += quest.rewardExp;
        if (!string.IsNullOrEmpty(quest.rewardItemID))
            SimpleInventory.Instance?.AddItem(quest.rewardItemID, quest.rewardItemCount);

        uiManager?.ShowRewardPopup(quest.rewardCoin, quest.rewardExp);

        Debug.Log($"[QuestManager] Completed FindNPC quest {quest.questName}. _isQuestAccepted={uiManager?._isQuestAccepted}, _currentQuestTitleKey={uiManager?._currentQuestTitleKey}, _currentProgressCount={uiManager?._currentProgressCount}/{uiManager?._totalProgressCount}");

        // Thông báo cho NPC giao nhiệm vụ rằng nhiệm vụ đã hoàn thành
        if (quest.giverNPCID != npcId)
        {
            Debug.Log($"[QuestManager] Notifying giver NPC {quest.giverNPCID} that quest {quest.questName} is completed");
            yield return StartCoroutine(OfferNextQuestIfGiverCoroutine(quest.giverNPCID));
        }

        // Cung cấp nhiệm vụ mới từ NPC mục tiêu nếu có
        yield return StartCoroutine(OfferNextQuestIfGiverCoroutine(npcId));
    }
    #endregion

    #region Offer next quest & unlock (giữ nguyên)
    private IEnumerator OfferNextQuestIfGiverCoroutine(string npcId)
    {
        var nextQuest = GetCurrentQuest();
        if (nextQuest == null || !IsQuestUnlocked(nextQuest) || nextQuest.isQuestCompleted)
        {
            Debug.Log($"[QuestManager] No valid next quest for NPC {npcId}, hiding UI");
            if (uiManager != null)
            {
                uiManager.HideQuestProgress();
                uiManager.SetQuestAccepted(false);
            }
            HideQuestUI();
            MouseManager.Instance?.HideCursorAndEnableInput();
            yield break;
        }

        Debug.Log($"[QuestManager] Offering next quest {nextQuest.questName} for NPC {npcId}, giverNPCID={nextQuest.giverNPCID}");
        yield return StartCoroutine(UpdateQuestUICoroutine(nextQuest));
        if (uiManager != null)
        {
            uiManager.SetQuestAccepted(false);
            yield return StartCoroutine(uiManager.RefreshCombinedQuestInfoUICoroutine());
        }

        if (nextQuest.giverNPCID == npcId)
        {
            var beforeCompleteKeys = nextQuest.GetDialogueKeys(QuestDialogueType.BeforeComplete);
            if (beforeCompleteKeys == null || beforeCompleteKeys.Length == 0)
            {
                Debug.Log($"[QuestManager] No BeforeComplete dialogue for {nextQuest.questName}, auto-accepting");
                AcceptQuest(nextQuest);
                yield return StartCoroutine(UpdateQuestUICoroutine(nextQuest));
                if (uiManager != null)
                {
                    yield return StartCoroutine(uiManager.RefreshCombinedQuestInfoUICoroutine());
                }
            }
            else
            {
                Debug.Log($"[QuestManager] Starting BeforeComplete dialogue for {nextQuest.questName} with {beforeCompleteKeys.Length} lines: {string.Join(", ", beforeCompleteKeys)}");
                DialogueManager.Instance?.StartQuestDialogue(nextQuest, QuestDialogueType.BeforeComplete, () =>
                {
                    HideQuestUI();
                    MouseManager.Instance?.HideCursorAndEnableInput();
                    Debug.Log($"[QuestManager] Completed BeforeComplete dialogue for quest {nextQuest.questName}");
                    if (nextQuest.questType == QuestType.FindNPC)
                    {
                        AcceptQuest(nextQuest); // Tự động nhận quest FindNPC sau khi hoàn thành dialogue
                        StartCoroutine(UpdateQuestUICoroutine(nextQuest));
                    }
                }, beforeCompleteKeys, nextQuest.voiceBeforeComplete_EN, nextQuest.voiceBeforeComplete_VI);
                ShowQuestUI();
                acceptButton?.SetActive(true);
                declineButton?.SetActive(true);
                claimRewardButton?.SetActive(false);
            }
        }
        else
        {
            Debug.Log($"[QuestManager] NPC {npcId} is not the giver for next quest {nextQuest.questName}, UI updated");
            HideQuestUI();
            MouseManager.Instance?.HideCursorAndEnableInput();
        }
    }
    #endregion

    #region Helpers, complete, claim reward, decline (giữ nguyên, có handle boss case)
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

    public void ClaimReward(QuestData quest)
    {
        StartCoroutine(ClaimRewardCoroutine(quest));
    }

    public IEnumerator ClaimRewardCoroutine(QuestData quest)
    {
        var status = GetQuestStatus(quest);
        if (quest == null || status == null || !status.isObjectiveMet || status.isCompleted)
        {
            Debug.LogWarning($"[QuestManager] Cannot claim reward for quest {(quest != null ? quest.questName : "null")}");
            yield break;
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

            DialogueManager.Instance?.StartQuestDialogue(quest, QuestDialogueType.AfterComplete, () =>
            {
                isPlayingAfterCompleteDialogue = false;
                HideQuestUI();
                MouseManager.Instance?.HideCursorAndEnableInput();
                StartCoroutine(OfferNextQuestIfGiverCoroutine(quest.giverNPCID));
            }, afterCompleteKeys, afterCompleteClipsEN, afterCompleteClipsVI);
        }
        else
        {
            isPlayingAfterCompleteDialogue = false;
            HideQuestUI();
            MouseManager.Instance?.HideCursorAndEnableInput();
            yield return StartCoroutine(OfferNextQuestIfGiverCoroutine(quest.giverNPCID));
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

        // if a boss was spawned, remove it and deactivate zone
        if (status != null)
        {
            if (status.bossInstance != null)
            {
                Destroy(status.bossInstance);
                status.bossInstance = null;
            }
            status.bossZone?.DeactivateZone();
        }

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
    #endregion

    #region Utility getters + UI show/hide (giữ nguyên)
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
    #endregion
}

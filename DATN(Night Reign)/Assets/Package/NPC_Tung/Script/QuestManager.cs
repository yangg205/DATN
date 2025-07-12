// Đầu file giữ nguyên
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("References")]
    public QuestDatabase questDatabase;
    public PlayerStats_Tung playerStats;

    [Header("UI References (Optional)")]
    [SerializeField] private GameObject _acceptButton;
    [SerializeField] private GameObject _declineButton;
    [SerializeField] private GameObject _claimRewardButton;
    [SerializeField] private GameObject _questUI;

    [SerializeField] private Dictionary<QuestData, CurrentQuestStatus> _activeQuests = new();

    private int _currentQuestIndex = 0;
    private bool _isQuestActiveInternal = false;
    private bool _isQuestCompletedInternal = false;

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
            Destroy(gameObject);
        else
            Instance = this;
    }

    private string GetLocalizedString(string tableName, string key, params object[] args)
    {
        var table = LocalizationSettings.StringDatabase.GetTable(tableName);
        if (table == null) return $"[TABLE NOT FOUND: {tableName}]";

        var entry = table.GetEntry(key);
        return entry?.GetLocalizedString(args) ?? $"[MISSING KEY: {key}]";
    }

    public QuestData GetCurrentQuest()
    {
        if (questDatabase?.quests == null || _currentQuestIndex >= questDatabase.quests.Length)
            return null;

        return questDatabase.quests[_currentQuestIndex];
    }

    public void AcceptQuest()
    {
        var quest = GetCurrentQuest();
        if (quest == null || _activeQuests.ContainsKey(quest)) return;

        var status = new CurrentQuestStatus(quest);
        _activeQuests[quest] = status;
        _isQuestActiveInternal = true;
        _isQuestCompletedInternal = false;

        // 🧭 TẠO WAYPOINT
        if (quest.hasQuestLocation && WaypointManager.Instance != null)
        {
            string id = $"Quest_{quest.questName}_{Guid.NewGuid()}";
            status.waypointId = id;

            var wp = new Waypoint(id, quest.GetQuestNameLocalized(), quest.questLocation, WaypointType.QuestLocation, quest.questLocationIcon);
            WaypointManager.Instance.AddWaypoint(wp, true);
        }

        // Hiển thị tiến độ
        if (UIManager.Instance != null)
        {
            switch (quest.questType)
            {
                case QuestType.KillEnemies:
                    UIManager.Instance.UpdateQuestProgress(0, quest.requiredKills);
                    break;
                case QuestType.CollectItem:
                    UIManager.Instance.UpdateQuestProgress(0, quest.requiredItemCount);
                    break;
                case QuestType.FindNPC:
                    UIManager.Instance.UpdateQuestProgressText(GetLocalizedString("NhiemVu", "Quest_Progress_FindNPC"));
                    break;
            }
        }

        HideQuestUI();
    }

    public void ReportKill()
    {
        var quest = GetCurrentQuest();
        if (quest == null || !_isQuestActiveInternal || _isQuestCompletedInternal || quest.questType != QuestType.KillEnemies)
            return;

        var status = GetQuestStatus(quest);
        if (status == null) return;

        status.currentProgress++;
        UIManager.Instance?.UpdateQuestProgress(status.currentProgress, status.GetRequiredProgress());

        if (status.currentProgress >= status.GetRequiredProgress())
        {
            status.isObjectiveMet = true;
            _isQuestCompletedInternal = true;
            UIManager.Instance?.HideQuestProgress();
        }
    }

    public void CheckItemCollectionProgress()
    {
        var quest = GetCurrentQuest();
        if (quest == null || !_isQuestActiveInternal || _isQuestCompletedInternal || quest.questType != QuestType.CollectItem)
            return;

        var status = GetQuestStatus(quest);
        if (status == null) return;

        if (SimpleInventory.Instance == null)
        {
            Debug.LogError("❌ SimpleInventory.Instance is NULL");
            return;
        }

        int count = SimpleInventory.Instance.GetItemCount(quest.targetItemID);
        status.currentProgress = count;
        UIManager.Instance?.UpdateQuestProgress(count, status.GetRequiredProgress());

        if (count >= status.GetRequiredProgress())
        {
            status.isObjectiveMet = true;
            _isQuestCompletedInternal = true;
            UIManager.Instance?.HideQuestProgress();
        }
    }

    public void TryCompleteQuestByTalk()
    {
        var quest = GetCurrentQuest();
        if (quest == null || !_isQuestActiveInternal || _isQuestCompletedInternal || quest.questType != QuestType.FindNPC)
            return;

        var status = GetQuestStatus(quest);
        if (status == null) return;

        if (status.currentProgress == 0)
        {
            status.currentProgress = 1;
            UIManager.Instance?.UpdateQuestProgressText(GetLocalizedString("NhiemVu", "Quest_Progress_NPCFound"));
        }

        if (status.currentProgress >= status.GetRequiredProgress())
        {
            status.isObjectiveMet = true;
            _isQuestCompletedInternal = true;
            UIManager.Instance?.HideQuestProgress();
        }
    }

    public void CompleteQuest()
    {
        var quest = GetCurrentQuest();
        if (quest == null || !_isQuestActiveInternal) return;

        var status = GetQuestStatus(quest);
        if (status == null || !status.isObjectiveMet || status.isCompleted) return;

        status.isCompleted = true;
        _isQuestActiveInternal = false;
        _isQuestCompletedInternal = false;

        UIManager.Instance?.HideQuestProgress();

        playerStats?.AddReward(quest.rewardCoin, quest.rewardExp);
        UIManager.Instance?.ShowRewardPopup(quest.rewardCoin, quest.rewardExp);

        if (quest.questType == QuestType.CollectItem)
        {
            SimpleInventory.Instance?.AddItem(quest.targetItemID, -quest.requiredItemCount);
        }

        // 🧭 XÓA WAYPOINT SAU KHI HOÀN THÀNH
        if (!string.IsNullOrEmpty(status.waypointId))
            WaypointManager.Instance?.RemoveWaypoint(status.waypointId);

        _currentQuestIndex++;
        HideQuestUI();
    }

    public void ClaimReward() => CompleteQuest();

    public void DeclineQuest()
    {
        var quest = GetCurrentQuest();
        if (quest == null || !_activeQuests.ContainsKey(quest)) return;

        var status = GetQuestStatus(quest);

        // 🧭 XÓA WAYPOINT NẾU TỪ CHỐI
        if (status != null && !string.IsNullOrEmpty(status.waypointId))
            WaypointManager.Instance?.RemoveWaypoint(status.waypointId);

        _activeQuests.Remove(quest);
        _isQuestActiveInternal = false;
        _isQuestCompletedInternal = false;

        HideQuestUI();
    }

    public bool IsQuestCompleted()
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);
        return status != null && status.isObjectiveMet && !status.isCompleted;
    }

    public bool IsQuestActive() => _isQuestActiveInternal;
    public bool IsQuestAccepted() => _isQuestActiveInternal && !IsQuestCompleted();

    public void DisplayQuestOfferUI(QuestData quest)
    {
        for (int i = 0; i < questDatabase.quests.Length; i++)
            if (questDatabase.quests[i] == quest)
                _currentQuestIndex = i;

        _isQuestActiveInternal = IsQuestActive();
        _isQuestCompletedInternal = IsQuestCompleted();

        if (_questUI != null)
        {
            _questUI.SetActive(true);
            _acceptButton?.SetActive(false);
            _declineButton?.SetActive(false);
            _claimRewardButton?.SetActive(false);

            var status = GetQuestStatus(quest);
            if (status != null)
            {
                if (status.isObjectiveMet && !status.isCompleted)
                    _claimRewardButton?.SetActive(true);
                else
                    HideQuestUI();
            }
            else
            {
                _acceptButton?.SetActive(true);
                _declineButton?.SetActive(true);
            }
        }
    }

    public CurrentQuestStatus GetQuestStatus(QuestData quest)
    {
        _activeQuests.TryGetValue(quest, out var status);
        return status;
    }

    public bool IsQuestCurrentlyActive(QuestData quest) => _activeQuests.ContainsKey(quest);

    public void HideQuestUI()
    {
        _questUI?.SetActive(false);
        _acceptButton?.SetActive(false);
        _declineButton?.SetActive(false);
        _claimRewardButton?.SetActive(false);
    }

    public void OnAcceptButtonPress() => AcceptQuest();
    public void OnDeclineButtonPress() => DeclineQuest();
    public void OnClaimRewardButtonPress() => ClaimReward();
}

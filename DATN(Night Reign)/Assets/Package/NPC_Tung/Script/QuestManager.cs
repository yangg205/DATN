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

    // Quản lý trạng thái thoại AfterComplete đang chạy
    private bool _isPlayingAfterCompleteDialogue = false;
    public bool IsPlayingAfterCompleteDialogue => _isPlayingAfterCompleteDialogue;

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
        MouseManager.Instance.HideCursorAndEnableInput();

        if (quest.hasQuestLocation && WaypointManager.Instance != null)
        {
            string id = $"QuestWaypoint_{quest.name}_{Guid.NewGuid()}";
            status.waypointId = id;
            var wp = new Waypoint(id, quest.GetQuestNameLocalized(), quest.questLocation, WaypointType.QuestLocation, quest.questLocationIcon);
            WaypointManager.Instance.AddWaypoint(wp, true);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateQuestProgressText(quest.GetQuestNameLocalized());
            UIManager.Instance.UpdateCurrentQuestObjective(quest.GetDescriptionLocalized());

            switch (quest.questType)
            {
                case QuestType.KillEnemies:
                    UIManager.Instance.UpdateQuestProgress(0, quest.requiredKills);
                    break;
                case QuestType.CollectItem:
                    UIManager.Instance.UpdateQuestProgress(0, quest.requiredItemCount);
                    break;
                case QuestType.FindNPC:
                    UIManager.Instance.UpdateQuestProgress(0, 1);
                    break;
            }
            UIManager.Instance.SetReturnToNPCInfo("", false);
        }

        HideQuestUI();
    }

    private void UpdateUIOnQuestAccept(QuestData quest)
    {
        if (UIManager.Instance == null) return;

        UIManager.Instance.UpdateQuestProgressText(quest.GetQuestNameLocalized());
        UIManager.Instance.UpdateCurrentQuestObjective(quest.GetDescriptionLocalized());

        int required = 0;
        switch (quest.questType)
        {
            case QuestType.KillEnemies:
                required = quest.requiredKills;
                break;
            case QuestType.CollectItem:
                required = quest.requiredItemCount;
                break;
            case QuestType.FindNPC:
                required = 1;
                break;
        }
        UIManager.Instance.UpdateQuestProgress(0, required);
        UIManager.Instance.SetReturnToNPCInfo("", false);
    }

    /// <summary>
    /// Ghi nhận 1 kill cho quest KillEnemies
    /// </summary>
    public void ReportKill()
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);

        if (quest == null || status == null || status.isCompleted || quest.questType != QuestType.KillEnemies) return;

        status.currentProgress++;
        UIManager.Instance?.UpdateQuestProgress(status.currentProgress, status.GetRequiredProgress());

        if (status.currentProgress >= status.GetRequiredProgress())
        {
            MarkObjectiveMet(status, quest);
        }
    }

    /// <summary>
    /// Kiểm tra tiến độ thu thập item cho quest CollectItem
    /// </summary>
    public void CheckItemCollectionProgress()
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);

        if (quest == null || status == null || status.isCompleted || quest.questType != QuestType.CollectItem) return;
        if (SimpleInventory.Instance == null)
        {
            Debug.LogError("SimpleInventory.Instance is NULL");
            return;
        }

        int count = SimpleInventory.Instance.GetItemCount(quest.targetItemID);
        status.currentProgress = count;
        UIManager.Instance?.UpdateQuestProgress(count, status.GetRequiredProgress());

        if (count >= status.GetRequiredProgress())
        {
            MarkObjectiveMet(status, quest);
        }
    }

    /// <summary>
    /// Cố gắng hoàn thành quest bằng cách nói chuyện với NPC cho quest FindNPC
    /// </summary>
    public void TryCompleteQuestByTalk()
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);

        if (quest == null || status == null || status.isCompleted || quest.questType != QuestType.FindNPC) return;

        if (status.currentProgress == 0) status.currentProgress = 1;

        if (status.currentProgress >= status.GetRequiredProgress())
        {
            status.isObjectiveMet = true;
            UIManager.Instance?.UpdateCurrentQuestObjective(GetLocalizedString("NhiemVu", "Objective_Status_Completed_FindNPC"));
            UIManager.Instance?.HideQuestProgress();
            UIManager.Instance?.SetReturnToNPCInfo(quest.GetGiverNPCNameLocalized(), true);
            UIManager.Instance?.ShowNotice(GetLocalizedString("NhiemVu", "Quest_Objective_Met_Notice"), 2.5f);

            RemoveWaypoint(status);
            CreateReturnWaypoint(status, quest);
        }
    }

    private void MarkObjectiveMet(CurrentQuestStatus status, QuestData quest)
    {
        status.isObjectiveMet = true;

        UIManager.Instance?.UpdateCurrentQuestObjective(GetLocalizedString("NhiemVu", "Objective_Status_Completed"));
        UIManager.Instance?.SetReturnToNPCInfo(quest.GetGiverNPCNameLocalized(), true);
        UIManager.Instance?.ShowNotice(GetLocalizedString("NhiemVu", "Quest_Objective_Met_Notice"), 2.5f);

        RemoveWaypoint(status);
        CreateReturnWaypoint(status, quest);
    }

    private void RemoveWaypoint(CurrentQuestStatus status)
    {
        if (!string.IsNullOrEmpty(status.waypointId) && WaypointManager.Instance != null)
        {
            WaypointManager.Instance.RemoveWaypoint(status.waypointId);
            status.waypointId = null;
        }
    }

    private void CreateReturnWaypoint(CurrentQuestStatus status, QuestData quest)
    {
        if (WaypointManager.Instance != null && !status.isCompleted)
        {
            string returnWaypointId = $"ReturnToNPC_{quest.giverNPCID}_{quest.name}";
            var returnWp = new Waypoint(returnWaypointId, GetLocalizedString("NhiemVu", "ReturnToNPC_Prefix", quest.GetGiverNPCNameLocalized()), quest.giverNPCTransform, WaypointType.QuestLocation, null);
            WaypointManager.Instance.AddWaypoint(returnWp, true);
            status.waypointId = returnWaypointId;
        }
    }

    /// <summary>
    /// Hoàn thành nhiệm vụ, trả thưởng, xóa waypoint, cập nhật trạng thái.
    /// </summary>
    public void CompleteQuest()
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);

        if (quest == null || status == null || !status.isObjectiveMet || status.isCompleted) return;

        status.isCompleted = true;

        UIManager.Instance?.HideQuestProgress();
        UIManager.Instance?.SetReturnToNPCInfo("", false);

        playerStats?.AddReward(quest.rewardCoin, quest.rewardExp);
        UIManager.Instance?.ShowRewardPopup(quest.rewardCoin, quest.rewardExp);

        if (quest.questType == QuestType.CollectItem)
        {
            SimpleInventory.Instance?.AddItem(quest.targetItemID, -quest.requiredItemCount);
        }

        if (!string.IsNullOrEmpty(status.waypointId))
            WaypointManager.Instance?.RemoveWaypoint(status.waypointId);

        _activeQuests.Remove(quest);
        _currentQuestIndex++;

        // KHÔNG TẮT UI Ở ĐÂY THEO YÊU CẦU
    }

    public void ClaimReward(Action onCompleteCallback = null)
    {
        var quest = GetCurrentQuest();
        if (quest == null || !(GetQuestStatus(quest)?.isObjectiveMet ?? false)) return;

        CompleteQuest();
        _isPlayingAfterCompleteDialogue = true;

        if (_questUI != null)
            _questUI.SetActive(true);

        HideAllActionButtons();

        string[] afterCompleteKeys = quest.GetDialogueKeys(QuestDialogueType.AfterComplete);
        AudioClip[] afterCompleteClips = quest.GetDialogueVoiceClips(QuestDialogueType.AfterComplete);

        DialogueManager.Instance.StartDialogue(afterCompleteKeys, afterCompleteClips, () =>
        {
            _isPlayingAfterCompleteDialogue = false;
            onCompleteCallback?.Invoke();
            HideQuestUI();
            MouseManager.Instance.HideCursorAndEnableInput();
        });
    }

    private void OnAfterCompleteDialogueFinished()
    {
        _isPlayingAfterCompleteDialogue = false;

        HideQuestUI();
        MouseManager.Instance.HideCursorAndEnableInput();
    }

    public void DeclineQuest()
    {
        var quest = GetCurrentQuest();
        if (quest == null || !_activeQuests.ContainsKey(quest)) return;

        var status = GetQuestStatus(quest);

        if (status != null && !string.IsNullOrEmpty(status.waypointId))
            WaypointManager.Instance?.RemoveWaypoint(status.waypointId);

        _activeQuests.Remove(quest);
        UIManager.Instance?.HideQuestProgress();
        UIManager.Instance?.SetReturnToNPCInfo("", false);

        HideQuestUI();
    }

    public bool IsQuestObjectiveMet(QuestData quest)
    {
        _activeQuests.TryGetValue(quest, out var status);
        return status != null && status.isObjectiveMet && !status.isCompleted;
    }

    public bool IsQuestTrulyCompleted(QuestData quest)
    {
        _activeQuests.TryGetValue(quest, out var status);
        return status != null && status.isCompleted;
    }

    public bool IsQuestAccepted(QuestData quest)
    {
        return _activeQuests.ContainsKey(quest) && !IsQuestTrulyCompleted(quest);
    }

    public void DisplayQuestOfferUI(QuestData quest)
    {
        for (int i = 0; i < questDatabase.quests.Length; i++)
        {
            if (questDatabase.quests[i] == quest)
            {
                _currentQuestIndex = i;
                break;
            }
        }

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
                {
                    _claimRewardButton?.SetActive(true);
                }
                else if (!status.isObjectiveMet && !status.isCompleted)
                {
                    _acceptButton?.SetActive(false);
                    _declineButton?.SetActive(false);
                    _claimRewardButton?.SetActive(false);
                }
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
        if (_isPlayingAfterCompleteDialogue)
        {
            // Tuyệt đối KHÔNG được tắt UI khi đang chạy AfterComplete thoại
            return;
        }

        _questUI?.SetActive(false);
        _acceptButton?.SetActive(false);
        _declineButton?.SetActive(false);
        _claimRewardButton?.SetActive(false);
    }

    public void HideAllActionButtons()
    {
        _acceptButton?.SetActive(false);
        _declineButton?.SetActive(false);
        _claimRewardButton?.SetActive(false);
    }

    public void OnAcceptButtonPress() => AcceptQuest();
    public void OnDeclineButtonPress() => DeclineQuest();
    public void OnClaimRewardButtonPress() => ClaimReward();

}

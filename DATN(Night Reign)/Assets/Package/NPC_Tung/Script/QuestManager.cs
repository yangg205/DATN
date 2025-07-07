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

    [SerializeField] private Dictionary<QuestData, CurrentQuestStatus> _activeQuests = new Dictionary<QuestData, CurrentQuestStatus>();

    private int _currentQuestIndex = 0;

    private bool _isQuestActiveInternal = false;
    private bool _isQuestCompletedInternal = false;

    public class CurrentQuestStatus
    {
        public QuestData questData;
        public int currentProgress;
        public bool isObjectiveMet;
        public bool isCompleted;

        public CurrentQuestStatus(QuestData data)
        {
            questData = data;
            currentProgress = 0;
            isObjectiveMet = false;
            isCompleted = false;
        }

        public int GetRequiredProgress()
        {
            switch (questData.questType)
            {
                case QuestType.KillEnemies:
                    return questData.requiredKills;
                case QuestType.CollectItem:
                    return questData.requiredItemCount;
                case QuestType.FindNPC:
                    return 1;
                default:
                    return 0;
            }
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Bỏ comment nếu bạn muốn nó tồn tại qua các cảnh
        }
    }

    private string GetLocalizedString(string tableName, string key, params object[] arguments)
    {
        var table = LocalizationSettings.StringDatabase.GetTable(tableName);
        if (table == null)
        {
            Debug.LogError($"❌ Bảng '{tableName}' không tồn tại. (QuestManager)");
            return $"[TABLE NOT FOUND: {tableName}]";
        }

        var entry = table.GetEntry(key);
        if (entry == null)
        {
            Debug.LogError($"❌ Key '{key}' không có trong bảng '{tableName}' (QuestManager)");
            return $"[MISSING KEY: {key}]";
        }

        return entry.GetLocalizedString(arguments) ?? $"[ERROR LOCALIZING: {key}]";
    }

    public QuestData GetCurrentQuest()
    {
        if (questDatabase == null || questDatabase.quests == null || _currentQuestIndex >= questDatabase.quests.Length)
        {
            return null;
        }
        return questDatabase.quests[_currentQuestIndex];
    }

    public void AcceptQuest()
    {
        QuestData quest = GetCurrentQuest();
        if (quest == null) return;
        if (_activeQuests.ContainsKey(quest)) return;

        _activeQuests.Add(quest, new CurrentQuestStatus(quest));
        _isQuestActiveInternal = true;
        _isQuestCompletedInternal = false;

        if (UIManager.Instance != null)
        {
            if (quest.questType == QuestType.KillEnemies)
            {
                UIManager.Instance.UpdateQuestProgress(0, quest.requiredKills);
            }
            else if (quest.questType == QuestType.FindNPC)
            {
                // Sử dụng key "Quest_Progress_FindNPC"
                string findNPCProgressText = GetLocalizedString("NhiemVu", "Quest_Progress_FindNPC");
                UIManager.Instance.UpdateQuestProgressText(findNPCProgressText);
            }
            else if (quest.questType == QuestType.CollectItem)
            {
                UIManager.Instance.UpdateQuestProgress(0, quest.requiredItemCount);
            }
        }
        HideQuestUI();
    }

    public void ReportKill()
    {
        QuestData quest = GetCurrentQuest();
        if (quest == null || !_isQuestActiveInternal || _isQuestCompletedInternal || quest.questType != QuestType.KillEnemies)
        {
            return;
        }

        CurrentQuestStatus status = GetQuestStatus(quest);
        if (status == null) return;

        status.currentProgress++;
        if (UIManager.Instance != null) UIManager.Instance.UpdateQuestProgress(status.currentProgress, status.GetRequiredProgress());

        if (status.currentProgress >= status.GetRequiredProgress())
        {
            status.isObjectiveMet = true;
            _isQuestCompletedInternal = true;
            if (UIManager.Instance != null) UIManager.Instance.HideQuestProgress();
        }
    }

    public void CheckItemCollectionProgress()
    {
        QuestData quest = GetCurrentQuest();
        if (quest == null || !_isQuestActiveInternal || _isQuestCompletedInternal || quest.questType != QuestType.CollectItem)
        {
            return;
        }

        CurrentQuestStatus status = GetQuestStatus(quest);
        if (status == null) return;

        if (SimpleInventory.Instance == null)
        {
            Debug.LogError("LỖI: SimpleInventory.Instance là NULL. Không thể kiểm tra tiến độ vật phẩm. Hãy đảm bảo SimpleInventory có trong cảnh và được setup.");
            return;
        }

        int currentInventoryCount = SimpleInventory.Instance.GetItemCount(quest.targetItemID);
        status.currentProgress = currentInventoryCount;
        if (UIManager.Instance != null) UIManager.Instance.UpdateQuestProgress(status.currentProgress, status.GetRequiredProgress());

        if (status.currentProgress >= status.GetRequiredProgress())
        {
            status.isObjectiveMet = true;
            _isQuestCompletedInternal = true;
            if (UIManager.Instance != null) UIManager.Instance.HideQuestProgress();
        }
    }

    public void TryCompleteQuestByTalk()
    {
        QuestData quest = GetCurrentQuest();
        if (quest == null || !_isQuestActiveInternal || _isQuestCompletedInternal || quest.questType != QuestType.FindNPC)
        {
            return;
        }

        CurrentQuestStatus status = GetQuestStatus(quest);
        if (status == null) return;

        if (status.currentProgress == 0) // Chỉ hoàn thành nếu chưa hoàn thành
        {
            status.currentProgress = 1;
            if (UIManager.Instance != null)
            {
                // Sử dụng key "Quest_Progress_NPCFound"
                string npcFoundProgressText = GetLocalizedString("NhiemVu", "Quest_Progress_NPCFound");
                UIManager.Instance.UpdateQuestProgressText(npcFoundProgressText);
            }

            if (status.currentProgress >= status.GetRequiredProgress())
            {
                status.isObjectiveMet = true;
                _isQuestCompletedInternal = true;
                if (UIManager.Instance != null) UIManager.Instance.HideQuestProgress();
            }
        }
    }

    public void CompleteQuest()
    {
        QuestData quest = GetCurrentQuest();
        if (quest == null || !_isQuestActiveInternal) return;

        CurrentQuestStatus status = GetQuestStatus(quest);
        if (status == null) return;

        if (status.isObjectiveMet && !status.isCompleted)
        {
            status.isCompleted = true;
            _isQuestActiveInternal = false;
            _isQuestCompletedInternal = true;

            if (UIManager.Instance != null) UIManager.Instance.HideQuestProgress();

            if (playerStats == null)
            {
                Debug.LogError("LỖI: Tham chiếu PlayerStats trong QuestManager là NULL! Không thể thêm phần thưởng. Hãy gán nó trong Inspector.");
            }
            else
            {
                // Gọi ShowRewardPopup với coin và exp
                playerStats.AddReward(quest.rewardCoin, quest.rewardExp);
                if (UIManager.Instance != null) UIManager.Instance.ShowRewardPopup(quest.rewardCoin, quest.rewardExp);
            }

            if (quest.questType == QuestType.CollectItem && SimpleInventory.Instance != null)
            {
                SimpleInventory.Instance.AddItem(quest.targetItemID, -quest.requiredItemCount); // Trừ vật phẩm
            }

            _currentQuestIndex++; // Chuyển sang nhiệm vụ tiếp theo
            _isQuestActiveInternal = false; // Reset trạng thái để chuẩn bị cho quest mới
            _isQuestCompletedInternal = false; // Reset trạng thái
            HideQuestUI();
        }
    }

    public void ClaimReward()
    {
        CompleteQuest();
    }

    public void DeclineQuest()
    {
        QuestData quest = GetCurrentQuest();
        if (quest == null) return;
        if (_activeQuests.ContainsKey(quest))
        {
            _activeQuests.Remove(quest);
        }

        _isQuestActiveInternal = false;
        _isQuestCompletedInternal = false;

        HideQuestUI();
    }

    public bool IsQuestCompleted()
    {
        QuestData quest = GetCurrentQuest();
        if (quest == null) return false;

        CurrentQuestStatus status = GetQuestStatus(quest);
        // Trả về true nếu mục tiêu đã đạt được nhưng chưa nhận thưởng
        return status != null && status.isObjectiveMet && !status.isCompleted;
    }

    public bool IsQuestActive()
    {
        return _isQuestActiveInternal;
    }

    public bool IsQuestAccepted()
    {
        return _isQuestActiveInternal && !IsQuestCompleted();
    }

    public void DisplayQuestOfferUI(QuestData quest)
    {
        // Đặt _currentQuestIndex đến quest đang được tương tác
        for (int i = 0; i < questDatabase.quests.Length; i++)
        {
            if (questDatabase.quests[i] == quest)
            {
                _currentQuestIndex = i;
                break;
            }
        }

        _isQuestActiveInternal = IsQuestActive();
        _isQuestCompletedInternal = IsQuestCompleted();

        if (_questUI != null)
        {
            _questUI.SetActive(true);

            _acceptButton?.SetActive(false);
            _declineButton?.SetActive(false);
            _claimRewardButton?.SetActive(false);

            CurrentQuestStatus status = GetQuestStatus(quest);

            if (status != null)
            {
                if (status.isObjectiveMet && !status.isCompleted)
                {
                    _claimRewardButton?.SetActive(true);
                }
                else if (status.isCompleted)
                {
                    HideQuestUI(); // Đã hoàn thành và nhận thưởng, ẩn UI
                }
                else
                {
                    HideQuestUI(); // Quest đang hoạt động nhưng chưa hoàn thành mục tiêu, ẩn UI (thoại đã xử lý)
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
        _activeQuests.TryGetValue(quest, out CurrentQuestStatus status);
        return status;
    }

    public bool IsQuestCurrentlyActive(QuestData quest)
    {
        return _activeQuests.ContainsKey(quest);
    }

    public void HideQuestUI()
    {
        if (_questUI != null)
        {
            _questUI.SetActive(false);
            _acceptButton?.gameObject.SetActive(false);
            _declineButton?.gameObject.SetActive(false);
            _claimRewardButton?.gameObject.SetActive(false);
        }
    }

    public void OnAcceptButtonPress()
    {
        AcceptQuest();
    }

    public void OnDeclineButtonPress()
    {
        DeclineQuest();
    }

    public void OnClaimRewardButtonPress()
    {
        ClaimReward();
    }
}
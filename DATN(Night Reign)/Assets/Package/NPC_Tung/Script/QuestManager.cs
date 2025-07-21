using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Threading.Tasks;

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

    public int _currentQuestIndex = 0;

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

    public QuestData GetCurrentQuest()
    {
        if (questDatabase?.quests == null || _currentQuestIndex >= questDatabase.quests.Length)
            return null;

        return questDatabase.quests[_currentQuestIndex];
    }

    public async void AcceptQuest()
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
            string questNameLocalized = await quest.GetQuestNameLocalizedAsync();
            var wp = new Waypoint(id, questNameLocalized, quest.questLocation, WaypointType.QuestLocation, quest.questLocationIcon);
            WaypointManager.Instance.AddWaypoint(wp, true);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateQuestProgressText(await quest.GetQuestNameLocalizedAsync());
            UIManager.Instance.UpdateCurrentQuestObjective(await quest.GetDescriptionLocalizedAsync());

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
            UIManager.Instance.SetReturnToNPCInfo(await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Empty_Return_Info_Key"), false);
        }

        HideQuestUI();
    }

    private async void UpdateUIOnQuestAccept(QuestData quest)
    {
        if (UIManager.Instance == null) return;

        UIManager.Instance.UpdateQuestProgressText(await quest.GetQuestNameLocalizedAsync());
        UIManager.Instance.UpdateCurrentQuestObjective(await quest.GetDescriptionLocalizedAsync());

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
        UIManager.Instance.SetReturnToNPCInfo(await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Empty_Return_Info_Key"), false);
    }

    public void ReportKill()
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);

        if (quest == null || status == null || status.isCompleted || quest.questType != QuestType.KillEnemies) return;

        status.currentProgress++;
        Debug.Log($"ReportKill called: {status.currentProgress}/{status.GetRequiredProgress()}"); // 👈 Dòng này

        UIManager.Instance?.UpdateQuestProgress(status.currentProgress, status.GetRequiredProgress());

        if (status.currentProgress >= status.GetRequiredProgress())
        {
            MarkObjectiveMet(status, quest);
        }
    }


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

    public async void TryCompleteQuestByTalk()
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);

        if (quest == null || status == null || status.isCompleted || quest.questType != QuestType.FindNPC) return;

        if (status.currentProgress == 0) status.currentProgress = 1;

        if (status.currentProgress >= status.GetRequiredProgress())
        {
            status.isObjectiveMet = true;
            UIManager.Instance?.UpdateCurrentQuestObjective(await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Objective_Status_Completed_FindNPC_Key"));
            UIManager.Instance?.HideQuestProgress();
            UIManager.Instance?.SetReturnToNPCInfo(await quest.GetGiverNPCNameLocalizedAsync(), true);
            UIManager.Instance?.ShowNotice(await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Quest_Objective_Met_Notice_Key"), 2.5f);

            RemoveWaypoint(status);
            CreateReturnWaypoint(status, quest);
        }
    }

    private async void MarkObjectiveMet(CurrentQuestStatus status, QuestData quest)
    {
        status.isObjectiveMet = true;

        UIManager.Instance?.UpdateCurrentQuestObjective(await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Objective_Status_Completed_Key"));
        UIManager.Instance?.SetReturnToNPCInfo(await quest.GetGiverNPCNameLocalizedAsync(), true);
        UIManager.Instance?.ShowNotice(await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Quest_Objective_Met_Notice_Key"), 2.5f);

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

    private async void CreateReturnWaypoint(CurrentQuestStatus status, QuestData quest)
    {
        if (WaypointManager.Instance != null && !status.isCompleted)
        {
            string returnWaypointId = $"ReturnToNPC_{quest.giverNPCID}_{quest.name}";
            string returnToNPCName = await quest.GetGiverNPCNameLocalizedAsync();
            var returnWp = new Waypoint(returnWaypointId, await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "ReturnToNPC_Prefix_Key", returnToNPCName), quest.giverNPCTransform, WaypointType.QuestLocation, null);
            WaypointManager.Instance.AddWaypoint(returnWp, true);
            status.waypointId = returnWaypointId;
        }
    }

    // Thêm phương thức này để kiểm tra quest tiếp theo có phải là FindNPC không
    public bool IsNextQuestFindNPC()
    {
        if (questDatabase == null || questDatabase.quests == null) return false;
        if (_currentQuestIndex + 1 >= questDatabase.quests.Length) return false;

        return questDatabase.quests[_currentQuestIndex + 1].questType == QuestType.FindNPC;
    }

    // Sửa lại phương thức CompleteQuest
    // Sửa lại phương thức CompleteQuest
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

        // Luôn tăng currentQuestIndex, không phân biệt loại quest
        _currentQuestIndex++;

        // Nếu quest tiếp theo là FindNPC, tự động nhận
        if (IsNextQuestFindNPC())
        {
            var nextQuest = questDatabase.quests[_currentQuestIndex];
            if (nextQuest != null)
            {
                AcceptQuest();
            }
        }
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

        // SỬA LỖI TẠI ĐÂY:
        // Gọi GetDialogueVoiceClips hai lần để truyền mảng cho cả EN và VI.
        // QuestData của bạn tự động chọn đúng mảng dựa trên ngôn ngữ hiện tại,
        // nên cả hai tham số này sẽ nhận được cùng một mảng (mảng của ngôn ngữ hiện tại).
        AudioClip[] afterCompleteClipsEN = quest.GetDialogueVoiceClips(QuestDialogueType.AfterComplete); // Mảng cho EN
        AudioClip[] afterCompleteClipsVI = quest.GetDialogueVoiceClips(QuestDialogueType.AfterComplete); // Mảng cho VI

        DialogueManager.Instance.StartDialogue(
            afterCompleteKeys,
            afterCompleteClipsEN,  // Tham số voiceClipsEN
            afterCompleteClipsVI,  // Tham số voiceClipsVI
            () => // Tham số Action onDialogueEnd (biểu thức lambda của bạn)
            {
                _isPlayingAfterCompleteDialogue = false;
                onCompleteCallback?.Invoke();
                //HideQuestUI();
                MouseManager.Instance.HideCursorAndEnableInput(); // Đảm bảo bạn có lớp MouseManager
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
            _acceptButton?.gameObject.SetActive(false);
            _declineButton?.gameObject.SetActive(false);
            _claimRewardButton?.gameObject.SetActive(false);

            var status = GetQuestStatus(quest);

            if (status != null)
            {
                if (status.isObjectiveMet && !status.isCompleted)
                {
                    _claimRewardButton?.gameObject.SetActive(true);
                }
                else if (!status.isObjectiveMet && !status.isCompleted)
                {
                    _acceptButton?.gameObject.SetActive(false);
                    _declineButton?.gameObject.SetActive(false);
                    _claimRewardButton?.gameObject.SetActive(false);
                }
            }
            else
            {
                _acceptButton?.gameObject.SetActive(true);
                _declineButton?.gameObject.SetActive(true);
            }
        }
        if (!IsQuestUnlocked(quest))
        {
            UIManager.Instance?.ShowNotice("Nhiệm vụ này chưa thể nhận. Hãy hoàn thành nhiệm vụ trước đó!", 2.5f);
            return;
        }
    }

    public CurrentQuestStatus GetQuestStatus(QuestData quest)
    {
        _activeQuests.TryGetValue(quest, out var status);
        return status;
    }

    public bool IsQuestCurrentlyActive(QuestData quest) => _activeQuests.ContainsKey(quest);
    public bool IsQuestUnlocked(QuestData quest)
    {
        if (string.IsNullOrEmpty(quest.prerequisiteQuestName)) return true; // Không có điều kiện -> unlocked

        foreach (var completed in _activeQuests)
        {
            if (completed.Key.questName == quest.prerequisiteQuestName && completed.Value.isCompleted)
                return true;
        }

        return false; // Chưa hoàn thành prerequisite
    }

    public void HideQuestUI()
    {
        if (_isPlayingAfterCompleteDialogue)
        {
            return;
        }

        _questUI?.SetActive(false);
        _acceptButton?.gameObject.SetActive(false);
        _declineButton?.gameObject.SetActive(false);
        _claimRewardButton?.gameObject.SetActive(false);
    }

    public void HideAllActionButtons()
    {
        _acceptButton?.gameObject.SetActive(false);
        _declineButton?.gameObject.SetActive(false);
        _claimRewardButton?.gameObject.SetActive(false);
    }

    public void OnAcceptButtonPress() => AcceptQuest();
    public void OnDeclineButtonPress() => DeclineQuest();
    public void OnClaimRewardButtonPress() => ClaimReward();
    public void SetCurrentQuestIndex(int index)
    {
        if (index >= 0 && index < (questDatabase?.quests?.Length ?? 0))
        {
            _currentQuestIndex = index;
        }
        else
        {
            Debug.LogWarning("SetCurrentQuestIndex: Index out of range!");
        }
    }

}
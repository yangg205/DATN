using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("References")]
    public QuestDatabase questDatabase;
    public PlayerStats_Tung playerStats;
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
            Destroy(gameObject);
        else
            Instance = this;
    }

    public QuestData GetCurrentQuest()
    {
        if (questDatabase == null || questDatabase.quests == null || currentQuestIndex >= questDatabase.quests.Length)
            return null;

        return questDatabase.quests[currentQuestIndex];
    }

    public async void AcceptQuest()
    {
        var quest = GetCurrentQuest();
        if (quest == null || activeQuests.ContainsKey(quest)) return;

        var status = new CurrentQuestStatus(quest);
        activeQuests.Add(quest, status);

        // Setup waypoint if needed
        if (quest.hasQuestLocation && waypointManager != null)
        {
            string waypointId = $"QuestWaypoint_{quest.name}_{System.Guid.NewGuid()}";
            status.waypointId = waypointId;
            string questName = await quest.GetQuestNameLocalizedAsync();
            waypointManager.AddWaypoint(new Waypoint(
                waypointId,
                questName,
                quest.questLocation,
                WaypointType.QuestLocation,
                quest.questLocationIcon
            ), true);
        }

        // Update and show quest UI
        await UpdateQuestUI(quest);
        ShowQuestUI(); // Hiển thị giao diện chi tiết nhiệm vụ
    }

    private async Task UpdateQuestUI(QuestData quest)
    {
        if (uiManager == null) return;

        string questName = await quest.GetQuestNameLocalizedAsync();
        string description = await quest.GetDescriptionLocalizedAsync();

        uiManager.UpdateQuestProgressText(questName);
        uiManager.UpdateCurrentQuestObjective(description);

        switch (quest.questType)
        {
            case QuestType.KillEnemies:
                uiManager.UpdateQuestProgress(0, quest.requiredKills);
                break;
            case QuestType.CollectItem:
                uiManager.UpdateQuestProgress(0, quest.requiredItemCount);
                break;
            case QuestType.FindNPC:
                uiManager.UpdateQuestProgress(0, 1);
                break;
        }

        uiManager.SetReturnToNPCInfo("", false);
    }

    public void ReportKill()
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);

        if (quest == null || status == null || status.isCompleted || quest.questType != QuestType.KillEnemies) return;

        status.currentProgress++;
        uiManager?.UpdateQuestProgress(status.currentProgress, status.GetRequiredProgress());

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

        int count = SimpleInventory.Instance.GetItemCount(quest.targetItemID);
        status.currentProgress = count;
        uiManager?.UpdateQuestProgress(count, status.GetRequiredProgress());

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

        status.currentProgress = 1;

        if (status.currentProgress >= status.GetRequiredProgress())
        {
            status.isObjectiveMet = true;
            uiManager?.UpdateCurrentQuestObjective(await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Objective_Status_Completed_FindNPC_Key"));
            uiManager?.HideQuestProgress();
            uiManager?.SetReturnToNPCInfo(await quest.GetGiverNPCNameLocalizedAsync(), true);
            uiManager?.ShowNotice(await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Quest_Objective_Met_Notice_Key"), 2.5f);

            RemoveWaypoint(status);
            CreateReturnWaypoint(status, quest);
        }
    }

    private async void MarkObjectiveMet(CurrentQuestStatus status, QuestData quest)
    {
        status.isObjectiveMet = true;

        uiManager?.UpdateCurrentQuestObjective(await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Objective_Status_Completed_Key"));
        uiManager?.SetReturnToNPCInfo(await quest.GetGiverNPCNameLocalizedAsync(), true);
        uiManager?.ShowNotice(await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Quest_Objective_Met_Notice_Key"), 2.5f);

        RemoveWaypoint(status);
        CreateReturnWaypoint(status, quest);
    }

    private void RemoveWaypoint(CurrentQuestStatus status)
    {
        if (!string.IsNullOrEmpty(status.waypointId) && waypointManager != null)
        {
            waypointManager.RemoveWaypoint(status.waypointId);
            status.waypointId = null;
        }
    }

    private async void CreateReturnWaypoint(CurrentQuestStatus status, QuestData quest)
    {
        if (waypointManager != null && !status.isCompleted)
        {
            string returnWaypointId = $"ReturnToNPC_{quest.giverNPCID}_{quest.name}";
            string returnToNPCName = await quest.GetGiverNPCNameLocalizedAsync();
            string returnText = await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "ReturnToNPC_Prefix_Key", returnToNPCName);

            var returnWp = new Waypoint(
                returnWaypointId,
                returnText,
                quest.giverNPCTransform,
                WaypointType.QuestLocation,
                null
            );

            waypointManager.AddWaypoint(returnWp, true);
            status.waypointId = returnWaypointId;
        }
    }

    public void CompleteQuest()
    {
        var quest = GetCurrentQuest();
        var status = GetQuestStatus(quest);

        if (quest == null || status == null || !status.isObjectiveMet || status.isCompleted) return;

        status.isCompleted = true;

        uiManager?.HideQuestProgress();
        uiManager?.SetReturnToNPCInfo("", false);

        playerStats?.AddReward(quest.rewardCoin, quest.rewardExp);
        uiManager?.ShowRewardPopup(quest.rewardCoin, quest.rewardExp);

        if (quest.questType == QuestType.CollectItem)
        {
            SimpleInventory.Instance?.AddItem(quest.targetItemID, -quest.requiredItemCount);
        }

        if (!string.IsNullOrEmpty(status.waypointId))
            waypointManager?.RemoveWaypoint(status.waypointId);

        activeQuests.Remove(quest);
        currentQuestIndex++;

        // Auto accept next quest if it's FindNPC type
        if (IsNextQuestFindNPC())
        {
            currentQuestIndex = Mathf.Clamp(currentQuestIndex, 0, questDatabase.quests.Length - 1);
            AcceptQuest();
        }
    }

    public bool IsNextQuestFindNPC()
    {
        if (questDatabase == null || questDatabase.quests == null) return false;
        if (currentQuestIndex >= questDatabase.quests.Length) return false;

        return questDatabase.quests[currentQuestIndex].questType == QuestType.FindNPC;
    }

    public void ClaimReward()
    {
        var quest = GetCurrentQuest();
        if (quest == null || !(GetQuestStatus(quest)?.isObjectiveMet ?? false)) return;

        CompleteQuest();
        isPlayingAfterCompleteDialogue = true;

        ShowQuestUI();
        HideAllActionButtons();

        string[] afterCompleteKeys = quest.GetDialogueKeys(QuestDialogueType.AfterComplete);
        AudioClip[] afterCompleteClips = quest.GetDialogueVoiceClips(QuestDialogueType.AfterComplete);

        DialogueManager.Instance.StartDialogue(
            afterCompleteKeys,
            afterCompleteClips,
            afterCompleteClips,
            () => {
                isPlayingAfterCompleteDialogue = false;
                HideQuestUI();
            });
    }

    public void DeclineQuest()
    {
        var quest = GetCurrentQuest();
        if (quest == null || !activeQuests.ContainsKey(quest)) return;

        var status = GetQuestStatus(quest);

        if (status != null && !string.IsNullOrEmpty(status.waypointId))
            waypointManager?.RemoveWaypoint(status.waypointId);

        activeQuests.Remove(quest);
        uiManager?.HideQuestProgress();
        uiManager?.SetReturnToNPCInfo("", false);

        HideQuestUI();
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
        return GetQuestStatus(quest)?.isCompleted ?? false;
    }

    public bool IsQuestAccepted(QuestData quest)
    {
        return activeQuests.ContainsKey(quest) && !IsQuestCompleted(quest);
    }

    public void ShowQuestUI()
    {
        questUI?.SetActive(true);
        MouseManager.Instance?.ShowCursorAndDisableInput();
    }

    public void HideQuestUI()
    {
        if (isPlayingAfterCompleteDialogue) return;
        questUI?.SetActive(false);
        MouseManager.Instance?.HideCursorAndEnableInput();
    }

    public void HideAllActionButtons()
    {
        acceptButton?.SetActive(false);
        declineButton?.SetActive(false);
        claimRewardButton?.SetActive(false);
    }

    public void SetCurrentQuestIndex(int index)
    {
        if (index >= 0 && index < (questDatabase?.quests?.Length ?? 0))
        {
            currentQuestIndex = index;
        }
    }
    public bool IsQuestUnlocked(QuestData quest)
    {
        if (questDatabase == null || questDatabase.quests == null) return false;

        // Nếu nhiệm vụ là nhiệm vụ đầu tiên
        if (quest == questDatabase.quests[0]) return true;

        // Tìm index của quest này
        for (int i = 1; i < questDatabase.quests.Length; i++)
        {
            if (questDatabase.quests[i] == quest)
            {
                var previousQuest = questDatabase.quests[i - 1];
                return IsQuestCompleted(previousQuest);
            }
        }

        return false;
    }
}
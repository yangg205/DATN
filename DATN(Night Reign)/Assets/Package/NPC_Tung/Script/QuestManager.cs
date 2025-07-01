using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public QuestDatabase questDatabase;
    public PlayerStats_Tung playerStats;

    private int currentQuestIndex = 0;
    private int currentKills = 0;
    private int currentItemCount = 0;
    private bool isQuestActive = false;
    private bool isQuestCompleted = false;

    public GameObject acceptButton, declineButton, claimRewardButton;
    public GameObject questUI;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (isQuestActive)
        {
            var quest = GetCurrentQuest();
            if (quest != null && quest.questType == QuestType.KillEnemies && Input.GetKeyDown(KeyCode.K))
            {
                ReportKill();
            }
        }
    }

    public QuestData GetCurrentQuest() =>
        (questDatabase && currentQuestIndex < questDatabase.quests.Length)
        ? questDatabase.quests[currentQuestIndex]
        : null;

    public void AcceptQuest()
    {
        var quest = GetCurrentQuest();
        if (quest == null || isQuestActive || isQuestCompleted) return;

        isQuestActive = true;
        isQuestCompleted = false;
        currentKills = 0;
        currentItemCount = 0;

        if (quest.questType == QuestType.KillEnemies)
        {
            UIManager.Instance.UpdateQuestProgress(0, quest.requiredKills);
        }
        else if (quest.questType == QuestType.FindNPC)
        {
            UIManager.Instance.UpdateQuestProgressText("Tìm và nói chuyện với NPC mục tiêu");
        }
        else if (quest.questType == QuestType.CollectItem)
        {
            UIManager.Instance.UpdateQuestProgress(0, quest.requiredItemCount);
        }

        HideQuestUI();
    }

    public void ReportKill()
    {
        var quest = GetCurrentQuest();
        if (!isQuestActive || isQuestCompleted || quest.questType != QuestType.KillEnemies) return;

        currentKills++;
        UIManager.Instance.UpdateQuestProgress(currentKills, quest.requiredKills);

        if (currentKills >= quest.requiredKills)
            CompleteQuest();
    }

    public void CheckItemCollectionProgress()
    {
        var quest = GetCurrentQuest();
        if (!isQuestActive || isQuestCompleted || quest.questType != QuestType.CollectItem) return;

        int current = SimpleInventory.Instance.GetItemCount(quest.targetItemID);
        currentItemCount = current;
        UIManager.Instance.UpdateQuestProgress(currentItemCount, quest.requiredItemCount);

        if (currentItemCount >= quest.requiredItemCount)
            CompleteQuest();
    }

    public void TryCompleteQuestByTalk()
    {
        var quest = GetCurrentQuest();
        if (quest != null && isQuestActive && !isQuestCompleted && quest.questType == QuestType.FindNPC)
            CompleteQuest();
    }

    public void CompleteQuest()
    {
        isQuestActive = false;
        isQuestCompleted = true;

        UIManager.Instance.HideQuestProgress();

        acceptButton?.SetActive(false);
        declineButton?.SetActive(false);
        claimRewardButton?.SetActive(true);
    }

    public void ClaimReward()
    {
        var quest = GetCurrentQuest();
        if (!isQuestCompleted || quest == null) return;

        playerStats?.AddReward(quest.rewardSoul, quest.rewardExp);
        UIManager.Instance.ShowRewardPopup(quest.rewardSoul, quest.rewardExp);
        UIManager.Instance.HideQuestProgress();

        claimRewardButton?.SetActive(false);

        currentQuestIndex++;
        isQuestCompleted = false;
        HideQuestUI();
    }

    public void DeclineQuest()
    {
        isQuestActive = false;
        isQuestCompleted = false;

        acceptButton?.SetActive(false);
        declineButton?.SetActive(false);
        claimRewardButton?.SetActive(false);
        HideQuestUI();
    }

    public void HideQuestUI()
    {
        if (questUI != null)
            questUI.SetActive(false);
    }

    public bool IsQuestCompleted() => isQuestCompleted;
    public bool IsQuestActive() => isQuestActive;
}

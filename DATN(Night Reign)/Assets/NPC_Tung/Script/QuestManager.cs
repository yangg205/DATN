using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public QuestDatabase questDatabase;
    public PlayerStats_Tung playerStats; // Đảm bảo đã gán PlayerStats_Tung GameObject vào đây trong Inspector

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
        {
            Instance = this;
            Debug.Log("✨ QuestManager Instance đã được thiết lập.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("⚠️ Đã có một instance QuestManager khác trong scene. Hủy bản sao này.");
        }
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
            // Thêm logic kiểm tra thu thập vật phẩm ở đây nếu cần (tùy thuộc vào cách bạn làm game)
            // Ví dụ: if (quest != null && quest.questType == QuestType.CollectItem) CheckItemCollectionProgress();
        }
    }

    public QuestData GetCurrentQuest() =>
        (questDatabase != null && currentQuestIndex < questDatabase.quests.Length)
        ? questDatabase.quests[currentQuestIndex]
        : null;

    public void AcceptQuest()
    {
        var quest = GetCurrentQuest();
        if (quest == null || isQuestActive || isQuestCompleted)
        {
            Debug.LogWarning("⛔ Không thể chấp nhận nhiệm vụ: Không có nhiệm vụ, nhiệm vụ đang hoạt động, hoặc đã hoàn thành.");
            return;
        }

        isQuestActive = true;
        isQuestCompleted = false;
        currentKills = 0;
        currentItemCount = 0;

        Debug.Log($"🔵 Nhiệm vụ '{quest.questName}' đã được chấp nhận!");

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
        if (!isQuestActive || isQuestCompleted || quest == null || quest.questType != QuestType.KillEnemies)
        {
            Debug.LogWarning("⛔ Không thể báo cáo tiêu diệt: Nhiệm vụ không hoạt động, đã hoàn thành, hoặc không phải nhiệm vụ tiêu diệt.");
            return;
        }

        currentKills++;
        UIManager.Instance.UpdateQuestProgress(currentKills, quest.requiredKills);
        Debug.Log($"🔄 Tiến độ tiêu diệt: {currentKills}/{quest.requiredKills}");

        if (currentKills >= quest.requiredKills)
        {
            CompleteQuest();
        }
    }

    public void CheckItemCollectionProgress()
    {
        var quest = GetCurrentQuest();
        if (!isQuestActive || isQuestCompleted || quest == null || quest.questType != QuestType.CollectItem)
        {
            Debug.LogWarning("⛔ Không thể kiểm tra vật phẩm: Nhiệm vụ không hoạt động, đã hoàn thành, hoặc không phải nhiệm vụ thu thập.");
            return;
        }

        // Đảm bảo SimpleInventory.Instance không null
        if (SimpleInventory.Instance == null)
        {
            Debug.LogError("🔴 LỖI: SimpleInventory.Instance là NULL. Không thể kiểm tra tiến độ vật phẩm.");
            return;
        }

        int current = SimpleInventory.Instance.GetItemCount(quest.targetItemID);
        currentItemCount = current;
        UIManager.Instance.UpdateQuestProgress(currentItemCount, quest.requiredItemCount);
        Debug.Log($"🔄 Tiến độ thu thập: {currentItemCount}/{quest.requiredItemCount}");


        if (currentItemCount >= quest.requiredItemCount)
        {
            CompleteQuest();
        }
    }

    public void TryCompleteQuestByTalk()
    {
        var quest = GetCurrentQuest();
        if (quest != null && isQuestActive && !isQuestCompleted && quest.questType == QuestType.FindNPC)
        {
            Debug.Log($"✅ Nhiệm vụ FindNPC đã hoàn thành bằng cách nói chuyện với NPC: {quest.targetNPCID}");
            CompleteQuest();
        }
        else
        {
            Debug.LogWarning("⛔ Không thể hoàn thành nhiệm vụ FindNPC: Không đúng loại nhiệm vụ hoặc trạng thái.");
        }
    }

    public void CompleteQuest()
    {
        if (!isQuestActive || isQuestCompleted)
        {
            Debug.LogWarning("⛔ Không thể hoàn thành nhiệm vụ: Nhiệm vụ không hoạt động hoặc đã hoàn thành.");
            return;
        }

        isQuestActive = false;
        isQuestCompleted = true;
        Debug.Log($"✅ Nhiệm vụ '{GetCurrentQuest()?.questName}' đã hoàn thành! Sẵn sàng nhận thưởng.");

        UIManager.Instance.HideQuestProgress();

        // Đảm bảo các nút được cập nhật đúng sau khi hoàn thành nhiệm vụ
        acceptButton?.SetActive(false);
        declineButton?.SetActive(false);
        claimRewardButton?.SetActive(true);
    }

    public void ClaimReward()
    {
        var quest = GetCurrentQuest();
        if (!isQuestCompleted || quest == null)
        {
            Debug.LogWarning("⛔ Không thể nhận thưởng: Nhiệm vụ chưa hoàn thành hoặc không có nhiệm vụ.");
            return;
        }

        Debug.Log($"🔵 Đang nhận thưởng cho nhiệm vụ: {quest.questName}. Soul: {quest.rewardSoul}, EXP: {quest.rewardExp}");

        // --- KIỂM TRA THAM CHIẾU PLAYERSTATS TRƯỚC KHI GỌI HÀM ---
        if (playerStats == null)
        {
            Debug.LogError("🔴 LỖI: Tham chiếu PlayerStats trong QuestManager là NULL! Không thể thêm phần thưởng. Hãy gán nó trong Inspector.");
            HideQuestUI();
            return;
        }
        // -----------------------------------------------------

        playerStats.AddReward(quest.rewardSoul, quest.rewardExp); // Gọi PlayerStats_Tung để cộng dồn và cập nhật UI tổng

        // UIManager.Instance.ShowRewardPopup chỉ hiển thị popup tạm thời, không liên quan đến tổng Soul/EXP
        UIManager.Instance.ShowRewardPopup(quest.rewardSoul, quest.rewardExp);

        UIManager.Instance.HideQuestProgress();

        claimRewardButton?.SetActive(false);

        // Chuyển sang nhiệm vụ tiếp theo
        currentQuestIndex++;
        isQuestCompleted = false; // Đặt lại trạng thái để có thể nhận nhiệm vụ tiếp theo

        Debug.Log($"🟢 Đã nhận thưởng thành công. Tổng Soul hiện tại: {playerStats.soul}, Tổng EXP hiện tại: {playerStats.experience}. Chuyển sang nhiệm vụ tiếp theo (Index: {currentQuestIndex})");

        HideQuestUI(); // Ẩn UI nhiệm vụ sau khi nhận thưởng
    }

    public void DeclineQuest()
    {
        isQuestActive = false;
        isQuestCompleted = false;
        Debug.Log("⛔ Nhiệm vụ đã bị từ chối.");

        acceptButton?.SetActive(false);
        declineButton?.SetActive(false);
        claimRewardButton?.SetActive(false);
        HideQuestUI();
    }

    public void HideQuestUI()
    {
        if (questUI != null)
        {
            questUI.SetActive(false);
            Debug.Log(" UI nhiệm vụ đã ẩn.");
        }
    }

    public bool IsQuestCompleted() => isQuestCompleted;
    public bool IsQuestActive() => isQuestActive;

    // Hàm kiểm tra người chơi đã nhận nhiệm vụ (đang hoạt động nhưng chưa hoàn thành)
    public bool IsQuestAccepted()
    {
        return isQuestActive && !isQuestCompleted;
    }
}
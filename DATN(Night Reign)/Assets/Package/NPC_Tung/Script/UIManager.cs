using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI; // Thêm namespace này cho Slider

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // Phải là public static cho Singleton

    [Header("Quest Reward UI")]
    [SerializeField] private GameObject rewardPopup;
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Player Stats UI")]
    [SerializeField] private Slider expSlider;

    [Header("Quest Progress UI")]
    [SerializeField] private TextMeshProUGUI questProgressText;
    [SerializeField] private TextMeshProUGUI questDistanceText;

    [Header("Quest Notice Text (hiện trong Box)")]
    [SerializeField] private TextMeshProUGUI questNoticeText;

    private Coroutine noticeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ UIManager Instance đã sẵn sàng.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("⚠️ Có nhiều hơn 1 UIManager trong scene, đã hủy bản sao.");
        }

        // Đăng ký sự kiện từ WaypointManager để cập nhật khoảng cách
        if (FindObjectOfType<WaypointManager>() != null)
        {
            WaypointManager.Instance.OnActiveWaypointChanged += UpdateActiveWaypointDistance;
        }
        else
        {
            Debug.LogWarning("WaypointManager.Instance không tìm thấy. Tính năng hiển thị khoảng cách waypoint sẽ không hoạt động.");
        }
    }

    private void Update()
    {
        // Cập nhật liên tục khoảng cách tới Active Waypoint
        if (questDistanceText != null && FindObjectOfType<WaypointManager>() != null && WaypointManager.Instance.GetActiveWaypoint() != null)
        {
            float distance = WaypointManager.Instance.GetDistanceToActiveWaypoint();
            if (distance >= 0)
            {
                questDistanceText.text = $"{Mathf.RoundToInt(distance)}m";
                questDistanceText.gameObject.SetActive(true);
            }
            else
            {
                questDistanceText.gameObject.SetActive(false);
            }
        }
        else if (questDistanceText != null)
        {
            questDistanceText.gameObject.SetActive(false);
        }
    }

    // 🟢 Hiển thị thông báo tạm trong Box (Text UI)
    public void ShowNotice(string message, float duration = 2.5f)
    {
        if (questNoticeText == null)
        {
            Debug.LogWarning("⚠️ Chưa gán questNoticeText trong UIManager!");
            return;
        }

        if (noticeCoroutine != null)
            StopCoroutine(noticeCoroutine);

        noticeCoroutine = StartCoroutine(ShowNoticeCoroutine(message, duration));
    }

    private IEnumerator ShowNoticeCoroutine(string message, float duration)
    {
        questNoticeText.text = message;
        yield return new WaitForSeconds(duration);
        questNoticeText.text = "";
    }

    // 🟢 Popup phần thưởng
    public void ShowRewardPopup(int coin, int exp)
    {
        if (rewardPopup == null || coinText == null)
        {
            Debug.LogWarning("⚠️ Reward UI chưa được gán đầy đủ trong Inspector (thiếu rewardPopup hoặc coinText).");
            return;
        }

        coinText.text = $"Coin: {coin}";
        rewardPopup.SetActive(true);
        StartCoroutine(HideRewardPopupAfterDelay(2f));
    }

    private IEnumerator HideRewardPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        rewardPopup.SetActive(false);
    }

    // Cập nhật tiến độ nhiệm vụ dạng số (vd: 0/5)
    public void UpdateQuestProgress(int current, int total)
    {
        if (questProgressText != null)
        {
            string localizedProgress = GetLocalizedString("NhiemVu", "Quest_Progress");
            questProgressText.text = string.Format(localizedProgress, current, total);
        }
    }

    // Cập nhật tiến độ nhiệm vụ dạng text (vd: "Tìm và nói chuyện với NPC mục tiêu")
    public void UpdateQuestProgressText(string progressMessage)
    {
        if (questProgressText != null)
        {
            questProgressText.text = progressMessage;
        }
    }

    public void HideQuestProgress()
    {
        if (questProgressText != null)
            questProgressText.text = "";
        if (questDistanceText != null)
            questDistanceText.text = "";
        questDistanceText.gameObject.SetActive(false);
    }

    // 🟢 Hàm để cập nhật EXP Slider
    public void UpdateExpSlider(float currentExp, float maxExp)
    {
        if (expSlider == null)
        {
            Debug.LogWarning("⚠️ Chưa gán Exp Slider trong UIManager!");
            return;
        }

        expSlider.maxValue = maxExp;
        expSlider.value = currentExp;
    }

    // Hàm gọi khi active waypoint thay đổi
    private void UpdateActiveWaypointDistance(Waypoint activeWaypoint)
    {
        if (questDistanceText == null) return;

        if (activeWaypoint != null)
        {
            questDistanceText.gameObject.SetActive(true);
        }
        else
        {
            questDistanceText.text = "";
            questDistanceText.gameObject.SetActive(false);
        }
    }

    private string GetLocalizedString(string tableName, string key)
    {
        // KHÔNG CÓ LocalizationSettings.HasInstance ở đây.
        StringTable table = LocalizationSettings.StringDatabase.GetTable(tableName);
        if (table == null)
        {
            Debug.LogError($"❌ Bảng '{tableName}' không tồn tại. (UIManager)");
            return $"[TABLE NOT FOUND: {tableName}]";
        }

        var entry = table.GetEntry(key);
        if (entry == null)
        {
            Debug.LogError($"❌ Key '{key}' không có trong bảng '{tableName}' (UIManager)");
            return $"[MISSING KEY: {key}]";
        }

        return entry.GetLocalizedString();
    }
}
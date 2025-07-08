using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI; // Thêm namespace này cho Slider

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

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

        // Kiểm tra WaypointManager trước khi đăng ký sự kiện
        // Có thể dùng FindObjectOfType<WaypointManager>() != null trong Awake,
        // nhưng tốt hơn hết là để WaypointManager tự Awake trước UIManager.
        // Hoặc tìm kiếm instance sau khi chắc chắn nó đã tồn tại.
        // Trong trường hợp này, WaypointManager.Instance sẽ là null nếu nó chưa Awake.
        // Dùng sự kiện OnEnable/OnDisable hoặc kiểm tra trong Start là tốt hơn.
    }

    private void Start()
    {
        if (WaypointManager.Instance != null)
        {
            WaypointManager.Instance.OnActiveWaypointChanged += UpdateActiveWaypointDistance;
            Debug.Log("UIManager đã đăng ký sự kiện OnActiveWaypointChanged từ WaypointManager.");
        }
        else
        {
            Debug.LogWarning("WaypointManager.Instance không tìm thấy khi UIManager Start. Tính năng hiển thị khoảng cách waypoint sẽ không hoạt động.");
        }
    }

    private void OnDestroy()
    {
        // Đảm bảo hủy đăng ký sự kiện để tránh lỗi khi đối tượng bị hủy
        if (WaypointManager.Instance != null)
        {
            WaypointManager.Instance.OnActiveWaypointChanged -= UpdateActiveWaypointDistance;
            Debug.Log("UIManager đã hủy đăng ký sự kiện OnActiveWaypointChanged.");
        }
    }

    private void Update()
    {
        if (questDistanceText != null && WaypointManager.Instance != null && WaypointManager.Instance.GetActiveWaypoint() != null)
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

    public void UpdateQuestProgress(int current, int total)
    {
        if (questProgressText != null)
        {
            string localizedProgress = GetLocalizedString("NhiemVu", "Quest_Progress");
            questProgressText.text = string.Format(localizedProgress, current, total);
        }
    }

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
        if (questDistanceText != null) // Đảm bảo không null trước khi truy cập gameObject
            questDistanceText.gameObject.SetActive(false);
    }

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

    private void UpdateActiveWaypointDistance(Waypoint activeWaypoint)
    {
        // Logic hiển thị/ẩn TextDistance sẽ được thực hiện trong Update() hàng Frame
        // Phương thức này chỉ đơn thuần kích hoạt việc hiển thị khoảng cách nếu có waypoint hoạt động
        // hoặc ẩn nó nếu không có.
        if (questDistanceText == null) return;

        if (activeWaypoint != null)
        {
            questDistanceText.gameObject.SetActive(true);
            // Giá trị text sẽ được cập nhật trong Update()
        }
        else
        {
            questDistanceText.text = "";
            questDistanceText.gameObject.SetActive(false);
        }
    }

    private string GetLocalizedString(string tableName, string key)
    {
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
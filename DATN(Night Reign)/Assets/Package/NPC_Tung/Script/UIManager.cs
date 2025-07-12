using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

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
    }

    private void Start()
    {
        if (WaypointManager.Instance != null)
        {
            // WaypointManager.OnActiveWaypointChanged now correctly passes a Waypoint object
            WaypointManager.Instance.OnActiveWaypointChanged += UpdateActiveWaypointDistance;
            Debug.Log("UIManager đã đăng ký sự kiện OnActiveWaypointChanged từ WaypointManager.");

            // Call once at start to set initial state based on current active waypoint
            UpdateActiveWaypointDistance(WaypointManager.Instance.GetActiveWaypoint());
        }
        else
        {
            Debug.LogWarning("WaypointManager.Instance không tìm thấy khi UIManager Start. Khoảng cách waypoint sẽ không hiển thị.");
            if (questDistanceText != null)
            {
                questDistanceText.text = "";
                questDistanceText.gameObject.SetActive(false);
            }
        }

        // Ensure reward popup is hidden initially
        if (rewardPopup != null)
        {
            rewardPopup.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (WaypointManager.Instance != null)
        {
            WaypointManager.Instance.OnActiveWaypointChanged -= UpdateActiveWaypointDistance;
            Debug.Log("UIManager đã hủy đăng ký sự kiện OnActiveWaypointChanged.");
        }
    }

    private void Update()
    {
        if (questDistanceText == null) return;

        var waypointManager = WaypointManager.Instance;
        if (waypointManager != null)
        {
            var activeWaypoint = waypointManager.GetActiveWaypoint();
            if (activeWaypoint != null)
            {
                // REVERTED: Using GetDistanceToActiveWaypoint() as per your WaypointManager code
                float distance = waypointManager.GetDistanceToActiveWaypoint();
                if (distance >= 0)
                {
                    questDistanceText.text = $"{Mathf.RoundToInt(distance)}m";
                    // Ensure the GameObject is active if a valid distance is found
                    if (!questDistanceText.gameObject.activeSelf)
                    {
                        questDistanceText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    // Hide if distance is invalid (-1)
                    questDistanceText.text = "";
                    if (questDistanceText.gameObject.activeSelf)
                    {
                        questDistanceText.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                // If activeWaypoint becomes null during Update, hide the text
                if (questDistanceText.gameObject.activeSelf)
                {
                    questDistanceText.text = "";
                    questDistanceText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // If WaypointManager.Instance itself is null, hide the text
            if (questDistanceText.gameObject.activeSelf)
            {
                questDistanceText.text = "";
                questDistanceText.gameObject.SetActive(false);
            }
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
            Debug.LogWarning("⚠️ Reward UI chưa được gán đầy đủ trong Inspector.");
            return;
        }

        coinText.text = $"Coin: {coin}";
        rewardPopup.SetActive(true);
        StartCoroutine(HideRewardPopupAfterDelay(2f));
    }

    private IEnumerator HideRewardPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rewardPopup != null)
            rewardPopup.SetActive(false);
    }

    public void UpdateQuestProgress(int current, int total)
    {
        if (questProgressText == null) return;

        // NO CHANGES HERE - Localization logic preserved
        string localizedProgress = GetLocalizedString("NhiemVu", "Quest_Progress");
        questProgressText.text = $"{localizedProgress}: {current}/{total}";
    }

    public void UpdateQuestProgressText(string progressMessage)
    {
        if (questProgressText == null) return;

        questProgressText.text = progressMessage;
    }

    public void HideQuestProgress()
    {
        if (questProgressText != null)
            questProgressText.text = "";

        if (questDistanceText != null)
        {
            questDistanceText.text = "";
            questDistanceText.gameObject.SetActive(false);
        }
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

    // Keep this method name as requested. Its parameter now correctly matches System.Action<Waypoint>
    // This method is for handling the visibility state of questDistanceText.
    private void UpdateActiveWaypointDistance(Waypoint activeWaypoint)
    {
        if (questDistanceText == null) return;

        if (activeWaypoint != null)
        {
            // Activate the GameObject. The 'Update()' loop will handle populating the distance text.
            questDistanceText.gameObject.SetActive(true);
        }
        else
        {
            // If active waypoint becomes null, hide and clear the text.
            questDistanceText.text = "";
            questDistanceText.gameObject.SetActive(false);
        }
    }

    private string GetLocalizedString(string tableName, string key)
    {
        // NO CHANGES HERE - Localization logic preserved
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
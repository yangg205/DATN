using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Quest Reward UI")]
    [SerializeField] private GameObject rewardPopup;
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Player Stats UI")]
    [SerializeField] private Slider expSlider;

    [Header("Combined Quest Info UI")]
    [SerializeField] private TextMeshProUGUI combinedQuestInfoText;

    [Header("Quest Notice Text (hiện trong Box)")]
    [SerializeField] private TextMeshProUGUI questNoticeText;

    private Coroutine noticeCoroutine;

    private string _currentQuestTitleKey = "";
    private string _currentObjectiveKey = "";
    private string _currentQuestTitle = "";
    private string _currentObjectiveText = "";

    private int _currentProgressCount = 0;
    private int _totalProgressCount = 0;
    private float _currentWaypointDistance = -1f;

    private string _returnToNPCGiverNameKey = "";
    private string _returnToNPCGiverName = "";
    private bool _showReturnToNPC = false;

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
            WaypointManager.Instance.OnActiveWaypointChanged += UpdateActiveWaypointDistance;
            Debug.Log("UIManager đã đăng ký sự kiện OnActiveWaypointChanged từ WaypointManager.");
            UpdateActiveWaypointDistance(WaypointManager.Instance.GetActiveWaypoint());
        }
        else
        {
            Debug.LogWarning("WaypointManager.Instance không tìm thấy khi UIManager Start. Khoảng cách waypoint sẽ không hiển thị.");
        }

        if (rewardPopup != null)
        {
            rewardPopup.SetActive(false);
        }

        if (combinedQuestInfoText != null)
        {
            combinedQuestInfoText.text = "";
            combinedQuestInfoText.gameObject.SetActive(false);
        }

        SetReturnToNPCInfo("", false);

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDestroy()
    {
        if (WaypointManager.Instance != null)
        {
            WaypointManager.Instance.OnActiveWaypointChanged -= UpdateActiveWaypointDistance;
            Debug.Log("UIManager đã hủy đăng ký sự kiện OnActiveWaypointChanged.");
        }

        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private async void OnLocaleChanged(UnityEngine.Localization.Locale newLocale)
    {
        await RefreshCombinedQuestInfoUIAsync();
        Debug.Log($"[UIManager] Ngôn ngữ đã thay đổi thành: {newLocale.Identifier.Code}. Đang làm mới UI.");
    }

    private void Update()
    {
        if (WaypointManager.Instance != null && WaypointManager.Instance.GetActiveWaypoint() != null)
        {
            float newDistance = WaypointManager.Instance.GetDistanceToActiveWaypoint();
            if (Mathf.Abs(newDistance - _currentWaypointDistance) > 0.5f)
            {
                _currentWaypointDistance = newDistance;
                StartCoroutine(UpdateUIAsync());
            }
        }
        else if (_currentWaypointDistance != -1f)
        {
            _currentWaypointDistance = -1f;
            StartCoroutine(UpdateUIAsync());
        }
    }

    private IEnumerator UpdateUIAsync()
    {
        yield return RefreshCombinedQuestInfoUIAsync();
    }

    public async void ShowNotice(string messageKey, float duration = 2.5f)
    {
        if (questNoticeText == null)
        {
            Debug.LogWarning("⚠️ Chưa gán questNoticeText trong UIManager!");
            return;
        }

        if (noticeCoroutine != null)
            StopCoroutine(noticeCoroutine);

        // Lấy chuỗi bản địa hóa trước khi chạy coroutine
        string message = await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", messageKey);
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

        coinText.text = $"Coin: {coin}\nExp: {exp}";
        rewardPopup.SetActive(true);
        StartCoroutine(HideRewardPopupAfterDelay(2f));
    }

    private IEnumerator HideRewardPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rewardPopup != null)
            rewardPopup.SetActive(false);
    }

    public void UpdateQuestProgressText(string localizedQuestNameKey)
    {
        _currentQuestTitleKey = localizedQuestNameKey;
        Debug.Log($"[UIManager] UpdateQuestProgressText (Set Key): {_currentQuestTitleKey}");
        StartCoroutine(UpdateUIAsync());
    }

    public void UpdateCurrentQuestObjective(string localizedObjectiveKey)
    {
        _currentObjectiveKey = localizedObjectiveKey;
        Debug.Log($"[UIManager] UpdateCurrentQuestObjective (Set Key): {_currentObjectiveKey}");
        StartCoroutine(UpdateUIAsync());
    }

    public void UpdateQuestProgress(int current, int total)
    {
        _currentProgressCount = current;
        _totalProgressCount = total;
        Debug.Log($"[UIManager] UpdateQuestProgress (Set Progress): {current}/{total}");
        StartCoroutine(UpdateUIAsync());

        if (current >= total && total > 0)
        {
            if (combinedQuestInfoText != null) combinedQuestInfoText.color = Color.yellow;
        }
        else
        {
            if (combinedQuestInfoText != null) combinedQuestInfoText.color = Color.white;
        }
    }

    public void SetReturnToNPCInfo(string npcNameKey, bool show)
    {
        _returnToNPCGiverNameKey = npcNameKey;
        _showReturnToNPC = show;
        StartCoroutine(UpdateUIAsync());
        Debug.Log($"[UIManager] SetReturnToNPCInfo: Key={npcNameKey}, Show={show}");
    }

    public void HideQuestProgress()
    {
        _currentQuestTitleKey = "";
        _currentObjectiveKey = "";
        _currentQuestTitle = "";
        _currentObjectiveText = "";
        _currentProgressCount = 0;
        _totalProgressCount = 0;
        _currentWaypointDistance = -1f;
        _returnToNPCGiverNameKey = "";
        _returnToNPCGiverName = "";
        _showReturnToNPC = false;

        Debug.Log("[UIManager] HideQuestProgress: All quest info cleared.");

        if (combinedQuestInfoText != null)
        {
            combinedQuestInfoText.text = "";
            combinedQuestInfoText.gameObject.SetActive(false);
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

    private void UpdateActiveWaypointDistance(Waypoint activeWaypoint)
    {
        if (activeWaypoint == null)
        {
            _currentWaypointDistance = -1f;
            Debug.Log("[UIManager] UpdateActiveWaypointDistance: Active waypoint is NULL.");
        }
        StartCoroutine(UpdateUIAsync());
    }

    public async System.Threading.Tasks.Task RefreshCombinedQuestInfoUIAsync()
    {
        if (combinedQuestInfoText == null)
        {
            Debug.LogError("❌ combinedQuestInfoText chưa được gán trong Inspector!");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (!string.IsNullOrEmpty(_currentQuestTitleKey))
        {
            _currentQuestTitle = await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", _currentQuestTitleKey);
            sb.AppendLine($"<b>{_currentQuestTitle}</b>");

            if (!string.IsNullOrEmpty(_currentObjectiveKey))
            {
                _currentObjectiveText = await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", _currentObjectiveKey);
                sb.AppendLine(_currentObjectiveText);
            }

            if (_totalProgressCount > 0)
            {
                string progressLabel = await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Quest_Progress");
                sb.AppendLine($"{progressLabel}: {_currentProgressCount}/{_totalProgressCount}");
            }

            if (_currentWaypointDistance >= 0 && WaypointManager.Instance.GetActiveWaypoint() != null)
            {
                string distanceLabel = await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Quest/Distance");
                sb.AppendLine($"{distanceLabel}: {Mathf.RoundToInt(_currentWaypointDistance)}m");
            }

            if (_showReturnToNPC && !string.IsNullOrEmpty(_returnToNPCGiverNameKey))
            {
                string returnText = await LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "ReturnToNPC_Prefix");
                _returnToNPCGiverName = await LocalizationManager.Instance.GetLocalizedStringAsync("NPC_Names", _returnToNPCGiverNameKey);
                sb.AppendLine($"{returnText} <b>{_returnToNPCGiverName}</b>");
            }
        }

        combinedQuestInfoText.text = sb.ToString();
        combinedQuestInfoText.gameObject.SetActive(!string.IsNullOrEmpty(combinedQuestInfoText.text));

        Debug.Log($"--- [UIManager] RefreshCombinedQuestInfoUIAsync Output ---\n" +
                  $"Title Key: '{_currentQuestTitleKey}' (Text: '{_currentQuestTitle}')\n" +
                  $"Objective Key: '{_currentObjectiveKey}' (Text: '{_currentObjectiveText}')\n" +
                  $"Progress: {_currentProgressCount}/{_totalProgressCount}\n" +
                  $"Distance: {_currentWaypointDistance}\n" +
                  $"Return to NPC: {_showReturnToNPC} - Key: '{_returnToNPCGiverNameKey}' (Text: '{_returnToNPCGiverName}')\n" +
                  $"Final Text Set to UI:\n'{combinedQuestInfoText.text}'\n" +
                  $"UI Active: {combinedQuestInfoText.gameObject.activeSelf}\n" +
                  $"--------------------------------------------------");
    }
}
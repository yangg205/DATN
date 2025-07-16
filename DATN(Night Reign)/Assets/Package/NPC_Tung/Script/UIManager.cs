// UIManager.cs (GIỮ NGUYÊN CẤU TRÚC VÀ LOGIC CỦA BẠN, CÓ BỔ SUNG NHỎ ĐÃ THẢO LUẬN)
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Quest Reward UI")]
    [SerializeField] private GameObject rewardPopup;
    [SerializeField] private TextMeshProUGUI coinText;
    // Có thể thêm [SerializeField] private TextMeshProUGUI expText; nếu bạn muốn hiển thị EXP riêng

    [Header("Player Stats UI")]
    [SerializeField] private Slider expSlider;

    [Header("Combined Quest Info UI")]
    [SerializeField] private TextMeshProUGUI combinedQuestInfoText;

    [Header("Quest Notice Text (hiện trong Box)")]
    [SerializeField] private TextMeshProUGUI questNoticeText;

    // ĐÃ XÓA: [Header("Return to NPC Text")] và returnToNPCNameText
    // Thay vào đó, thông tin này sẽ được quản lý nội bộ và thêm vào combinedQuestInfoText

    private Coroutine noticeCoroutine;

    private string _currentQuestTitle = "";
    private string _currentObjectiveText = "";

    private int _currentProgressCount = 0;
    private int _totalProgressCount = 0;
    private float _currentWaypointDistance = -1f;

    // THÊM VÀO: Biến để lưu trữ tên NPC cần quay lại
    private string _returnToNPCGiverName = "";
    private bool _showReturnToNPC = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Có thể giữ UIManager qua các cảnh nếu cần
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
            UpdateActiveWaypointDistance(WaypointManager.Instance.GetActiveWaypoint()); // Cập nhật khoảng cách ban đầu nếu có waypoint
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

        // Đảm bảo ẩn thông tin "Quay lại gặp NPC" khi khởi tạo
        SetReturnToNPCInfo("", false); // Đã đổi tên hàm
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
        // Logic cập nhật khoảng cách trong Update() là để khoảng cách được refresh liên tục.
        if (WaypointManager.Instance != null && WaypointManager.Instance.GetActiveWaypoint() != null)
        {
            float newDistance = WaypointManager.Instance.GetDistanceToActiveWaypoint();
            if (Mathf.Abs(newDistance - _currentWaypointDistance) > 0.5f) // Chỉ cập nhật nếu có thay đổi đáng kể
            {
                _currentWaypointDistance = newDistance;
                RefreshCombinedQuestInfoUI();
            }
        }
        else if (_currentWaypointDistance != -1f) // Nếu không còn waypoint nhưng vẫn hiển thị khoảng cách cũ
        {
            _currentWaypointDistance = -1f;
            RefreshCombinedQuestInfoUI();
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

    public void ShowRewardPopup(int coin, int exp) // Thêm exp vào đây vì bạn có rewardExp trong QuestData
    {
        if (rewardPopup == null || coinText == null)
        {
            Debug.LogWarning("⚠️ Reward UI chưa được gán đầy đủ trong Inspector.");
            return;
        }

        coinText.text = $"Coin: {coin}\nExp: {exp}"; // Hiển thị cả Coin và Exp
        rewardPopup.SetActive(true);
        StartCoroutine(HideRewardPopupAfterDelay(2f));
    }

    private IEnumerator HideRewardPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rewardPopup != null)
            rewardPopup.SetActive(false);
    }

    public void UpdateQuestProgressText(string localizedQuestName)
    {
        _currentQuestTitle = localizedQuestName;
        Debug.Log($"[UIManager] UpdateQuestProgressText (Set Localized Title): {_currentQuestTitle}");
        RefreshCombinedQuestInfoUI();
        if (combinedQuestInfoText != null) combinedQuestInfoText.color = Color.white;
    }

    public void UpdateCurrentQuestObjective(string localizedObjectiveDescription)
    {
        _currentObjectiveText = localizedObjectiveDescription;
        Debug.Log($"[UIManager] UpdateCurrentQuestObjective (Set Localized Objective): {_currentObjectiveText}");
        RefreshCombinedQuestInfoUI();
    }

    public void UpdateQuestProgress(int current, int total)
    {
        _currentProgressCount = current;
        _totalProgressCount = total;
        Debug.Log($"[UIManager] UpdateQuestProgress (Set Progress): {current}/{total}");
        RefreshCombinedQuestInfoUI();

        if (current >= total && total > 0) // Chỉ đổi màu khi có tiến độ và đã đạt mục tiêu
        {
            if (combinedQuestInfoText != null) combinedQuestInfoText.color = Color.yellow;
        }
        else
        {
            if (combinedQuestInfoText != null) combinedQuestInfoText.color = Color.white;
        }
    }

    /// <summary>
    /// Đặt thông tin tên NPC cần quay lại và trạng thái hiển thị.
    /// Thông tin này sẽ được thêm vào combinedQuestInfoText.
    /// </summary>
    /// <param name="npcName">Tên của NPC cần quay lại gặp.</param>
    /// <param name="show">True nếu thông tin này nên được hiển thị trong combinedQuestInfoText.</param>
    public void SetReturnToNPCInfo(string npcName, bool show)
    {
        _returnToNPCGiverName = npcName;
        _showReturnToNPC = show;
        RefreshCombinedQuestInfoUI(); // Kích hoạt refresh UI để hiển thị/ẩn thông tin
    }

    public void HideQuestProgress()
    {
        _currentQuestTitle = "";
        _currentObjectiveText = "";

        _currentProgressCount = 0;
        _totalProgressCount = 0;
        _currentWaypointDistance = -1f;
        Debug.Log("[UIManager] HideQuestProgress: All quest info cleared.");

        if (combinedQuestInfoText != null)
        {
            combinedQuestInfoText.text = "";
            combinedQuestInfoText.gameObject.SetActive(false);
        }
        SetReturnToNPCInfo("", false); // Ẩn luôn thông tin NPC khi ẩn progress
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
        // Khi waypoint active thay đổi (có thể là null), chỉ cần gọi RefreshCombinedQuestInfoUI
        // vì biến _currentWaypointDistance sẽ được cập nhật trong hàm Update() chính
        // hoặc được reset về -1f nếu activeWaypoint là null.
        // Hơn nữa, RefreshCombinedQuestInfoUI sẽ kiểm tra _currentWaypointDistance >= 0 để hiển thị.
        if (activeWaypoint == null)
        {
            Debug.Log("[UIManager] UpdateActiveWaypointDistance: Active waypoint is NULL. UI will reflect this on next refresh.");
        }
        RefreshCombinedQuestInfoUI(); // Kích hoạt UI refresh khi waypoint thay đổi
    }

    public void RefreshCombinedQuestInfoUI()
    {
        if (combinedQuestInfoText == null)
        {
            Debug.LogError("❌ combinedQuestInfoText chưa được gán trong Inspector!");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Chỉ hiển thị nếu có tên nhiệm vụ
        if (!string.IsNullOrEmpty(_currentQuestTitle))
        {
            sb.AppendLine($"<b>{_currentQuestTitle}</b>");

            if (!string.IsNullOrEmpty(_currentObjectiveText))
            {
                sb.AppendLine(_currentObjectiveText);
            }

            // Chỉ hiển thị tiến độ số nếu _totalProgressCount > 0
            if (_totalProgressCount > 0)
            {
                string localizedProgressLabel = GetLocalizedString("NhiemVu", "Quest_Progress");
                sb.AppendLine($"{localizedProgressLabel}: {_currentProgressCount}/{_totalProgressCount}");
            }

            // Chỉ hiển thị khoảng cách nếu có waypoint và khoảng cách hợp lệ
            if (_currentWaypointDistance >= 0 && WaypointManager.Instance.GetActiveWaypoint() != null)
            {
                string localizedDistanceLabel = GetLocalizedString("NhiemVu", "Quest/Distance");
                sb.AppendLine($"{localizedDistanceLabel}: {Mathf.RoundToInt(_currentWaypointDistance)}m");
            }

            // THÊM VÀO: Logic để hiển thị "Quay lại gặp NPC" vào combinedQuestInfoText
            if (_showReturnToNPC && !string.IsNullOrEmpty(_returnToNPCGiverName))
            {
                string returnText = GetLocalizedString("NhiemVu", "ReturnToNPC_Prefix"); // Ví dụ: "Quay lại gặp:"
                sb.AppendLine($"{returnText} <b>{_returnToNPCGiverName}</b>");
            }
        }

        combinedQuestInfoText.text = sb.ToString();
        combinedQuestInfoText.gameObject.SetActive(!string.IsNullOrEmpty(combinedQuestInfoText.text)); // Ẩn/hiện panel

        Debug.Log($"--- [UIManager] RefreshCombinedQuestInfoUI Output ---\n" +
                  $"Title: '{_currentQuestTitle}'\n" +
                  $"Objective: '{_currentObjectiveText}'\n" +
                  $"Progress: {_currentProgressCount}/{_totalProgressCount}\n" +
                  $"Distance: {_currentWaypointDistance}\n" +
                  $"Return to NPC: {_showReturnToNPC} - {_returnToNPCGiverName}\n" + // Thêm log cho dễ debug
                  $"Final Text Set to UI:\n" +
                  $"'{combinedQuestInfoText.text}'\n" +
                  $"UI Active: {combinedQuestInfoText.gameObject.activeSelf}\n" +
                  $"--------------------------------------------------");
    }

    public string GetLocalizedString(string tableName, string key)
    {
        StringTable table = LocalizationSettings.StringDatabase.GetTable(tableName);
        if (table == null)
        {
            Debug.LogError($"❌ Bảng '{tableName}' không tồn tại. (UIManager)");
            return $"[TABLE NOT FOUND: {tableName}]";
        }
        key = key.Trim();
        var entry = table.GetEntry(key);
        if (entry == null)
        {
            Debug.LogError($"❌ Key '{key}' không có trong bảng '{tableName}' (UIManager)");
            return $"[MISSING KEY: {key}]";
        }

        string localizedString = entry.GetLocalizedString();
        return localizedString;
    }
}
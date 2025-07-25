using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Threading.Tasks;
using System.Linq;

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
    private bool isUIUpdatePending = false;

    public string _currentQuestTitleKey = "";
    public string _currentObjectiveKey = "";
    public int _currentProgressCount = 0;
    public int _totalProgressCount = 0;
    public float _currentWaypointDistance = -1f;
    public string _returnToNPCGiverNameKey = "";
    public bool _showReturnToNPC = false;
    public bool _isQuestAccepted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ UIManager Instance initialized.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("⚠️ Multiple UIManager instances found, destroying duplicate.");
        }
    }

    private void Start()
    {
        if (WaypointManager.Instance != null)
        {
            WaypointManager.Instance.OnActiveWaypointChanged += UpdateActiveWaypointDistance;
            Debug.Log("UIManager subscribed to OnActiveWaypointChanged.");
            UpdateActiveWaypointDistance(WaypointManager.Instance.GetActiveWaypoint());
        }
        else
        {
            Debug.LogWarning("WaypointManager.Instance not found at UIManager Start. Waypoint distance will not display.");
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
            Debug.Log("UIManager unsubscribed from OnActiveWaypointChanged.");
        }

        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale newLocale)
    {
        Debug.Log($"[UIManager] Locale changed to: {newLocale.Identifier.Code}. Refreshing UI.");
        if (_isQuestAccepted && QuestManager.Instance != null)
        {
            QuestData activeQuest = QuestManager.Instance.GetActiveQuest();
            if (activeQuest != null)
            {
                UpdateQuestProgressText(activeQuest.questName);
                UpdateCurrentQuestObjective(activeQuest.description);
            }
        }
        StartCoroutine(RefreshCombinedQuestInfoUICoroutine());
    }

    private void Update()
    {
        if (WaypointManager.Instance != null && WaypointManager.Instance.GetActiveWaypoint() != null)
        {
            float newDistance = WaypointManager.Instance.GetDistanceToActiveWaypoint();
            if (Mathf.Abs(newDistance - _currentWaypointDistance) > 0.5f)
            {
                _currentWaypointDistance = newDistance;
                RequestUIUpdate();
            }
        }
        else if (_currentWaypointDistance != -1f)
        {
            _currentWaypointDistance = -1f;
            RequestUIUpdate();
        }
    }

    private void RequestUIUpdate()
    {
        if (!isUIUpdatePending)
        {
            isUIUpdatePending = true;
            StartCoroutine(UpdateUI());
        }
    }

    private IEnumerator UpdateUI()
    {
        yield return new WaitForEndOfFrame();
        isUIUpdatePending = false;
        yield return StartCoroutine(RefreshAllUI());
    }

    private IEnumerator RefreshAllUI()
    {
        yield return StartCoroutine(RefreshCombinedQuestInfoUICoroutine());
        yield return StartCoroutine(RefreshRewardPopup());
        yield return StartCoroutine(RefreshNoticeText());
    }

    public async Task RefreshCombinedQuestInfoUIAsync()
    {
        await Task.Run(() => StartCoroutine(RefreshCombinedQuestInfoUICoroutine()));
    }

    public IEnumerator RefreshCombinedQuestInfoUICoroutine()
    {
        if (combinedQuestInfoText == null)
        {
            Debug.LogError("❌ combinedQuestInfoText not assigned in Inspector!");
            yield break;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (_isQuestAccepted && !string.IsNullOrEmpty(_currentQuestTitleKey))
        {
            Task<string> titleTask = LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", _currentQuestTitleKey);
            yield return new WaitUntil(() => titleTask.IsCompleted);
            string localizedTitle = titleTask.IsFaulted ? _currentQuestTitleKey : titleTask.Result;
            sb.AppendLine($"<b>{localizedTitle}</b>");

            if (!string.IsNullOrEmpty(_currentObjectiveKey))
            {
                Task<string> objectiveTask = LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", _currentObjectiveKey);
                yield return new WaitUntil(() => objectiveTask.IsCompleted);
                string localizedObjective = objectiveTask.IsFaulted ? _currentObjectiveKey : objectiveTask.Result;
                sb.AppendLine(localizedObjective);
            }

            Task<string> progressLabelTask = LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Quest_Progress");
            yield return new WaitUntil(() => progressLabelTask.IsCompleted);
            string progressLabel = progressLabelTask.IsFaulted ? "Progress" : progressLabelTask.Result;
            sb.AppendLine($"{progressLabel}: {_currentProgressCount}/{_totalProgressCount}");

            if (_currentWaypointDistance >= 0 && WaypointManager.Instance?.GetActiveWaypoint() != null)
            {
                Task<string> distanceLabelTask = LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "Quest/Distance");
                yield return new WaitUntil(() => distanceLabelTask.IsCompleted);
                string distanceLabel = distanceLabelTask.IsFaulted ? "Distance" : distanceLabelTask.Result;
                sb.AppendLine($"{distanceLabel}: {Mathf.RoundToInt(_currentWaypointDistance)}m");
            }

            if (_showReturnToNPC && !string.IsNullOrEmpty(_returnToNPCGiverNameKey))
            {
                Task<string> returnTextTask = LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", "ReturnToNPC_Prefix");
                Task<string> npcNameTask = LocalizationManager.Instance.GetLocalizedStringAsync("NPC_Names", _returnToNPCGiverNameKey);
                yield return new WaitUntil(() => returnTextTask.IsCompleted && npcNameTask.IsCompleted);
                string returnText = returnTextTask.IsFaulted ? "Return to" : returnTextTask.Result;
                string localizedNPCName = npcNameTask.IsFaulted ? _returnToNPCGiverNameKey : npcNameTask.Result;
                sb.AppendLine($"{returnText} <b>{localizedNPCName}</b>");
            }
        }

        combinedQuestInfoText.text = sb.ToString();
        combinedQuestInfoText.gameObject.SetActive(_isQuestAccepted && !string.IsNullOrEmpty(combinedQuestInfoText.text));

        Debug.Log($"[UIManager] RefreshCombinedQuestInfoUI:\n" +
                  $"Title Key: {_currentQuestTitleKey}\n" +
                  $"Objective Key: {_currentObjectiveKey}\n" +
                  $"Progress: {_currentProgressCount}/{_totalProgressCount}\n" +
                  $"Distance: {_currentWaypointDistance}\n" +
                  $"Return NPC: {_showReturnToNPC}, Key: {_returnToNPCGiverNameKey}\n" +
                  $"UI Text: {combinedQuestInfoText.text}\n" +
                  $"UI Active: {combinedQuestInfoText.gameObject.activeSelf}\n" +
                  $"Quest Accepted: {_isQuestAccepted}");
        yield return null;
    }

    private IEnumerator RefreshRewardPopup()
    {
        if (rewardPopup != null && coinText != null && rewardPopup.activeSelf)
        {
            if (!string.IsNullOrEmpty(coinText.text))
            {
                string coinValue = coinText.text.Trim();
                coinText.text = $"{coinValue}";
            }
        }
        yield return null;
    }

    private IEnumerator RefreshNoticeText()
    {
        if (questNoticeText != null && !string.IsNullOrEmpty(questNoticeText.text))
        {
            Task<string> noticeTask = LocalizationManager.Instance.GetLocalizedStringAsync("NhiemVu", questNoticeText.text);
            yield return new WaitUntil(() => noticeTask.IsCompleted);
            questNoticeText.text = noticeTask.IsFaulted ? questNoticeText.text : noticeTask.Result;
            Debug.Log($"[UIManager] RefreshNoticeText: Text={questNoticeText.text}");
        }
        yield return null;
    }

    public async void ShowNotice(string messageKey, float duration = 2.5f)
    {
        if (questNoticeText == null)
        {
            Debug.LogWarning("⚠️ questNoticeText not assigned in UIManager!");
            return;
        }

        if (noticeCoroutine != null)
            StopCoroutine(noticeCoroutine);

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
            Debug.LogWarning("⚠️ Reward UI not fully assigned in Inspector.");
            return;
        }

        coinText.text = $"{coin}";
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
        Debug.Log($"[UIManager] UpdateQuestProgressText: Key={_currentQuestTitleKey}");
        RequestUIUpdate();
    }

    public void UpdateCurrentQuestObjective(string localizedObjectiveKey)
    {
        _currentObjectiveKey = localizedObjectiveKey;
        Debug.Log($"[UIManager] UpdateCurrentQuestObjective: Key={_currentObjectiveKey}");
        RequestUIUpdate();
    }

    public void UpdateQuestProgress(int current, int total)
    {
        _currentProgressCount = current;
        _totalProgressCount = total;
        Debug.Log($"[UIManager] UpdateQuestProgress: {current}/{total}");
        if (combinedQuestInfoText != null)
        {
            combinedQuestInfoText.color = (current >= total && total > 0) ? Color.yellow : Color.white;
        }
        RequestUIUpdate();
    }

    public void SetReturnToNPCInfo(string npcNameKey, bool show)
    {
        _returnToNPCGiverNameKey = npcNameKey;
        _showReturnToNPC = show;
        Debug.Log($"[UIManager] SetReturnToNPCInfo: Key={npcNameKey}, Show={show}");
        RequestUIUpdate();
    }

    public void HideQuestProgress()
    {
        _currentQuestTitleKey = "";
        _currentObjectiveKey = "";
        _currentProgressCount = 0;
        _totalProgressCount = 0;
        _currentWaypointDistance = -1f;
        _returnToNPCGiverNameKey = "";
        _showReturnToNPC = false;
        _isQuestAccepted = false;

        Debug.Log("[UIManager] HideQuestProgress: All quest info cleared.");
        if (combinedQuestInfoText != null)
        {
            combinedQuestInfoText.text = "";
            combinedQuestInfoText.gameObject.SetActive(false);
        }
        RequestUIUpdate();
    }

    public void UpdateExpSlider(float currentExp, float maxExp)
    {
        if (expSlider == null)
        {
            Debug.LogWarning("⚠️ Exp Slider not assigned in UIManager!");
            return;
        }

        expSlider.maxValue = maxExp;
        expSlider.value = currentExp;
    }

    private void UpdateActiveWaypointDistance(Waypoint activeWaypoint)
    {
        _currentWaypointDistance = activeWaypoint != null ? WaypointManager.Instance.GetDistanceToActiveWaypoint() : -1f;
        Debug.Log($"[UIManager] UpdateActiveWaypointDistance: Distance={_currentWaypointDistance}");
        RequestUIUpdate();
    }

    public void SetQuestAccepted(bool accepted)
    {
        _isQuestAccepted = accepted;
        Debug.Log($"[UIManager] SetQuestAccepted: {_isQuestAccepted}");
        RequestUIUpdate();
    }
}
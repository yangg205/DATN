using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using server.model;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class BattleBoss : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup resultCanvasGroup; // CanvasGroup cho panel kết quả
    [SerializeField] private TextMeshProUGUI pointText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button closeButton;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;

    private Canvas panelCanvas;
    private SignalRClient signalRClient;

    private float currentTime = 0f;
    public bool isFighting = false;
    private int deathCount = 0;

    // Các tham số cho API
    public int playercharacterId;
    public int bossId;
    public double maxTime;

    void Awake()
    {
        // Lấy CanvasGroup nếu chưa set
        if (resultCanvasGroup == null)
            resultCanvasGroup = GetComponent<CanvasGroup>();
        if (resultCanvasGroup == null)
            resultCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Lấy Canvas riêng cho panel
        panelCanvas = GetComponent<Canvas>();
        if (panelCanvas == null)
            panelCanvas = gameObject.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 0;

        resultCanvasGroup.alpha = 0f; // Ẩn panel ban đầu
        resultCanvasGroup.blocksRaycasts = false;
        resultCanvasGroup.interactable = false;
    }

    void Start()
    {
        signalRClient = FindObjectOfType<SignalRClient>();
        if (signalRClient == null)
            Debug.LogError("SignalRClient not found in scene!");

        if (timerText != null&&isFighting==false)
            timerText.gameObject.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        else
            Debug.LogError("Close button is not assigned!");
    }

    public void StartFight(int setPlayerId, int setBossId, double setMaxTime, int initialDeath = 0)
    {
        playercharacterId = setPlayerId;
        bossId = setBossId;
        maxTime = setMaxTime;
        deathCount = initialDeath;

        currentTime = 0f;
        isFighting = true;

        if (isFighting)
        {
            timerText.gameObject.SetActive(true);
            UpdateTimerText();
        }

        Debug.Log("Fight started!");
    }

    public void OnPlayerDeath()
    {
        if (isFighting)
        {
            deathCount++;
            Debug.Log($"Player died! Total deaths: {deathCount}");
        }
    }

    public void OnBossDeath()
    {
        if (!isFighting) return;

        isFighting = false;

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        StartBattle(playercharacterId, bossId, deathCount, maxTime, currentTime);

        Debug.Log($"Boss defeated! Time: {currentTime}, Deaths: {deathCount}");
    }

    public void ShowResult(Battle battleResult)
    {
        if (battleResult == null)
        {
            Debug.LogError("Battle result is null!");
            return;
        }

        // Cập nhật thông tin
        if (pointText != null) pointText.text = battleResult.point.ToString();
        if (rankText != null) rankText.text = battleResult.text;

        // Hiển thị cursor và vô hiệu hóa input game
        MouseManager.Instance.ShowCursorAndDisableInput();

        // Đưa panel lên trên cùng
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 900;

        // Reset alpha và tương tác
        resultCanvasGroup.alpha = 0f;
        resultCanvasGroup.interactable = false;
        resultCanvasGroup.blocksRaycasts = false;

        // Fade in mượt
        resultCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            // Bật tương tác sau khi fade xong
            resultCanvasGroup.interactable = true;
            resultCanvasGroup.blocksRaycasts = true;
        });
    }

    public void ClosePanel()
    {
        Debug.Log("Close button clicked!");

        // Ngay lập tức vô hiệu hóa tương tác để tránh click lặp
        resultCanvasGroup.interactable = false;
        resultCanvasGroup.blocksRaycasts = false;

        // Fade out
        resultCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InCubic).OnComplete(() =>
        {
            panelCanvas.sortingOrder = 0;
            MouseManager.Instance.HideCursorAndEnableInput();
        });
    }


    private async void StartBattle(int playerId, int bossId, int death, double maxTime, double realTime)
    {
        if (signalRClient == null)
        {
            Debug.LogError("SignalRClient not initialized!");
            return;
        }

        try
        {
            BattleRq battleRq = new BattleRq
            {
                Player_Characters_id = playerId,
                Boss_id = bossId,
                Death = death,
                maxtime = maxTime,
                realtime = realTime
            };

            Battle battleResult = await signalRClient.Battle(battleRq);
            ShowResult(battleResult);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error calling Battle API: " + ex.Message);
        }
    }

    void Update()
    {
        if (isFighting)
        {
            currentTime += Time.deltaTime;
            UpdateTimerText();
        }
        if (isFighting == false && Input.GetKeyDown(KeyCode.Space))
        {
            ClosePanel();
        }
        //if (Input.GetKeyDown(KeyCode.B))
        //{
        //    StartFight(44, 6, 300);
        //}
        //if (Input.GetKeyDown(KeyCode.N))
        //{
        //    OnBossDeath();
        //}
        //if (Input.GetKeyDown(KeyCode.V))
        //{
        //    OnPlayerDeath();
        //}
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);
    }
}

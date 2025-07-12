using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WaypointUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public RectTransform rectTransform;
    [SerializeField] public CanvasGroup canvasGroup;
    [SerializeField] public TextMeshProUGUI distanceTMP;
    [SerializeField] public Image iconImage;

    [Header("Settings")]
    public float maxScale = 1f; // Dùng cho minimap và large map

    private Waypoint waypointData;
    private Transform playerTransform; // Sẽ được lấy từ WaypointManager

    private void Awake()
    {
        // Tự động gán nếu chưa được gán trong Inspector
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        Debug.Log($"[WaypointUI - {gameObject.name}] Awake called. GameObject activeSelf: {gameObject.activeSelf}");

        // Kiểm tra các tham chiếu UI quan trọng (phải được gán trong prefab)
        if (rectTransform == null) Debug.LogError($"[WaypointUI - {gameObject.name}] ERROR: 'rectTransform' is NULL on Awake! This UI element cannot function. Please assign in Prefab Inspector!");
        if (canvasGroup == null) Debug.LogWarning($"[WaypointUI - {gameObject.name}] WARNING: 'canvasGroup' is NULL on Awake! Alpha fading may not work correctly. Please add CanvasGroup component and assign in Prefab Inspector.");
        if (distanceTMP == null) Debug.LogWarning($"[WaypointUI - {gameObject.name}] WARNING: 'distanceTMP' is NULL on Awake! Distance text will not be displayed. Please assign in Prefab Inspector.");
        if (iconImage == null) Debug.LogWarning($"[WaypointUI - {gameObject.name}] WARNING: 'iconImage' is NULL on Awake! Waypoint icon will not be displayed. Please assign in Prefab Inspector.");
        else Debug.Log($"[WaypointUI - {gameObject.name}] 'iconImage' assigned: {iconImage.name}");
    }

    private void Start()
    {
        // Lấy playerTransform từ WaypointManager sau khi WaypointManager.Awake đã chạy
        if (WaypointManager.Instance != null)
        {
            playerTransform = WaypointManager.Instance.playerTransform;
            if (playerTransform == null)
            {
                Debug.LogWarning("[WaypointUI] Player Transform is NULL in WaypointManager. Distance calculation will not work.");
            }
        }
        else
        {
            Debug.LogWarning("[WaypointUI] WaypointManager Instance not found in Start. Player transform will not be available for distance calculation.");
        }
    }

    public void SetData(Waypoint waypoint)
    {
        waypointData = waypoint;

        if (iconImage != null && waypoint.icon != null)
        {
            iconImage.sprite = waypoint.icon;
            iconImage.gameObject.SetActive(true);
            // Debug.Log($"[WaypointUI - {gameObject.name}] Icon set to {waypoint.icon.name}.");
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
            // Debug.Log($"[WaypointUI - {gameObject.name}] No icon sprite provided or iconImage is NULL. Icon image hidden.");
        }

        gameObject.name = "WaypointUI_" + waypoint.id;
        Debug.Log($"[WaypointUI - {gameObject.name}] SetData called for {waypoint.name}. Current world position: {waypoint.worldPosition}.");
    }

    public void SetMapUIPosition(Vector2 anchoredPosition, float scale)
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.localScale = Vector3.one * scale;
            // Debug.Log($"[WaypointUI - {gameObject.name}] Set position: {anchoredPosition}, Scale: {scale}. Current RectTransform size: {rectTransform.rect.size}");
        }
        else
        {
            Debug.LogError($"[WaypointUI - {gameObject.name}] rectTransform is NULL when SetMapUIPosition is called! UI won't render correctly.");
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = scale > 0.001f ? 1f : 0f; // Đảm bảo alpha bằng 0 nếu scale quá nhỏ
            // Debug.Log($"[WaypointUI - {gameObject.name}] CanvasGroup alpha set to {canvasGroup.alpha}.");
        }
        else
        {
            // Nếu không có CanvasGroup, có thể ẩn/hiện trực tiếp dựa trên scale
            gameObject.SetActive(scale > 0.001f);
            // Debug.Log($"[WaypointUI - {gameObject.name}] GameObject active state set to {gameObject.activeSelf} (no CanvasGroup).");
        }

        // Cập nhật khoảng cách mỗi khi vị trí được đặt (chỉ nếu đang hiển thị)
        if (gameObject.activeSelf)
        {
            UpdateDistanceText();
        }
    }

    public Waypoint GetWaypointData()
    {
        return waypointData;
    }

    public void UpdateDistanceText()
    {
        if (waypointData == null || distanceTMP == null || playerTransform == null || !gameObject.activeSelf)
        {
            if (distanceTMP != null) distanceTMP.gameObject.SetActive(false);
            return;
        }

        float distance = Vector3.Distance(playerTransform.position, waypointData.worldPosition);
        distanceTMP.text = $"{distance:F0}m";
        distanceTMP.gameObject.SetActive(true);
        // Debug.Log($"[WaypointUI - {gameObject.name}] Distance updated: {distance:F0}m");
    }

    public void SetDistanceText(float distance)
    {
        if (distanceTMP != null)
        {
            distanceTMP.text = $"{distance:F0}m";
            distanceTMP.gameObject.SetActive(true);
        }
    }
}
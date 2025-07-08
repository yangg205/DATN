using UnityEngine;
using UnityEngine.UI;
using TMPro; // Sử dụng TextMeshPro cho văn bản

// Không có namespace

public class WaypointUI : MonoBehaviour
{
    public string Id { get; private set; }
    public WaypointType Type { get; private set; }
    private Vector3 _worldPosition;

    [Header("UI Components")]
    public Image waypointIcon;
    public TextMeshProUGUI distanceText; // Sử dụng TextMeshProUGUI

    [Header("Display Settings")]
    [Tooltip("Khoảng cách tối đa mà waypoint này được hiển thị trên HUD/Compass.")]
    public float maxDisplayDistance = 500f;
    [Tooltip("Khoảng cách mà waypoint sẽ tự động ẩn khi người chơi đến gần.")]
    public float hideNearDistance = 5f;
    [Tooltip("Khoảng cách mà QuestLocation waypoint sẽ bị xóa khi người chơi đến gần.")]
    public float questLocationCompletionDistance = 5f;

    // Các thiết lập scale mặc định cho WaypointUI (có thể bị override bởi Compass/Minimap)
    public float minScale = 0.5f;
    public float maxScale = 1.0f;

    private RectTransform _rectTransform;
    private Transform _playerTransform;
    private Camera _mainCamera;

    public enum WaypointUIType { HUD, Minimap, Compass, LargeMap }
    public WaypointUIType uiType;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (waypointIcon == null) Debug.LogError("[WaypointUI] Waypoint Icon Image is not assigned!", this);
        // distanceText có thể null nếu không hiển thị khoảng cách
    }

    public void Initialize(string id, Sprite icon, WaypointType type, Vector3 worldPosition, WaypointUIType uiType)
    {
        this.Id = id;
        this.waypointIcon.sprite = icon;
        this.Type = type;
        this._worldPosition = worldPosition;
        this.uiType = uiType;

        // Tìm kiếm các tham chiếu một lần để tối ưu hiệu suất
        if (_playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _playerTransform = playerObj.transform;
            else Debug.LogWarning("[WaypointUI] Player GameObject with 'Player' tag not found. WaypointUI might not update correctly.");
        }
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) Debug.LogWarning("[WaypointUI] Main Camera not found. WaypointUI might not update correctly.");
        }

        UpdateUI(); // Cập nhật ngay sau khi khởi tạo
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_playerTransform == null || _rectTransform == null || _worldPosition == Vector3.zero)
        {
            gameObject.SetActive(false); // Ẩn nếu thiếu tham chiếu
            return;
        }

        float distance = Vector3.Distance(_playerTransform.position, _worldPosition);

        // --- Logic ẩn/hiện và cập nhật khoảng cách cho HUD và Compass ---
        if (uiType == WaypointUIType.HUD || uiType == WaypointUIType.Compass)
        {
            // NEW: Xử lý QuestLocation hoàn thành (m chết hả)
            if (Type == WaypointType.QuestLocation && distance < questLocationCompletionDistance)
            {
                Debug.Log($"[WaypointUI] QuestLocation '{Id}' completed! Removing waypoint.");
                if (WaypointManager.Instance != null)
                {
                    WaypointManager.Instance.RemoveWaypoint(Id);
                }
                gameObject.SetActive(false); // Ẩn ngay lập tức
                return; // Không xử lý thêm logic hiển thị
            }

            // Logic ẩn/hiện thông thường (cho các loại waypoint khác hoặc QuestLocation chưa đạt)
            if (distance > maxDisplayDistance || distance < hideNearDistance)
            {
                gameObject.SetActive(false);
                return;
            }
            else
            {
                gameObject.SetActive(true);
            }

            // Cập nhật khoảng cách
            if (distanceText != null)
            {
                distanceText.text = distance.ToString("F0") + "m";
            }
        }
        else // Minimap và LargeMap thường luôn hiển thị (logic riêng)
        {
            gameObject.SetActive(true);
            if (distanceText != null)
            {
                distanceText.text = distance.ToString("F0") + "m";
            }
        }

        // --- Logic định vị icon UI dựa trên WaypointUIType ---
        // Đối với HUD và Compass, WaypointUI tự tính toán vị trí trên màn hình/la bàn
        if (uiType == WaypointUIType.HUD)
        {
            if (_mainCamera == null)
            {
                gameObject.SetActive(false);
                return;
            }

            Vector3 screenPoint = _mainCamera.WorldToScreenPoint(_worldPosition);

            // Kiểm tra nếu waypoint nằm sau camera hoặc quá xa khỏi rìa màn hình
            if (screenPoint.z < 0 || screenPoint.x < 0 || screenPoint.x > Screen.width || screenPoint.y < 0 || screenPoint.y > Screen.height)
            {
                gameObject.SetActive(false);
                return;
            }
            else
            {
                gameObject.SetActive(true);
            }

            RectTransform parentRectTransform = transform.parent.GetComponent<RectTransform>();
            if (parentRectTransform == null) { Debug.LogWarning($"[WaypointUI] HUD WaypointUI's parent '{transform.parent.name}' does not have a RectTransform.", this); return; }

            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, screenPoint, _mainCamera, out localPoint))
            {
                _rectTransform.anchoredPosition = localPoint;
            }

            // Scale theo khoảng cách (chỉ áp dụng cho HUD)
            float normalizedDistance = Mathf.InverseLerp(hideNearDistance, maxDisplayDistance, distance);
            float currentScale = Mathf.Lerp(maxScale, minScale, normalizedDistance);
            _rectTransform.localScale = new Vector3(currentScale, currentScale, currentScale);
        }
        // Đối với Minimap, Compass, LargeMap, vị trí và scale sẽ được đặt từ bên ngoài (WaypointManager, MinimapController, QT_CompassBar)
    }

    /// <summary>
    /// Đặt vị trí và scale cho WaypointUI trên Minimap, Compass, hoặc Large Map.
    /// </summary>
    public void SetMapUIPosition(Vector2 anchoredPosition, float currentScale)
    {
        if (_rectTransform != null)
        {
            _rectTransform.anchoredPosition = anchoredPosition;
            _rectTransform.localScale = new Vector3(currentScale, currentScale, currentScale);
        }
    }

    /// <summary>
    /// Đặt rotation cho WaypointUI trên bản đồ (ví dụ: icon người chơi trên Large Map).
    /// </summary>
    public void SetMapUIRotation(Quaternion rotation)
    {
        if (_rectTransform != null)
        {
            _rectTransform.rotation = rotation;
        }
    }
}
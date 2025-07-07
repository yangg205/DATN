using UnityEngine;
using UnityEngine.UI;
using System; // For Action
using UnityEngine.EventSystems; // Để kiểm tra click vào UI

public class LargeMapController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject largeMapPanel; // Panel chứa bản đồ lớn (LargeMap_Panel)
    public RawImage largeMapDisplayRaw; // Tham chiếu đến Raw Image hiển thị bản đồ lớn
    public RectTransform playerIconLargeMapRectTransform; // Player_LargeMapIcon

    [Header("World References")]
    public Camera largeMapCamera; // Camera cho bản đồ lớn (LargeMapCamera)
    public Transform playerTransform; // Transform của người chơi
    public Transform playerMainCameraTransform; // Thêm để icon xoay theo hướng người chơi

    [Tooltip("Terrain GameObject if you want to automatically get its size and position.")]
    public Terrain activeTerrain; // Kéo Terrain của bạn vào đây nếu có

    [Header("Terrain Dimensions (Manual if no Terrain GameObject assigned)")]
    public float terrainWidth = 150f;
    public float terrainLength = 150f;
    public float terrainMinX = 0f;
    public float terrainMinZ = 0f;

    [Header("Waypoint Creation")]
    public Sprite customWaypointIcon; // Icon cho waypoint do người chơi tạo
    private string currentCustomWaypointId; // ID của waypoint tự tạo hiện tại

    void Awake()
    {
        if (activeTerrain != null)
        {
            terrainWidth = activeTerrain.terrainData.size.x;
            terrainLength = activeTerrain.terrainData.size.z;
            terrainMinX = activeTerrain.transform.position.x;
            terrainMinZ = activeTerrain.transform.position.z;
        }
        else
        {
            Debug.LogWarning("Active Terrain is not assigned. Please manually set Terrain Dimensions (Width, Length, Min X, Min Z) in LargeMapController.");
        }
    }

    void Start()
    {
        if (largeMapPanel == null) Debug.LogError("LargeMapPanel is not assigned!");
        if (largeMapCamera == null) Debug.LogError("LargeMapCamera is not assigned!");
        if (largeMapDisplayRaw == null) Debug.LogError("LargeMapDisplayRaw is not assigned!");
        if (playerIconLargeMapRectTransform == null) Debug.LogError("PlayerIconLargeMapRectTransform is not assigned!");
        if (playerTransform == null) Debug.LogError("PlayerTransform is not assigned!");
        if (playerMainCameraTransform == null) Debug.LogError("Player Main Camera Transform is not assigned!"); // Thêm kiểm tra

        if (largeMapPanel != null)
        {
            largeMapPanel.SetActive(false);
            if (largeMapCamera != null) largeMapCamera.enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (largeMapPanel != null)
            {
                bool isActive = !largeMapPanel.activeSelf;
                largeMapPanel.SetActive(isActive);
                if (largeMapCamera != null) largeMapCamera.enabled = isActive;

                if (isActive)
                {
                    UpdatePlayerIconPosition();
                }

                //// Khóa/mở khóa input của người chơi khi bản đồ lớn bật/tắt
                //if (PlayerInputLocker.Instance != null)
                //{
                //    PlayerInputLocker.Instance.SetInputLocked(isActive);
                //}
            }
        }

        // Logic click chuột để tạo waypoint
        if (largeMapPanel != null && largeMapPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            // Kiểm tra xem chuột có click vào UI khác không để tránh tạo waypoint nhầm
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                //Debug.Log("Clicked on UI, not creating waypoint.");
                return;
            }

            // Tạo ray từ vị trí chuột trên màn hình
            Ray ray = largeMapCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Thực hiện raycast để tìm vị trí trên terrain
            // Đảm bảo Terrain của bạn có Collider
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain"))) // Đảm bảo layer "Terrain" tồn tại và terrain có collider
            {
                Vector3 hitPoint = hit.point;
                // Debug.Log($"Clicked on terrain at: {hitPoint}");

                // Xóa waypoint tự tạo cũ nếu có
                if (!string.IsNullOrEmpty(currentCustomWaypointId))
                {
                    WaypointManager.Instance?.RemoveWaypoint(currentCustomWaypointId);
                }

                // Tạo Waypoint mới
                currentCustomWaypointId = Guid.NewGuid().ToString(); // Tạo ID duy nhất
                Waypoint newWaypoint = new Waypoint(
                    currentCustomWaypointId,
                    "Điểm đánh dấu tùy chỉnh", // Tên hiển thị
                    hitPoint,
                    WaypointType.CustomMarker,
                    customWaypointIcon // Gán icon bạn muốn
                );

                WaypointManager.Instance?.AddWaypoint(newWaypoint, true); // Thêm và đặt làm active waypoint
            }
            else
            {
                Debug.Log("Clicked, but no terrain hit or wrong layer.");
            }
        }
    }

    void LateUpdate()
    {
        if (largeMapPanel != null && largeMapPanel.activeSelf)
        {
            UpdatePlayerIconPosition();
        }
    }

    void UpdatePlayerIconPosition()
    {
        if (playerTransform == null || playerIconLargeMapRectTransform == null || largeMapDisplayRaw == null || terrainWidth <= 0 || terrainLength <= 0 || playerMainCameraTransform == null)
        {
            return;
        }

        Rect largeMapDisplayRect = largeMapDisplayRaw.rectTransform.rect;

        float normalizedX = (playerTransform.position.x - terrainMinX) / terrainWidth;
        float normalizedY = (playerTransform.position.z - terrainMinZ) / terrainLength;

        float uiX = (normalizedX - 0.5f) * largeMapDisplayRect.width;
        float uiY = (normalizedY - 0.5f) * largeMapDisplayRect.height;

        playerIconLargeMapRectTransform.anchoredPosition = new Vector2(uiX, uiY);

        // Icon người chơi trên bản đồ lớn: XOAY THEO HƯỚNG NHÌN CỦA PLAYER
        // Dấu trừ cho góc Y của camera để icon xoay đúng hướng trên UI
        // (Unity UI Z-rotation dương là ngược chiều kim đồng hồ, camera Y-rotation dương là chiều kim đồng hồ)
        playerIconLargeMapRectTransform.localEulerAngles = new Vector3(0, 0, -playerMainCameraTransform.eulerAngles.y);
    }
}
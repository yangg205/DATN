using UnityEngine;
using UnityEngine.UI;

// Không có namespace

public class MinimapController : MonoBehaviour
{
    [Header("UI References")]
    public RawImage minimapDisplayRaw; // RawImage hiển thị minimap
    public RectTransform playerIconMinimapRectTransform;
    public RectTransform minimapWaypointsParentRectTransform; // Same as WaypointManager's minimapWaypointsParent

    [Header("World References")]
    public Camera minimapCamera; // Camera của minimap
    public Transform playerTransform;
    public Transform playerMainCameraTransform;

    [Header("Minimap Settings")]
    [Tooltip("Kích thước thế giới mà minimap hiển thị (ví dụ: 100 có nghĩa là 100x100 mét quanh người chơi).")]
    public float minimapViewRadius = 100f; // Kích thước của khu vực mà minimap hiển thị

    [Header("Terrain Reference")]
    public Terrain activeTerrain; // Tham chiếu đến Terrain để lấy kích thước tổng thể

    private void Awake()
    {
        if (activeTerrain != null)
        {
            if (WaypointManager.Instance != null)
            {
                WaypointManager.Instance.activeTerrain = activeTerrain; // Đảm bảo WaypointManager có terrain data
            }
            else
            {
                Debug.LogWarning("[MinimapController] WaypointManager Instance not found in Awake. Terrain might not be set.");
            }
        }
        else
        {
            Debug.LogWarning("[MinimapController] Active Terrain is not assigned. Minimap positioning might be incorrect.");
        }
    }

    void Start()
    {
        CheckRequiredReferences();
        SetupMinimapCamera();

        // Set up player icon parent (should be the same as WaypointManager's minimapWaypointsParent)
        if (playerIconMinimapRectTransform != null && minimapWaypointsParentRectTransform != null)
        {
            playerIconMinimapRectTransform.SetParent(minimapWaypointsParentRectTransform, false);
            Debug.Log("[MinimapController] Player icon parent set to minimapWaypointsParentRectTransform.");
        }
    }

    void LateUpdate()
    {
        UpdateMinimapCameraPosition();
        UpdatePlayerIconPositionAndRotation();
        UpdateWaypointUIsOnMinimap();
    }

    private void CheckRequiredReferences()
    {
        if (minimapDisplayRaw == null) Debug.LogError("[MinimapController] minimapDisplayRaw is not assigned!");
        if (playerIconMinimapRectTransform == null) Debug.LogError("[MinimapController] playerIconMinimapRectTransform is not assigned!");
        if (minimapWaypointsParentRectTransform == null) Debug.LogError("[MinimapController] minimapWaypointsParentRectTransform is not assigned!");
        if (minimapCamera == null) Debug.LogError("[MinimapController] minimapCamera is not assigned!");
        if (playerTransform == null) Debug.LogError("[MinimapController] playerTransform is not assigned!");
        if (playerMainCameraTransform == null) Debug.LogError("[MinimapController] playerMainCameraTransform is not assigned!");
        if (activeTerrain == null) Debug.LogError("[MinimapController] Active Terrain is not assigned!");
    }

    private void SetupMinimapCamera()
    {
        if (minimapCamera == null) return;

        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = minimapViewRadius / 2f; // Bán kính nhìn của minimap
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Nhìn thẳng xuống
    }

    private void UpdateMinimapCameraPosition()
    {
        if (playerTransform == null || minimapCamera == null) return;

        // Di chuyển camera minimap theo người chơi nhưng giữ nguyên chiều cao và xoay 90 độ
        minimapCamera.transform.position = new Vector3(
            playerTransform.position.x,
            playerTransform.position.y + minimapViewRadius, // Đặt camera cao hơn người chơi
            playerTransform.position.z
        );
    }

    private void UpdatePlayerIconPositionAndRotation()
    {
        // Icon người chơi trên minimap không cần di chuyển vị trí, vì bản đồ quay quanh nó.
        // Nó chỉ cần xoay để khớp với hướng của người chơi.
        if (playerIconMinimapRectTransform != null && playerMainCameraTransform != null)
        {
            playerIconMinimapRectTransform.localEulerAngles = new Vector3(0, 0, -playerMainCameraTransform.eulerAngles.y);
        }
    }

    private void UpdateWaypointUIsOnMinimap()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.minimapWaypointUIs == null || playerTransform == null || minimapDisplayRaw == null || minimapCamera == null) return;

        float mapWidth = minimapDisplayRaw.rectTransform.rect.width;
        float mapHeight = minimapDisplayRaw.rectTransform.rect.height;

        foreach (var entry in WaypointManager.Instance.minimapWaypointUIs)
        {
            WaypointUI waypointUI = entry.Value;
            Waypoint waypointData = WaypointManager.Instance.activeWaypointsData[entry.Key]; // Lấy Waypoint data gốc

            // Chuyển đổi vị trí thế giới của waypoint sang vị trí trên minimap UI
            // Điểm 0,0 của minimap UI là tâm của nó
            Vector3 relativePos = waypointData.worldPosition - minimapCamera.transform.position; // Vị trí tương đối với camera minimap

            // Scale relativePos để khớp với kích thước của minimap UI
            // orthographicSize là một nửa chiều cao của view frustum
            float pixelsPerWorldUnitX = mapWidth / (minimapCamera.orthographicSize * 2f);
            float pixelsPerWorldUnitY = mapHeight / (minimapCamera.orthographicSize * 2f);

            float uiX = relativePos.x * pixelsPerWorldUnitX;
            float uiY = relativePos.z * pixelsPerWorldUnitY; // Z của thế giới là Y trên UI

            // Tính toán khoảng cách để scale và ẩn/hiện (nếu cần)
            float distance = Vector3.Distance(playerTransform.position, waypointData.worldPosition);

            // Kiểm tra xem waypoint có nằm trong phạm vi hiển thị của minimap camera không
            // Nếu camera nhìn 100x100m, thì waypoint phải nằm trong 50m theo X và Z so với tâm camera.
            if (Mathf.Abs(relativePos.x) <= minimapCamera.orthographicSize && Mathf.Abs(relativePos.z) <= minimapCamera.orthographicSize)
            {
                waypointUI.gameObject.SetActive(true);
                waypointUI.SetMapUIPosition(new Vector2(uiX, uiY), waypointUI.maxScale); // Scale mặc định maxScale cho Minimap
            }
            else
            {
                waypointUI.gameObject.SetActive(false);
            }
        }
    }
}
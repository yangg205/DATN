using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapController : MonoBehaviour
{
    [Header("UI References")]
    public RawImage minimapDisplayRaw; // Background của minimap (UI)
    public RectTransform playerIconMinimapRectTransform; // Icon người chơi trên minimap

    [Header("World References")]
    public Camera minimapCamera; // Camera dùng để render minimap
    public Transform playerMainCameraTransform; // Camera chính của người chơi để lấy hướng

    [Header("Minimap Settings")]
    public float minimapViewRadius = 100f; // Bán kính nhìn của minimap trong thế giới game
    [Tooltip("Size of the UI container for minimap waypoints (e.g., SmallMinimapUIContainer)")]
    public float minimapUIContainerSize = 200f; // Kích thước của khung UI minimap (nếu hình vuông)

    void Start()
    {
        CheckRequiredReferences();
        SetupMinimapCamera();
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
        if (minimapCamera == null) Debug.LogError("[MinimapController] minimapCamera is not assigned!");
        if (playerMainCameraTransform == null) Debug.LogError("[MinimapController] playerMainCameraTransform is not assigned!");

        if (WaypointManager.Instance == null)
        {
            Debug.LogError("[MinimapController] WaypointManager.Instance is NULL! Make sure it exists and runs its Awake method.");
            return;
        }
        if (WaypointManager.Instance.playerTransform == null) Debug.LogError("[MinimapController] WaypointManager.Instance.playerTransform is not assigned!");
        if (WaypointManager.Instance.minimapWaypointsParent == null) Debug.LogError("[MinimapController] WaypointManager.Instance.minimapWaypointsParent is not assigned!");
        else
        {
            // Lấy kích thước thực tế của parent (SmallMinimapUIContainer) để tính toán vị trí UI chính xác hơn
            // Giả sử SmallMinimapUIContainer là hình vuông, lấy cạnh ngắn nhất
            RectTransform parentRect = WaypointManager.Instance.minimapWaypointsParent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                minimapUIContainerSize = Mathf.Min(parentRect.rect.width, parentRect.rect.height);
                Debug.Log($"[MinimapController] Minimap UI Container Size set to: {minimapUIContainerSize} based on parent RectTransform.");
            }
            else
            {
                Debug.LogWarning("[MinimapController] minimapWaypointsParent does not have a RectTransform. Using default minimapUIContainerSize.");
            }
        }
    }

    private void SetupMinimapCamera()
    {
        if (minimapCamera == null) return;

        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = minimapViewRadius; // orthographicSize là một nửa chiều cao của khung nhìn
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Camera nhìn thẳng xuống
        minimapCamera.clearFlags = CameraClearFlags.Color; // hoặc Solid Color
        minimapCamera.backgroundColor = Color.black; // Màu nền camera, nếu không có render texture
        // minimapCamera.targetTexture = yourRenderTexture; // Nếu bạn sử dụng Render Texture
        Debug.Log($"[MinimapController] Minimap Camera setup. Orthographic Size: {minimapCamera.orthographicSize}");
    }

    private void UpdateMinimapCameraPosition()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.playerTransform == null || minimapCamera == null) return;

        // Camera sẽ theo dõi vị trí XZ của người chơi, đặt ở độ cao cố định để nhìn xuống
        minimapCamera.transform.position = new Vector3(
            WaypointManager.Instance.playerTransform.position.x,
            WaypointManager.Instance.playerTransform.position.y + minimapViewRadius, // Đặt camera cao hơn player một khoảng bằng bán kính nhìn
            WaypointManager.Instance.playerTransform.position.z
        );
    }

    private void UpdatePlayerIconPositionAndRotation()
    {
        if (playerIconMinimapRectTransform != null && playerMainCameraTransform != null)
        {
            // Icon người chơi luôn ở giữa minimap (0,0) đối với RectTransform cha của nó
            playerIconMinimapRectTransform.anchoredPosition = Vector2.zero;
            // Xoay icon người chơi theo hướng nhìn của camera chính (chiều Y của camera)
            playerIconMinimapRectTransform.localEulerAngles = new Vector3(0, 0, -playerMainCameraTransform.eulerAngles.y);
            // Đảm bảo icon người chơi luôn hiển thị
            if (!playerIconMinimapRectTransform.gameObject.activeSelf)
                playerIconMinimapRectTransform.gameObject.SetActive(true);
        }
    }

    private void UpdateWaypointUIsOnMinimap()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.minimapWaypointUIs == null ||
            WaypointManager.Instance.playerTransform == null || minimapCamera == null ||
            WaypointManager.Instance.minimapWaypointsParent == null) return;

        RectTransform minimapContainerRect = WaypointManager.Instance.minimapWaypointsParent.GetComponent<RectTransform>();
        if (minimapContainerRect == null)
        {
            Debug.LogError("[MinimapController] minimapWaypointsParent does not have a RectTransform. Cannot calculate UI positions.");
            return;
        }

        float worldVisibleSize = minimapCamera.orthographicSize * 2f;
        float pixelsPerWorldUnit = minimapContainerRect.rect.width / worldVisibleSize;

        Vector3 playerWorldPos = WaypointManager.Instance.playerTransform.position;

        // Duyệt qua TẤT CẢ các waypoint đã được WaypointManager tạo UI cho minimap
        foreach (var entry in WaypointManager.Instance.minimapWaypointUIs)
        {
            WaypointUI waypointUI = entry.Value;
            Waypoint waypointData = entry.Value.GetWaypointData();

            if (waypointUI == null || waypointUI.gameObject == null)
            {
                Debug.LogWarning($"[MinimapController] minimapWaypointUI for ID '{entry.Key}' is null or its GameObject is null. Skipping.");
                continue;
            }

            // Tính toán khoảng cách
            Vector3 relativeWorldPos = waypointData.worldPosition - playerWorldPos;
            Vector2 relativeWorldPos2D = new Vector2(relativeWorldPos.x, relativeWorldPos.z);
            float distance = relativeWorldPos2D.magnitude;

            // Chỉ hiển thị waypoint nếu nó nằm trong bán kính nhìn của minimap
            if (distance <= minimapViewRadius)
            {
                // Chuyển đổi vị trí tương đối thế giới sang tọa độ UI
                float uiX = relativeWorldPos2D.x * pixelsPerWorldUnit;
                float uiY = relativeWorldPos2D.y * pixelsPerWorldUnit;

                // Clamp vị trí UI vào bên trong khung minimap (nếu cần)
                float mapUIHalfWidth = minimapContainerRect.rect.width / 2f;
                float mapUIHalfHeight = minimapContainerRect.rect.height / 2f;
                uiX = Mathf.Clamp(uiX, -mapUIHalfWidth, mapUIHalfWidth);
                uiY = Mathf.Clamp(uiY, -mapUIHalfHeight, mapUIHalfHeight);

                // Đảm bảo waypoint được bật nếu nó đang active và trong phạm vi
                if (!waypointUI.gameObject.activeSelf) waypointUI.gameObject.SetActive(true);
                waypointUI.SetMapUIPosition(new Vector2(uiX, uiY), waypointUI.maxScale); // Sử dụng maxScale cho minimap
                waypointUI.UpdateDistanceText();
            }
            else
            {
                // Ẩn nếu ngoài phạm vi
                if (waypointUI.gameObject.activeSelf) waypointUI.gameObject.SetActive(false);
            }
        }
    }
}
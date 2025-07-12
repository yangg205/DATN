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

        // Sử dụng kích thước của container (khung hiển thị thực tế của minimap trên UI)
        float mapUIHalfWidth = minimapContainerRect.rect.width / 2f;
        float mapUIHalfHeight = minimapContainerRect.rect.height / 2f;

        // Tính toán tỉ lệ chuyển đổi từ đơn vị thế giới sang đơn vị UI (pixel)
        // worldUnitPerUIPixel = (minimapCamera.orthographicSize * 2f) / minimapUIContainerSize;
        // pixelsPerWorldUnit = minimapUIContainerSize / (minimapCamera.orthographicSize * 2f);

        // orthographicSize là một nửa chiều cao của khung nhìn. Khung nhìn là hình vuông.
        // Vậy chiều rộng/chiều cao thế giới mà camera nhìn thấy là 2 * minimapCamera.orthographicSize
        float worldVisibleSize = minimapCamera.orthographicSize * 2f;
        float pixelsPerWorldUnit = minimapContainerRect.rect.width / worldVisibleSize; // Giả sử container hình vuông

        Vector3 playerWorldPos = WaypointManager.Instance.playerTransform.position;

        foreach (var entry in WaypointManager.Instance.minimapWaypointUIs)
        {
            WaypointUI waypointUI = entry.Value;
            Waypoint waypointData = entry.Value.GetWaypointData();

            if (waypointUI == null || waypointUI.gameObject == null)
            {
                Debug.LogWarning($"[MinimapController] minimapWaypointUI for ID '{entry.Key}' is null or its GameObject is null. Skipping.");
                continue;
            }

            // Chỉ hiển thị waypoint nếu nó là ACTIVE WAYPOINT.
            // Nếu bạn muốn hiển thị TẤT CẢ các waypoint trong phạm vi, hãy bỏ `&& isActiveWaypoint`
            bool isActiveWaypoint = (WaypointManager.Instance.activeWaypoint != null && WaypointManager.Instance.activeWaypoint.id == waypointData.id);

            if (isActiveWaypoint)
            {
                // Vị trí waypoint tương đối so với người chơi (trung tâm của minimap)
                // Chỉ lấy thành phần XZ
                Vector3 relativeWorldPos = waypointData.worldPosition - playerWorldPos;
                Vector2 relativeWorldPos2D = new Vector2(relativeWorldPos.x, relativeWorldPos.z);

                float distance = relativeWorldPos2D.magnitude;

                // Ẩn waypoint nếu nó nằm ngoài bán kính nhìn của minimap
                if (distance > minimapViewRadius)
                {
                    if (waypointUI.gameObject.activeSelf) waypointUI.gameObject.SetActive(false);
                    continue; // Bỏ qua các tính toán khác nếu đã ẩn
                }

                // Chuyển đổi vị trí tương đối thế giới sang tọa độ UI
                // uiX và uiY là offset từ tâm của UI container (0,0)
                float uiX = relativeWorldPos2D.x * pixelsPerWorldUnit;
                float uiY = relativeWorldPos2D.y * pixelsPerWorldUnit;

                // Clamp vị trí UI vào bên trong khung minimap (nếu cần, để tránh vượt ra ngoài rìa)
                // Điều này hữu ích nếu bạn muốn waypoint dừng lại ở rìa bản đồ thay vì biến mất
                // Nhưng với logic khoảng cách và setActive(false) ở trên, việc này ít cần thiết hơn.
                uiX = Mathf.Clamp(uiX, -mapUIHalfWidth, mapUIHalfWidth);
                uiY = Mathf.Clamp(uiY, -mapUIHalfHeight, mapUIHalfHeight);

                // Đảm bảo waypoint được bật nếu nó đang active và trong phạm vi
                if (!waypointUI.gameObject.activeSelf) waypointUI.gameObject.SetActive(true);
                waypointUI.SetMapUIPosition(new Vector2(uiX, uiY), waypointUI.maxScale); // Sử dụng maxScale cho minimap
                waypointUI.UpdateDistanceText();
            }
            else
            {
                // Ẩn nếu không phải là active waypoint
                if (waypointUI.gameObject.activeSelf) waypointUI.gameObject.SetActive(false);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using ND; // Giữ namespace của bạn nếu có

public class LargeMapController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject largeMapPanel;
    public RawImage largeMapDisplayRaw;
    public RectTransform playerIconLargeMapRectTransform;

    [Header("World References")]
    public Camera largeMapCamera;
    public Transform playerMainCameraTransform;

    [Header("Waypoint Creation")]
    public Sprite customWaypointIcon;
    private string currentCustomWaypointId = "PlayerCustomMarker"; // ID cố định cho custom marker người chơi

    private int terrainLayerMask;
    private InputHandler playerInputHandler; // Nếu bạn dùng InputHandler để khóa input

    private void Start()
    {
        CheckRequiredReferences();

        playerInputHandler = FindObjectOfType<InputHandler>(); // Tìm InputHandler
        if (playerInputHandler == null)
        {
            Debug.LogWarning("[LargeMapController] InputHandler not found in scene. Player input lock/unlock may not work.");
        }

        int terrainLayerIndex = LayerMask.NameToLayer("Terrain");
        if (terrainLayerIndex == -1)
        {
            Debug.LogError("[LargeMapController] Layer 'Terrain' not found! Please create it for map raycasting.");
            terrainLayerMask = 0;
        }
        else
        {
            terrainLayerMask = 1 << terrainLayerIndex;
            Debug.Log($"[LargeMapController] Terrain Layer Mask set to: {terrainLayerMask}");
        }

        if (largeMapPanel != null)
        {
            largeMapPanel.SetActive(false);
            if (largeMapCamera != null) largeMapCamera.enabled = false;
        }

        SetupLargeMapCamera();

        // Đảm bảo icon người chơi là con của parent waypoint trên bản đồ lớn
        if (playerIconLargeMapRectTransform != null && WaypointManager.Instance != null && WaypointManager.Instance.largeMapWaypointsParent != null)
        {
            playerIconLargeMapRectTransform.SetParent(WaypointManager.Instance.largeMapWaypointsParent, false);
            Debug.Log("[LargeMapController] Player icon parent set to largeMapWaypointsParent.");
        }
        else
        {
            Debug.LogError("[LargeMapController] Cannot parent player icon: playerIconLargeMapRectTransform or WaypointManager.Instance or its parent is null.");
        }
    }

    private void Update()
    {
        HandleMapToggleInput();

        if (largeMapPanel != null && largeMapPanel.activeSelf)
        {
            HandleWaypointCreationInput();
            UpdatePlayerIconPositionAndRotation(); // Cập nhật vị trí icon người chơi
            UpdateWaypointUIsOnLargeMap(); // Cập nhật Waypoint UI khi bản đồ lớn đang mở
        }
    }

    private void CheckRequiredReferences()
    {
        if (largeMapPanel == null) Debug.LogError("[LargeMapController] largeMapPanel is not assigned!");
        if (largeMapDisplayRaw == null) Debug.LogError("[LargeMapController] largeMapDisplayRaw is not assigned!");
        if (playerIconLargeMapRectTransform == null) Debug.LogError("[LargeMapController] playerIconLargeMapRectTransform is not assigned!");
        if (largeMapCamera == null) Debug.LogError("[LargeMapController] largeMapCamera is not assigned!");
        if (playerMainCameraTransform == null) Debug.LogError("[LargeMapController] playerMainCameraTransform is not assigned!");
        if (customWaypointIcon == null) Debug.LogWarning("[LargeMapController] customWaypointIcon is not assigned. Custom markers will not have an icon.");

        // Kiểm tra các tham chiếu từ WaypointManager
        if (WaypointManager.Instance == null)
        {
            Debug.LogError("[LargeMapController] WaypointManager.Instance is NULL! Make sure it exists and runs its Awake method.");
            return;
        }
        if (WaypointManager.Instance.activeTerrain == null) Debug.LogError("[LargeMapController] WaypointManager.Instance.activeTerrain is not assigned!");
        if (WaypointManager.Instance.playerTransform == null) Debug.LogError("[LargeMapController] WaypointManager.Instance.playerTransform is not assigned!");
        if (WaypointManager.Instance.largeMapWaypointsParent == null) Debug.LogError("[LargeMapController] WaypointManager.Instance.largeMapWaypointsParent is not assigned!");
    }

    private void SetupLargeMapCamera()
    {
        if (largeMapCamera == null || WaypointManager.Instance == null || WaypointManager.Instance.activeTerrain == null) return;

        Terrain terrain = WaypointManager.Instance.activeTerrain;
        float terrainWidth = terrain.terrainData.size.x;
        float terrainLength = terrain.terrainData.size.z;

        float maxDimension = Mathf.Max(terrainWidth, terrainLength);
        largeMapCamera.orthographicSize = maxDimension / 2f;

        largeMapCamera.transform.position = new Vector3(
            terrain.transform.position.x + terrainWidth / 2f,
            terrain.transform.position.y + maxDimension * 0.75f, // Đặt cao hơn để nhìn toàn cảnh
            terrain.transform.position.z + terrainLength / 2f
        );
        largeMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Debug.Log($"[LargeMapController] Large Map Camera setup. Orthographic Size: {largeMapCamera.orthographicSize}");
    }

    private void HandleMapToggleInput()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            bool isActive = !largeMapPanel.activeSelf;
            largeMapPanel.SetActive(isActive);
            if (largeMapCamera != null) largeMapCamera.enabled = isActive;

            if (isActive)
            {
                SetupLargeMapCamera(); // Cần setup lại camera nếu terrain có thể thay đổi
                UpdatePlayerIconPositionAndRotation(); // Cập nhật ngay khi mở
                UpdateWaypointUIsOnLargeMap(); // Cập nhật tất cả waypoint UI khi mở bản đồ
                Debug.Log("[LargeMapController] Large map toggled ON.");
            }
            else
            {
                // Khi đóng bản đồ, ẩn tất cả waypoint UI trên bản đồ lớn
                if (WaypointManager.Instance != null && WaypointManager.Instance.largeMapWaypointUIs != null)
                {
                    foreach (var ui in WaypointManager.Instance.largeMapWaypointUIs.Values)
                    {
                        if (ui != null && ui.gameObject != null) ui.gameObject.SetActive(false);
                    }
                }
                Debug.Log("[LargeMapController] Large map toggled OFF. All large map waypoints hidden.");
            }

            SetPlayerInputLocked(isActive);
        }
    }

    private void HandleWaypointCreationInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Kiểm tra xem con trỏ có đang ở trên UI element nào khác không
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                bool clickedOnLargeMapRawImage = false;
                foreach (var r in results)
                {
                    if (r.gameObject == largeMapDisplayRaw.gameObject)
                    {
                        clickedOnLargeMapRawImage = true;
                        break;
                    }
                }

                if (!clickedOnLargeMapRawImage)
                {
                    Debug.Log("[LargeMapController] Click was on another UI element, not the large map image. Ignoring waypoint creation.");
                    return;
                }
            }

            Vector2 localPoint;
            // Xác định camera để chuyển đổi ScreenPointToLocalPointInRectangle
            Camera uiCam = largeMapDisplayRaw.canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : largeMapDisplayRaw.canvas.worldCamera;

            bool validPoint = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                largeMapDisplayRaw.rectTransform, Input.mousePosition, uiCam, out localPoint);

            if (!validPoint)
            {
                Debug.LogWarning("[LargeMapController] Cannot convert screen point to local point on map. Aborting waypoint creation.");
                return;
            }

            // Chuyển đổi tọa độ cục bộ trên RawImage sang Normalized Viewport Point của largeMapCamera
            // localPoint là từ tâm của rect, cần chuyển về 0-1
            float normX = (localPoint.x / largeMapDisplayRaw.rectTransform.rect.width) + 0.5f;
            float normY = (localPoint.y / largeMapDisplayRaw.rectTransform.rect.height) + 0.5f;

            // Debug.Log($"[LargeMapController] Clicked local: {localPoint}, Normalized: ({normX}, {normY})");

            Ray ray = largeMapCamera.ViewportPointToRay(new Vector3(normX, normY, 0f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f, terrainLayerMask))
            {
                Vector3 hitPoint = hit.point;
                Debug.Log($"[LargeMapController] Clicked terrain at {hitPoint}"); // Dòng 203 trong code gốc của bạn

                if (WaypointManager.Instance != null)
                {
                    // KHÔNG GỌI RemoveWaypoint ở đây nữa!
                    // WaypointManager.AddWaypoint giờ sẽ tự động cập nhật nếu ID tồn tại.

                    Waypoint newWaypoint = new Waypoint(
                        currentCustomWaypointId, // Vẫn sử dụng cùng ID để cập nhật
                        "Điểm đánh dấu tùy chỉnh",
                        hitPoint,
                        WaypointType.CustomMarker,
                        customWaypointIcon
                    );

                    // Thêm hoặc cập nhật waypoint và đặt nó là active
                    WaypointManager.Instance.AddWaypoint(newWaypoint, true); // Dòng 223 trong code gốc của bạn
                    Debug.Log($"[LargeMapController] Custom waypoint '{newWaypoint.id}' added/updated and set active.");

                    // Cần cập nhật ngay UI trên bản đồ lớn sau khi thêm waypoint mới
                    UpdateWaypointUIsOnLargeMap();
                }
                else
                {
                    Debug.LogWarning("[LargeMapController] WaypointManager Instance is NULL. Cannot create waypoint.");
                }
            }
            else
            {
                Debug.LogWarning("[LargeMapController] Raycast from map click did not hit any terrain object on 'Terrain' layer.");
            }
        }
    }

    private void UpdatePlayerIconPositionAndRotation()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.playerTransform == null ||
            playerIconLargeMapRectTransform == null || largeMapDisplayRaw == null || WaypointManager.Instance.activeTerrain == null ||
            playerMainCameraTransform == null)
        {
            // Debug.LogWarning("[LargeMapController] Missing references for player icon update. Skipping.");
            return;
        }

        Terrain terrain = WaypointManager.Instance.activeTerrain;
        Transform playerTransform = WaypointManager.Instance.playerTransform;

        float terrainWidth = terrain.terrainData.size.x;
        float terrainLength = terrain.terrainData.size.z;
        Vector3 terrainPos = terrain.transform.position;

        float normalizedX = (playerTransform.position.x - terrainPos.x) / terrainWidth;
        float normalizedY = (playerTransform.position.z - terrainPos.z) / terrainLength; // Z của world map là Y trên 2D map

        float uiX = (normalizedX - 0.5f) * largeMapDisplayRaw.rectTransform.rect.width;
        float uiY = (normalizedY - 0.5f) * largeMapDisplayRaw.rectTransform.rect.height;

        playerIconLargeMapRectTransform.anchoredPosition = new Vector2(uiX, uiY);
        playerIconLargeMapRectTransform.localEulerAngles = new Vector3(0, 0, -playerMainCameraTransform.eulerAngles.y);
    }

    private void UpdateWaypointUIsOnLargeMap()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.largeMapWaypointUIs == null ||
            largeMapDisplayRaw == null || WaypointManager.Instance.activeTerrain == null) return;

        // Large Map sẽ hiển thị tất cả các waypoint mà WaypointManager đang theo dõi
        foreach (var entry in WaypointManager.Instance.largeMapWaypointUIs)
        {
            WaypointUI waypointUI = entry.Value;
            Waypoint waypointData = entry.Value.GetWaypointData(); // Lấy data từ WaypointUI

            if (waypointUI == null || waypointUI.gameObject == null)
            {
                Debug.LogWarning($"[LargeMapController] largeMapWaypointUI for ID '{entry.Key}' is null or its GameObject is null. Skipping.");
                continue;
            }

            // Chỉ hiển thị waypoint nếu nó có trong danh sách activeWaypointsData của WaypointManager
            if (WaypointManager.Instance.activeWaypointsData.ContainsKey(waypointData.id))
            {
                Vector2 uiPos = ConvertWorldPositionToMapUI(waypointData.worldPosition);
                float scale = waypointUI.maxScale; // Sử dụng maxScale từ WaypointUI prefab

                // Đảm bảo UI được bật
                if (!waypointUI.gameObject.activeSelf)
                {
                    waypointUI.gameObject.SetActive(true);
                    // Debug.Log($"[LargeMapController] Showing large map UI for '{waypointData.id}'.");
                }
                waypointUI.SetMapUIPosition(uiPos, scale);
                waypointUI.UpdateDistanceText();
            }
            else
            {
                // Nếu không có trong activeWaypointsData (có thể đã bị xóa), ẩn UI
                if (waypointUI.gameObject.activeSelf)
                {
                    waypointUI.gameObject.SetActive(false);
                    // Debug.Log($"[LargeMapController] Hiding large map UI for '{waypointData.id}' (not in active data).");
                }
            }
        }
    }

    private Vector2 ConvertWorldPositionToMapUI(Vector3 worldPos)
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.activeTerrain == null || largeMapDisplayRaw == null)
        {
            Debug.LogError("[LargeMapController] Missing WaypointManager or Terrain or largeMapDisplayRaw for ConvertWorldPositionToMapUI!");
            return Vector2.zero;
        }

        Terrain terrain = WaypointManager.Instance.activeTerrain;

        float terrainMinX = terrain.transform.position.x;
        float terrainMaxX = terrainMinX + terrain.terrainData.size.x;
        float terrainMinZ = terrain.transform.position.z;
        float terrainMaxZ = terrainMinZ + terrain.terrainData.size.z;

        RectTransform mapRectTransform = largeMapDisplayRaw.rectTransform;
        float mapWidth = mapRectTransform.rect.width;
        float mapHeight = mapRectTransform.rect.height;

        float normalizedX = Mathf.InverseLerp(terrainMinX, terrainMaxX, worldPos.x);
        float normalizedZ = Mathf.InverseLerp(terrainMinZ, terrainMaxZ, worldPos.z);

        float uiX = (normalizedX - 0.5f) * mapWidth;
        float uiY = (normalizedZ - 0.5f) * mapHeight;

        return new Vector2(uiX, uiY);
    }

    private void SetPlayerInputLocked(bool locked)
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.enabled = !locked; // Vô hiệu hóa input handler của người chơi
            Debug.Log($"[LargeMapController] Player InputHandler enabled: {!locked}");
        }
        else
        {
            // Fallback for basic cursor management if InputHandler isn't present
            if (locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Debug.Log("[LargeMapController] Cursor unlocked and visible.");
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Debug.Log("[LargeMapController] Cursor locked and hidden.");
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using ND; // Giữ namespace của bạn nếu có
using System.Linq;

public class LargeMapController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo và thả GameObject Panel chính của bản đồ lớn vào đây từ Hierarchy.")]
    public GameObject largeMapPanel;
    [Tooltip("Kéo và thả RawImage sẽ hiển thị bản đồ từ camera vào đây từ Hierarchy.")]
    public RawImage largeMapDisplayRaw;
    [Tooltip("Kéo và thả RectTransform của icon người chơi trên bản đồ lớn vào đây từ Hierarchy. Đảm bảo nó là con của LargeMap_Display_Raw.")]
    public RectTransform playerIconLargeMapRectTransform;

    [Header("World References")]
    [Tooltip("Kéo và thả Camera sẽ render bản đồ lớn vào đây từ Hierarchy. Đảm bảo Target Texture của camera này được đặt thành một RenderTexture và RenderTexture đó được gán cho Large Map Display Raw.")]
    public Camera largeMapCamera;
    [Tooltip("Kéo và thả Transform của người chơi chính (hoặc camera chính của người chơi) vào đây.")]
    public Transform playerMainCameraTransform;

    [Header("Waypoint Creation")]
    [Tooltip("Sprite sẽ được sử dụng cho các waypoint tùy chỉnh do người chơi đặt.")]
    public Sprite customWaypointIcon;
    private string currentCustomWaypointId = "PlayerCustomMarker"; // ID cố định cho custom marker người chơi

    private int terrainLayerMask;
    private InputHandler playerInputHandler; // Nếu bạn dùng InputHandler để khóa input

    // Biến để lưu trữ Bounding Box của tất cả terrains
    private Bounds terrainsBounds;

    private void Start()
    {
        CheckRequiredReferences();

        playerInputHandler = FindObjectOfType<InputHandler>();
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

        // Đảm bảo panel và camera bản đồ bị tắt ngay từ đầu
        if (largeMapPanel != null)
        {
            largeMapPanel.SetActive(false);
        }
        if (largeMapCamera != null) // Đảm bảo camera cũng bị tắt
        {
            largeMapCamera.enabled = false;
        }

        // Tính toán Bounding Box của tất cả terrains khi Start
        CalculateTerrainsBounds();
        // SetupLargeMapCamera() sẽ được gọi khi bản đồ được bật lần đầu tiên
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
        if (largeMapPanel == null) Debug.LogError("[LargeMapController] largeMapPanel is not assigned! Please drag the main map Panel GameObject here.");
        if (largeMapDisplayRaw == null) Debug.LogError("[LargeMapController] largeMapDisplayRaw is not assigned! Please drag the RawImage component used for map display here.");
        if (playerIconLargeMapRectTransform == null) Debug.LogError("[LargeMapController] playerIconLargeMapRectTransform is not assigned! Please drag and drop your player icon UI element here.");
        if (largeMapCamera == null) Debug.LogError("[LargeMapController] largeMapCamera is not assigned! Please drag and drop the large map Camera from Hierarchy. Ensure its Target Texture is set to a RenderTexture, and that RenderTexture is assigned to Large Map Display Raw.");
        if (playerMainCameraTransform == null) Debug.LogError("[LargeMapController] playerMainCameraTransform is not assigned!");
        if (customWaypointIcon == null) Debug.LogWarning("[LargeMapController] customWaypointIcon is not assigned. Custom markers will not have an icon.");

        if (WaypointManager.Instance == null)
        {
            Debug.LogError("[LargeMapController] WaypointManager.Instance is NULL! Make sure it exists and runs its Awake method.");
            return;
        }
        if (WaypointManager.Instance.terrains == null || WaypointManager.Instance.terrains.Count == 0 || WaypointManager.Instance.terrains.Any(t => t == null))
        {
            Debug.LogError("[LargeMapController] WaypointManager.Instance.terrains is NULL or EMPTY or contains NULL elements! Large map features relying on terrain data may not function.");
        }

        if (WaypointManager.Instance.playerTransform == null) Debug.LogError("[LargeMapController] WaypointManager.Instance.playerTransform is not assigned!");
        if (WaypointManager.Instance.largeMapWaypointsParent == null) Debug.LogError("[LargeMapController] WaypointManager.Instance.largeMapWaypointsParent is not assigned! This should be the parent for all map UI icons, e.g., LargeMap_Display_Raw's RectTransform.");

        // Thêm kiểm tra cho RenderTexture
        if (largeMapCamera != null && largeMapCamera.targetTexture == null)
        {
            Debug.LogError("[LargeMapController] largeMapCamera's Target Texture is not assigned! It must be set to a RenderTexture for the map to display.");
        }
        if (largeMapDisplayRaw != null && largeMapDisplayRaw.texture == null)
        {
            Debug.LogError("[LargeMapController] largeMapDisplayRaw's Texture is not assigned! It should be the same RenderTexture as largeMapCamera's Target Texture.");
        }
    }

    // Tính toán Bounding Box bao quanh tất cả terrains
    private void CalculateTerrainsBounds()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.terrains == null || WaypointManager.Instance.terrains.Count == 0)
        {
            terrainsBounds = new Bounds(Vector3.zero, Vector3.zero);
            Debug.LogWarning("[LargeMapController] No terrains to calculate bounds for. Bounds will be zero.");
            return;
        }

        // Khởi tạo bounds với terrain đầu tiên
        // Bounds cần được khởi tạo với một điểm và sau đó mở rộng
        Vector3 firstTerrainCenter = WaypointManager.Instance.terrains[0].transform.position + WaypointManager.Instance.terrains[0].terrainData.size / 2f;
        terrainsBounds = new Bounds(firstTerrainCenter, WaypointManager.Instance.terrains[0].terrainData.size);

        for (int i = 1; i < WaypointManager.Instance.terrains.Count; i++)
        {
            Terrain t = WaypointManager.Instance.terrains[i];
            if (t != null)
            {
                // Thêm từng góc của terrain vào bounds để đảm bảo bao phủ đầy đủ
                Vector3 min = t.transform.position;
                Vector3 max = t.transform.position + t.terrainData.size;
                terrainsBounds.Encapsulate(min);
                terrainsBounds.Encapsulate(max);
            }
        }
        Debug.Log($"[LargeMapController] Calculated Terrains Bounds: Center={terrainsBounds.center}, Size={terrainsBounds.size}");
    }

    private void SetupLargeMapCamera()
    {
        if (largeMapCamera == null || WaypointManager.Instance == null || WaypointManager.Instance.terrains == null || WaypointManager.Instance.terrains.Count == 0 || terrainsBounds.size == Vector3.zero) return;

        // Tính toán kích thước camera dựa trên kích thước của terrainsBounds
        float maxDimension = Mathf.Max(terrainsBounds.size.x, terrainsBounds.size.z);
        largeMapCamera.orthographicSize = maxDimension / 2f; // Đối với orthographic camera, size là một nửa chiều cao
        Debug.Log($"[LargeMapController] largeMapCamera.orthographicSize set to {largeMapCamera.orthographicSize} (based on terrains bounds).");

        // Đặt vị trí camera ở trung tâm của Bounding Box và nhìn thẳng xuống
        largeMapCamera.transform.position = new Vector3(
            terrainsBounds.center.x,
            terrainsBounds.center.y + maxDimension, // Đặt camera đủ cao để bao quát tất cả terrain
            terrainsBounds.center.z
        );
        largeMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Nhìn thẳng xuống
        Debug.Log($"[LargeMapController] Large Map Camera repositioned to cover terrains bounds. Position: {largeMapCamera.transform.position}");
    }

    private void HandleMapToggleInput()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log($"[LargeMapController] Before toggle: largeMapPanel.activeSelf = {largeMapPanel.activeSelf}"); // Thêm dòng này
            bool isActive = !largeMapPanel.activeSelf;
            largeMapPanel.SetActive(isActive);

            // Bật/tắt camera chỉ khi panel được bật/tắt
            if (largeMapCamera != null)
            {
                largeMapCamera.enabled = isActive;
            }

            if (WaypointManager.Instance != null)
            {
                WaypointManager.Instance.SetLargeMapPanelActive(isActive);
            }

            if (isActive)
            {
                // Luôn tính lại Bounds và Setup Camera mỗi khi mở map
                CalculateTerrainsBounds();
                SetupLargeMapCamera();
                UpdatePlayerIconPositionAndRotation();
                UpdateWaypointUIsOnLargeMap();
                Debug.Log("[LargeMapController] Large map toggled ON.");
            }
            else
            {
                Debug.Log("[LargeMapController] Large map toggled OFF. WaypointManager informed.");
            }

            SetPlayerInputLocked(isActive);
            Debug.Log($"[LargeMapController] After toggle: largeMapPanel.activeSelf = {largeMapPanel.activeSelf}"); // Thêm dòng này
        }
    }

    private void HandleWaypointCreationInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[LargeMapController] Mouse Left Click detected while large map is active.");

            Camera uiCam = largeMapDisplayRaw.canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : largeMapDisplayRaw.canvas.worldCamera;
            if (uiCam == null && largeMapDisplayRaw.canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogError("[LargeMapController] uiCam is null but Canvas Render Mode is not Screen Space Overlay. This is an issue with Canvas setup!");
                return;
            }

            Vector2 localPoint;
            bool validPointOnMapUI = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                largeMapDisplayRaw.rectTransform, Input.mousePosition, uiCam, out localPoint);

            if (!validPointOnMapUI)
            {
                Debug.Log("[LargeMapController] Click was OUTSIDE the RectTransform of LargeMap_Display_Raw. Aborting waypoint creation.");
                // Thêm kiểm tra EventSystem để hiểu tại sao nó không được xem là click vào map,
                // nhưng không dùng nó để chặn logic chính.
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    Debug.Log("[LargeMapController] However, EventSystem reports pointer is OVER another UI object.");
                }
                return;
            }

            Debug.Log($"[LargeMapController] Click was INSIDE LargeMap_Display_Raw RectTransform. Local point: {localPoint}");

            // Chuyển đổi localPoint (từ RectTransform của RawImage) về normalized Viewport coordinates của largeMapCamera
            float normX = (localPoint.x / largeMapDisplayRaw.rectTransform.rect.width) + 0.5f;
            float normY = (localPoint.y / largeMapDisplayRaw.rectTransform.rect.height) + 0.5f;

            Debug.Log($"[LargeMapController] Normalized Viewport coordinates for raycast: ({normX}, {normY})");

            // Tạo một tia (Ray) từ camera bản đồ lớn dựa trên vị trí click trên UI
            Ray ray = largeMapCamera.ViewportPointToRay(new Vector3(normX, normY, 0f));
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 5f); // Vẽ Ray để debug trong Scene view

            if (Physics.Raycast(ray, out hit, 1000f, terrainLayerMask))
            {
                Vector3 hitPoint = hit.point;
                Debug.Log($"[LargeMapController] Raycast HIT terrain at {hitPoint}. Terrain name: {hit.collider.gameObject.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

                if (WaypointManager.Instance != null)
                {
                    Waypoint newWaypoint = new Waypoint(
                        currentCustomWaypointId,
                        "Điểm đánh dấu tùy chỉnh",
                        hitPoint,
                        WaypointType.CustomMarker,
                        customWaypointIcon
                    );

                    WaypointManager.Instance.AddWaypoint(newWaypoint, true);
                    Debug.Log($"[LargeMapController] Custom waypoint '{newWaypoint.id}' added/updated and set active.");

                    UpdateWaypointUIsOnLargeMap();
                }
                else
                {
                    Debug.LogWarning("[LargeMapController] WaypointManager Instance is NULL. Cannot create waypoint.");
                }
            }
            else
            {
                Debug.LogWarning("[LargeMapController] Raycast from map click did not hit any terrain object on 'Terrain' layer. Ensure terrains have colliders and are on the 'Terrain' layer, and that the terrainLayerMask is correct.");
                // Log chi tiết hơn về layer mask
                Debug.Log($"[LargeMapController] Current terrainLayerMask: {terrainLayerMask} (Layer 'Terrain' index: {LayerMask.NameToLayer("Terrain")})");
                // Log vị trí và hướng ray
                Debug.Log($"[LargeMapController] Ray Origin: {ray.origin}, Direction: {ray.direction}");
            }
        }
    }

    private void UpdatePlayerIconPositionAndRotation()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.playerTransform == null ||
            playerIconLargeMapRectTransform == null || largeMapDisplayRaw == null ||
            WaypointManager.Instance.terrains == null || WaypointManager.Instance.terrains.Count == 0 ||
            playerMainCameraTransform == null || terrainsBounds.size == Vector3.zero)
        {
            return;
        }

        Transform playerTransform = WaypointManager.Instance.playerTransform;

        // Sử dụng terrainsBounds để tính toán vị trí normalized của người chơi trên toàn bộ khu vực terrain
        float normalizedX = Mathf.InverseLerp(terrainsBounds.min.x, terrainsBounds.max.x, playerTransform.position.x);
        float normalizedZ = Mathf.InverseLerp(terrainsBounds.min.z, terrainsBounds.max.z, playerTransform.position.z);

        RectTransform mapRectTransform = largeMapDisplayRaw.rectTransform;
        float uiX = (normalizedX - 0.5f) * mapRectTransform.rect.width;
        float uiY = (normalizedZ - 0.5f) * mapRectTransform.rect.height; // Z của world map tương ứng với Y của UI

        playerIconLargeMapRectTransform.anchoredPosition = new Vector2(uiX, uiY);
        // Xoay icon để phù hợp với hướng nhìn của người chơi (dựa trên playerMainCameraTransform)
        playerIconLargeMapRectTransform.localEulerAngles = new Vector3(0, 0, -playerMainCameraTransform.eulerAngles.y);
    }

    private void UpdateWaypointUIsOnLargeMap()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.largeMapWaypointUIs == null ||
            largeMapDisplayRaw == null || WaypointManager.Instance.terrains == null || WaypointManager.Instance.terrains.Count == 0 || terrainsBounds.size == Vector3.zero) return;

        foreach (var entry in WaypointManager.Instance.largeMapWaypointUIs)
        {
            WaypointUI waypointUI = entry.Value;
            Waypoint waypointData = entry.Value.GetWaypointData();

            if (waypointUI == null || waypointUI.gameObject == null)
            {
                Debug.LogWarning($"[LargeMapController] largeMapWaypointUI for ID '{entry.Key}' is null or its GameObject is null. Skipping.");
                continue;
            }

            // Chỉ cập nhật các waypoint đang hoạt động
            if (WaypointManager.Instance.activeWaypointsData.ContainsKey(waypointData.id))
            {
                Vector2 uiPos = ConvertWorldPositionToMapUI(waypointData.worldPosition);
                float scale = waypointUI.maxScale;

                if (!waypointUI.gameObject.activeSelf)
                {
                    waypointUI.gameObject.SetActive(true);
                }
                waypointUI.SetMapUIPosition(uiPos, scale);
                waypointUI.UpdateDistanceText();
            }
            else
            {
                // Ẩn waypoint UI nếu nó không còn hoạt động
                if (waypointUI.gameObject.activeSelf)
                {
                    waypointUI.gameObject.SetActive(false);
                }
            }
        }
    }

    private Vector2 ConvertWorldPositionToMapUI(Vector3 worldPos)
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.terrains == null || WaypointManager.Instance.terrains.Count == 0 || largeMapDisplayRaw == null || terrainsBounds.size == Vector3.zero)
        {
            Debug.LogError("[LargeMapController] Missing WaypointManager or Terrain or largeMapDisplayRaw or Terrains Bounds for ConvertWorldPositionToMapUI!");
            return Vector2.zero;
        }

        // Sử dụng terrainsBounds đã tính toán để chuẩn hóa vị trí thế giới
        float normalizedX = Mathf.InverseLerp(terrainsBounds.min.x, terrainsBounds.max.x, worldPos.x);
        float normalizedZ = Mathf.InverseLerp(terrainsBounds.min.z, terrainsBounds.max.z, worldPos.z);

        RectTransform mapRectTransform = largeMapDisplayRaw.rectTransform;
        float mapWidth = mapRectTransform.rect.width;
        float mapHeight = mapRectTransform.rect.height;

        // Chuyển đổi vị trí normalized thành vị trí trên UI (centered)
        float uiX = (normalizedX - 0.5f) * mapWidth;
        float uiY = (normalizedZ - 0.5f) * mapHeight; // Z của thế giới là Y trên UI bản đồ

        return new Vector2(uiX, uiY);
    }

    private void SetPlayerInputLocked(bool locked)
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.enabled = !locked;
            Debug.Log($"[LargeMapController] Player InputHandler enabled: {!locked}");
        }
        else
        {
            // Fallback nếu không tìm thấy InputHandler
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
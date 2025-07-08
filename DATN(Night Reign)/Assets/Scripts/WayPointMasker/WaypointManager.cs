using UnityEngine;
using System.Collections.Generic;
using System; // For events

// Không có namespace

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [Header("Waypoint Prefabs (Must have WaypointUI / WorldWaypoint component)")]
    public GameObject minimapWaypointUIPrefab;
    public GameObject compassWaypointUIPrefab;
    public GameObject hudWaypointUIPrefab;
    public GameObject largeMapWaypointUIPrefab;
    public GameObject worldWaypointPrefab;

    [Header("UI Parent Transforms (RectTransform in Canvas)")]
    public RectTransform minimapWaypointsParent;
    public RectTransform compassWaypointsParent;
    public RectTransform hudWaypointsParent;
    public RectTransform largeMapWaypointsParent;

    [Header("Player and Map References")]
    public Transform playerTransform;
    public Camera mainCamera; // Camera chính của người chơi
    public Camera minimapCamera; // Camera của Minimap (Orthographic, nhìn thẳng xuống)
    public Camera largeMapCamera; // Camera của Large Map (Orthographic, nhìn thẳng xuống)
    public Terrain activeTerrain;

    // Các Dictionary public readonly để các script khác có thể đọc (nhưng không sửa trực tiếp)
    public readonly Dictionary<string, WaypointUI> compassWaypointUIs = new Dictionary<string, WaypointUI>();
    public readonly Dictionary<string, WaypointUI> hudWaypointUIs = new Dictionary<string, WaypointUI>();
    public readonly Dictionary<string, WaypointUI> minimapWaypointUIs = new Dictionary<string, WaypointUI>();
    public readonly Dictionary<string, WaypointUI> largeMapWaypointUIs = new Dictionary<string, WaypointUI>();
    public readonly Dictionary<string, GameObject> worldWaypointsInstances = new Dictionary<string, GameObject>();

    // Dictionary chứa tất cả các Waypoint data đang hoạt động (cho phép truy cập waypoint data từ bên ngoài)
    public readonly Dictionary<string, Waypoint> activeWaypointsData = new Dictionary<string, Waypoint>();

    // Events để thông báo khi waypoint được thêm/xóa
    public event Action<Waypoint> OnWaypointAdded;
    public event Action<string> OnWaypointRemoved;

    // NEW: Biến để lưu trữ waypoint đang hoạt động và sự kiện liên quan
    private Waypoint _activeWaypoint;
    public event Action<Waypoint> OnActiveWaypointChanged; // Sự kiện cho UIManager

    // Các thông số Terrain để tính toán vị trí trên bản đồ
    private float terrainWidth, terrainLength, terrainMinX, terrainMinZ;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (activeTerrain != null)
        {
            terrainWidth = activeTerrain.terrainData.size.x;
            terrainLength = activeTerrain.terrainData.size.z;
            terrainMinX = activeTerrain.transform.position.x;
            terrainMinZ = activeTerrain.transform.position.z;
            Debug.Log($"[WaypointManager] Terrain Data Loaded: Width={terrainWidth}, Length={terrainLength}, MinX={terrainMinX}, MinZ={terrainMinZ}");
        }
        else
        {
            Debug.LogWarning("[WaypointManager] Active Terrain is not assigned. Map waypoint positioning might be incorrect.");
        }

        CheckRequiredReferences();
    }

    private void CheckRequiredReferences()
    {
        if (minimapWaypointUIPrefab == null) Debug.LogError("Minimap Waypoint UI Prefab is not assigned in WaypointManager!");
        if (compassWaypointUIPrefab == null) Debug.LogError("Compass Waypoint UI Prefab is not assigned in WaypointManager!");
        if (hudWaypointUIPrefab == null) Debug.LogError("HUD Waypoint UI Prefab is not assigned in WaypointManager!");
        if (largeMapWaypointUIPrefab == null) Debug.LogError("Large Map Waypoint UI Prefab is not assigned in WaypointManager!");
        if (worldWaypointPrefab == null) Debug.LogWarning("World Waypoint Prefab is not assigned in WaypointManager. 3D Waypoints will not be created.");

        if (minimapWaypointsParent == null) Debug.LogError("Minimap Waypoints Parent is not assigned in WaypointManager!");
        if (compassWaypointsParent == null) Debug.LogError("Compass Waypoints Parent is not assigned in WaypointManager!");
        if (hudWaypointsParent == null) Debug.LogError("HUD Waypoints Parent is not assigned in WaypointManager!");
        if (largeMapWaypointsParent == null) Debug.LogError("Large Map Waypoints Parent is not assigned in WaypointManager!");

        if (playerTransform == null) Debug.LogError("Player Transform is not assigned in WaypointManager!");
        if (mainCamera == null) Debug.LogError("Main Camera is not assigned in WaypointManager!");
        if (minimapCamera == null) Debug.LogWarning("Minimap Camera is not assigned in WaypointManager. Minimap Waypoint positioning might be incorrect.");
        if (largeMapCamera == null) Debug.LogWarning("Large Map Camera is not assigned in WaypointManager. Large Map Waypoint positioning might be incorrect.");
    }

    /// <summary>
    /// Thêm một waypoint mới vào hệ thống.
    /// </summary>
    /// <param name="waypoint">Đối tượng Waypoint chứa dữ liệu.</param>
    /// <param name="isCustomWaypoint">Có phải là waypoint do người chơi tạo không (để xử lý xóa cái cũ nếu chỉ muốn 1 custom marker).</param>
    /// <param name="largeMapRawImageRectTransform">RectTransform của RawImage trên Large Map để tính toán vị trí UI chính xác hơn.</param>
    public void AddWaypoint(Waypoint waypoint, bool isCustomWaypoint, RectTransform largeMapRawImageRectTransform = null)
    {
        // Kiểm tra nếu waypoint đã tồn tại và là CustomMarker mới (để thay thế cái cũ)
        if (activeWaypointsData.ContainsKey(waypoint.id))
        {
            if (isCustomWaypoint && waypoint.waypointType == WaypointType.CustomMarker)
            {
                RemoveWaypoint(waypoint.id); // Xóa cái cũ để thay thế bằng cái mới
                Debug.Log($"[WaypointManager] Replacing existing CustomMarker with ID: {waypoint.id}");
            }
            else
            {
                Debug.LogWarning($"[WaypointManager] Waypoint with ID '{waypoint.id}' already exists and is not a custom marker. Skipping addition.");
                return;
            }
        }

        // Thêm Waypoint data vào danh sách activeWaypointsData
        activeWaypointsData[waypoint.id] = waypoint;

        // --- 1. Tạo Waypoint 3D trong thế giới ---
        if (worldWaypointPrefab != null)
        {
            // Nâng lên 0.5f để tránh bị lún vào terrain
            Vector3 worldPos = waypoint.worldPosition + Vector3.up * 0.5f;
            GameObject worldWaypointGo = Instantiate(worldWaypointPrefab, worldPos, Quaternion.identity);
            WorldWaypoint worldWaypointScript = worldWaypointGo.GetComponent<WorldWaypoint>();
            if (worldWaypointScript != null)
            {
                // CustomMarker có lifetime (ví dụ 30s), các loại khác tồn tại vĩnh viễn (0s)
                worldWaypointScript.Initialize(waypoint.id, worldPos, waypoint.waypointType == WaypointType.CustomMarker ? 30f : 0f);
            }
            worldWaypointGo.name = "WorldWaypoint_" + waypoint.name + "_" + waypoint.id;
            worldWaypointsInstances[waypoint.id] = worldWaypointGo;
            Debug.Log($"[WaypointManager] Created 3D World Waypoint for '{waypoint.name}' at: {worldPos}");
        }
        else
        {
            Debug.LogWarning("[WaypointManager] World Waypoint Prefab is not assigned. No 3D waypoint will be created for " + waypoint.name);
        }

        // --- 2. Tạo Waypoint UI cho HUD ---
        if (hudWaypointUIPrefab != null && hudWaypointsParent != null)
        {
            GameObject uiGo = Instantiate(hudWaypointUIPrefab, hudWaypointsParent);
            WaypointUI waypointUI = uiGo.GetComponent<WaypointUI>();
            if (waypointUI != null)
            {
                waypointUI.Initialize(waypoint.id, waypoint.icon, waypoint.waypointType, waypoint.worldPosition, WaypointUI.WaypointUIType.HUD);
                hudWaypointUIs[waypoint.id] = waypointUI;
                Debug.Log($"[WaypointManager] Created HUD UI Waypoint for '{waypoint.name}'.");
            }
            else
            {
                Debug.LogError($"HUD Waypoint Prefab '{hudWaypointUIPrefab.name}' does not have a WaypointUI component! Destroying instance.");
                Destroy(uiGo);
            }
        }
        else
        {
            Debug.LogWarning("HUD Waypoint Prefab or Parent is not assigned. Cannot create HUD UI Waypoint.");
        }

        // --- 3. Tạo Waypoint UI cho Minimap ---
        if (minimapWaypointUIPrefab != null && minimapWaypointsParent != null && minimapCamera != null && activeTerrain != null)
        {
            GameObject uiGo = Instantiate(minimapWaypointUIPrefab, minimapWaypointsParent);
            WaypointUI waypointUI = uiGo.GetComponent<WaypointUI>();
            if (waypointUI != null)
            {
                waypointUI.Initialize(waypoint.id, waypoint.icon, waypoint.waypointType, waypoint.worldPosition, WaypointUI.WaypointUIType.Minimap);
                minimapWaypointUIs[waypoint.id] = waypointUI;

                // Tính toán và đặt vị trí UI cho Minimap
                UpdateMapUIPosition(waypointUI, waypoint.worldPosition, minimapWaypointsParent.rect.width, minimapWaypointsParent.rect.height);

                Debug.Log($"[WaypointManager] Created Minimap UI Waypoint for '{waypoint.name}'.");
            }
            else
            {
                Debug.LogError($"Minimap Waypoint Prefab '{minimapWaypointUIPrefab.name}' does not have a WaypointUI component! Destroying instance.");
                Destroy(uiGo);
            }
        }
        else
        {
            Debug.LogWarning("Minimap Waypoint Prefab, Parent, Camera, or Terrain is not assigned. Cannot create Minimap UI Waypoint.");
        }

        // --- 4. Tạo Waypoint UI cho Compass ---
        if (compassWaypointUIPrefab != null && compassWaypointsParent != null)
        {
            GameObject uiGo = Instantiate(compassWaypointUIPrefab, compassWaypointsParent);
            WaypointUI waypointUI = uiGo.GetComponent<WaypointUI>();
            if (waypointUI != null)
            {
                // Đối với Compass, WaypointUI không tự tính toán vị trí, nó sẽ được QT_CompassBar tính toán và cập nhật.
                // Nhưng nó vẫn cần worldPosition và WaypointType để WaypointUI có thể tính khoảng cách và tự hủy nếu là QuestLocation.
                waypointUI.Initialize(waypoint.id, waypoint.icon, waypoint.waypointType, waypoint.worldPosition, WaypointUI.WaypointUIType.Compass);
                compassWaypointUIs[waypoint.id] = waypointUI;
                Debug.Log($"[WaypointManager] Created Compass UI Waypoint for '{waypoint.name}'.");
            }
            else
            {
                Debug.LogError($"Compass Waypoint Prefab '{compassWaypointUIPrefab.name}' does not have a WaypointUI component! Destroying instance.");
                Destroy(uiGo);
            }
        }
        else
        {
            Debug.LogWarning("Compass Waypoint Prefab or Parent is not assigned. Cannot create Compass UI Waypoint.");
        }

        // --- 5. Tạo Waypoint UI cho Large Map ---
        if (largeMapWaypointUIPrefab != null && largeMapWaypointsParent != null && largeMapCamera != null && activeTerrain != null && largeMapRawImageRectTransform != null)
        {
            GameObject uiGo = Instantiate(largeMapWaypointUIPrefab, largeMapWaypointsParent);
            WaypointUI waypointUI = uiGo.GetComponent<WaypointUI>();
            if (waypointUI != null)
            {
                waypointUI.Initialize(waypoint.id, waypoint.icon, waypoint.waypointType, waypoint.worldPosition, WaypointUI.WaypointUIType.LargeMap);
                largeMapWaypointUIs[waypoint.id] = waypointUI;

                UpdateMapUIPositionOnRawImage(waypointUI, waypoint.worldPosition, largeMapRawImageRectTransform);

                Debug.Log($"[WaypointManager] Created Large Map UI Waypoint for '{waypoint.name}'.");
            }
            else
            {
                Debug.LogError($"Large Map Waypoint Prefab '{largeMapWaypointUIPrefab.name}' does not have a WaypointUI component! Destroying instance.");
                Destroy(uiGo);
            }
        }
        else
        {
            Debug.LogWarning("Large Map Waypoint Prefab, Parent, Camera, RawImage, or Terrain is not assigned. Cannot create Large Map UI Waypoint.");
        }

        OnWaypointAdded?.Invoke(waypoint); // Kích hoạt sự kiện

        // NEW: Nếu đây là waypoint đầu tiên được thêm hoặc bạn muốn nó là active, hãy đặt nó.
        // Bạn có thể thêm logic ở đây để quyết định waypoint nào là active.
        // Ví dụ: SetActiveWaypoint(waypoint.id); // Hoặc logic phức tạp hơn
    }


    /// <summary>
    /// Xóa một waypoint khỏi hệ thống bằng ID.
    /// </summary>
    public void RemoveWaypoint(string waypointId)
    {
        // NEW: Kiểm tra nếu waypoint bị xóa là active waypoint
        if (_activeWaypoint != null && _activeWaypoint.id == waypointId)
        {
            _activeWaypoint = null;
            OnActiveWaypointChanged?.Invoke(null); // Thông báo rằng không có active waypoint nào
            Debug.Log($"[WaypointManager] Active Waypoint '{waypointId}' was removed, active waypoint set to null.");
        }

        // Xóa các Waypoint UI
        if (minimapWaypointUIs.ContainsKey(waypointId))
        {
            Destroy(minimapWaypointUIs[waypointId].gameObject);
            minimapWaypointUIs.Remove(waypointId);
        }
        if (compassWaypointUIs.ContainsKey(waypointId))
        {
            Destroy(compassWaypointUIs[waypointId].gameObject);
            compassWaypointUIs.Remove(waypointId);
        }
        if (hudWaypointUIs.ContainsKey(waypointId))
        {
            Destroy(hudWaypointUIs[waypointId].gameObject);
            hudWaypointUIs.Remove(waypointId);
        }
        if (largeMapWaypointUIs.ContainsKey(waypointId))
        {
            Destroy(largeMapWaypointUIs[waypointId].gameObject);
            largeMapWaypointUIs.Remove(waypointId);
        }

        // Xóa Waypoint 3D trong thế giới
        if (worldWaypointsInstances.ContainsKey(waypointId))
        {
            Destroy(worldWaypointsInstances[waypointId]);
            worldWaypointsInstances.Remove(waypointId);
            Debug.Log($"[WaypointManager] Removed World Waypoint 3D with ID: {waypointId}");
        }

        // Xóa Waypoint data khỏi danh sách activeWaypointsData
        if (activeWaypointsData.ContainsKey(waypointId))
        {
            activeWaypointsData.Remove(waypointId);
            Debug.Log($"[WaypointManager] Removed Waypoint Data with ID: {waypointId}");
        }

        OnWaypointRemoved?.Invoke(waypointId); // Kích hoạt sự kiện
    }

    // --- NEW: Phương thức để đặt waypoint đang hoạt động ---
    public void SetActiveWaypoint(string waypointId)
    {
        if (activeWaypointsData.TryGetValue(waypointId, out Waypoint newActiveWaypoint))
        {
            if (_activeWaypoint != newActiveWaypoint)
            {
                _activeWaypoint = newActiveWaypoint;
                OnActiveWaypointChanged?.Invoke(_activeWaypoint);
                Debug.Log($"[WaypointManager] Active Waypoint set to: {_activeWaypoint.name} (ID: {_activeWaypoint.id})");
            }
        }
        else
        {
            if (_activeWaypoint != null)
            {
                _activeWaypoint = null;
                OnActiveWaypointChanged?.Invoke(null); // Không có active waypoint nào
                Debug.Log("[WaypointManager] Attempted to set active waypoint with non-existent ID. Active waypoint set to null.");
            }
        }
    }

    /// <summary>
    /// Trả về waypoint đang hoạt động.
    /// </summary>
    public Waypoint GetActiveWaypoint()
    {
        return _activeWaypoint;
    }

    /// <summary>
    /// Trả về khoảng cách đến waypoint đang hoạt động.
    /// </summary>
    public float GetDistanceToActiveWaypoint()
    {
        if (_activeWaypoint != null && playerTransform != null)
        {
            return Vector3.Distance(playerTransform.position, _activeWaypoint.worldPosition);
        }
        return -1f; // Trả về -1 hoặc giá trị không hợp lệ nếu không có active waypoint hoặc playerTransform
    }


    // --- Utility Methods để tính toán vị trí UI trên map (Minimap) ---
    // Phương thức này được WaypointManager gọi khi tạo Minimap UI
    private void UpdateMapUIPosition(WaypointUI waypointUI, Vector3 worldPosition, float parentWidth, float parentHeight)
    {
        if (activeTerrain == null) return;

        // Chuyển đổi vị trí thế giới sang vị trí tương đối trên Terrain (0-1)
        float normalizedX = (worldPosition.x - terrainMinX) / terrainWidth;
        float normalizedY = (worldPosition.z - terrainMinZ) / terrainLength;

        // Chuyển đổi normalized position sang anchoredPosition trên RectTransform của parent
        // RectTransform có tâm là (0,0), nên từ normalized (0-1) ta phải chuyển về (-0.5 đến 0.5) * kích thước
        float uiX = (normalizedX - 0.5f) * parentWidth;
        float uiY = (normalizedY - 0.5f) * parentHeight;

        waypointUI.SetMapUIPosition(new Vector2(uiX, uiY), waypointUI.maxScale); // Đặt scale mặc định maxScale cho map UI
    }

    // Phương thức này dùng cho Large Map (khi Large Map là RawImage từ Render Texture)
    // Được WaypointManager gọi khi tạo Large Map UI
    private void UpdateMapUIPositionOnRawImage(WaypointUI waypointUI, Vector3 worldPosition, RectTransform largeMapRawImageRectTransform)
    {
        if (largeMapRawImageRectTransform == null || activeTerrain == null) return;

        // Tính toán vị trí của waypoint trên bản đồ dựa trên tỷ lệ của Terrain và RawImage
        float normalizedX = (worldPosition.x - terrainMinX) / terrainWidth;
        float normalizedY = (worldPosition.z - terrainMinZ) / terrainLength;

        // Chuyển đổi normalized position sang anchoredPosition trên RectTransform của RawImage
        float uiX = (normalizedX - 0.5f) * largeMapRawImageRectTransform.rect.width;
        float uiY = (normalizedY - 0.5f) * largeMapRawImageRectTransform.rect.height;

        waypointUI.SetMapUIPosition(new Vector2(uiX, uiY), waypointUI.maxScale);
        // Không cần xoay waypoint trên bản đồ lớn trừ khi nó là player icon
    }
}
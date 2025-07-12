// WaypointManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;

[DefaultExecutionOrder(-100)] // Đảm bảo WaypointManager Awake trước các script khác
public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [Header("Terrain Reference")]
    [SerializeField] public Terrain activeTerrain;

    [Header("Waypoint Data")]
    public Dictionary<string, Waypoint> activeWaypointsData = new Dictionary<string, Waypoint>();

    [Header("Waypoint UI Dictionaries")]
    public Dictionary<string, WaypointUI> minimapWaypointUIs = new Dictionary<string, WaypointUI>();
    public Dictionary<string, WaypointUI> compassWaypointUIs = new Dictionary<string, WaypointUI>();
    public Dictionary<string, WaypointUI> largeMapWaypointUIs = new Dictionary<string, WaypointUI>();

    [Header("Waypoint Prefabs")]
    [SerializeField] public GameObject minimapWaypointPrefab;
    [SerializeField] public GameObject compassWaypointPrefab;
    [SerializeField] public GameObject largeMapWaypointPrefab;

    [Header("Waypoint Parents")]
    [SerializeField] public Transform minimapWaypointsParent;
    [SerializeField] public Transform compassWaypointsParent;
    [SerializeField] public Transform largeMapWaypointsParent;

    public Waypoint activeWaypoint; // Waypoint hiện đang active
    public event Action<Waypoint> OnActiveWaypointChanged;

    [SerializeField] public Transform playerTransform;

    // Biến để theo dõi trạng thái của Large Map Panel
    private bool isLargeMapPanelActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[WaypointManager] Duplicate WaypointManager found. Destroying this duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Giữ WaypointManager giữa các scene

        Debug.Log("[WaypointManager] Awake started. Checking Inspector assignments:");

        // --- Cố gắng tải Prefab từ Resources nếu chưa được gán trong Inspector ---
        if (minimapWaypointPrefab == null)
        {
            minimapWaypointPrefab = Resources.Load<GameObject>("Waypoints/Minimap_Waypoint_UI_Prefab");
            if (minimapWaypointPrefab != null)
                Debug.Log("[WaypointManager] Loaded 'Minimap Waypoint Prefab' from Resources.");
            else
                Debug.LogError("[WaypointManager] ERROR: 'Minimap Waypoint Prefab' is NULL and could not be loaded from Resources. Please assign in Inspector or check Resources path!");
        }
        else Debug.Log($"[WaypointManager] 'Minimap Waypoint Prefab' assigned: {minimapWaypointPrefab.name}");

        if (compassWaypointPrefab == null)
        {
            compassWaypointPrefab = Resources.Load<GameObject>("Waypoints/Compass_Waypoint_UI_Prefab");
            if (compassWaypointPrefab != null)
                Debug.Log("[WaypointManager] Loaded 'Compass Waypoint Prefab' from Resources.");
            else
                Debug.LogError("[WaypointManager] ERROR: 'Compass Waypoint Prefab' is NULL and could not be loaded from Resources. Please assign in Inspector or check Resources path!");
        }
        else Debug.Log($"[WaypointManager] 'Compass Waypoint Prefab' assigned: {compassWaypointPrefab.name}");

        if (largeMapWaypointPrefab == null)
        {
            largeMapWaypointPrefab = Resources.Load<GameObject>("Waypoints/LargeMap_Waypoint_UI_Prefab");
            if (largeMapWaypointPrefab != null)
                Debug.Log("[WaypointManager] Loaded 'Large Map Waypoint Prefab' from Resources.");
            else
                Debug.LogError("[WaypointManager] ERROR: 'Large Map Waypoint Prefab' is NULL and could not be loaded from Resources. Please assign in Inspector or check Resources path!");
        }
        else Debug.Log($"[WaypointManager] 'Large Map Waypoint Prefab' assigned: {largeMapWaypointPrefab.name}");

        // --- Kiểm tra và cố gắng tìm các Parent từ Scene nếu chưa được gán trong Inspector ---
        if (minimapWaypointsParent == null)
        {
            var obj = GameObject.Find("SmallMinimapUIContainer");
            if (obj != null) minimapWaypointsParent = obj.transform;
            if (minimapWaypointsParent != null)
                Debug.Log($"[WaypointManager] Found 'Minimap Waypoints Parent' by name: {minimapWaypointsParent.name}. Parent active in hierarchy: {minimapWaypointsParent.gameObject.activeInHierarchy}. Path: {minimapWaypointsParent.GetHierarchyPath()}");
            else
                Debug.LogError("[WaypointManager] ERROR: 'Minimap Waypoints Parent' is NULL and could not be found by name. Please assign in Inspector!");
        }
        else Debug.Log($"[WaypointManager] 'Minimap Waypoints Parent' assigned: {minimapWaypointsParent.name}. Parent active in hierarchy: {minimapWaypointsParent.gameObject.activeInHierarchy}. Path: {minimapWaypointsParent.GetHierarchyPath()}");

        if (compassWaypointsParent == null)
        {
            var obj = GameObject.Find("Compass Bar");
            if (obj != null) compassWaypointsParent = obj.transform;
            if (compassWaypointsParent != null)
                Debug.Log($"[WaypointManager] Found 'Compass Waypoints Parent' by name: {compassWaypointsParent.name}. Parent active in hierarchy: {compassWaypointsParent.gameObject.activeInHierarchy}. Path: {compassWaypointsParent.GetHierarchyPath()}");
            else
                Debug.LogError("[WaypointManager] ERROR: 'Compass Waypoints Parent' is NULL and could not be found by name. Please assign in Inspector!");
        }
        else Debug.Log($"[WaypointManager] 'Compass Waypoints Parent' assigned: {compassWaypointsParent.name}. Parent active in hierarchy: {compassWaypointsParent.gameObject.activeInHierarchy}. Path: {compassWaypointsParent.GetHierarchyPath()}");

        // --- ĐẶC BIỆT: Logic tìm kiếm cho LargeMap_Panel (xử lý việc bị tắt) ---
        if (largeMapWaypointsParent == null)
        {
            Transform minimapUIParent = null;
            GameObject minimapUIObject = GameObject.Find("minimapUI"); // Tìm đối tượng "minimapUI" đang active

            if (minimapUIObject != null)
            {
                minimapUIParent = minimapUIObject.transform;
                Debug.Log($"[WaypointManager] Found active parent 'minimapUI' for LargeMap_Panel. Path: {minimapUIObject.transform.GetHierarchyPath()}. Active in hierarchy: {minimapUIObject.activeInHierarchy}");

                largeMapWaypointsParent = minimapUIParent.Find("LargeMap_Panel");

                if (largeMapWaypointsParent != null)
                {
                    Debug.Log($"[WaypointManager] Successfully found 'Large Map Waypoints Parent' ('LargeMap_Panel') as a child of 'minimapUI'. Active in hierarchy: {largeMapWaypointsParent.gameObject.activeInHierarchy}. Path: {largeMapWaypointsParent.GetHierarchyPath()}");
                }
                else
                {
                    Debug.LogError("[WaypointManager] ERROR: 'Large Map Waypoints Parent' is NULL. Could not find 'LargeMap_Panel' as a child of 'minimapUI'. Check name or hierarchy under 'minimapUI'!");
                }
            }
            else
            {
                Debug.LogError("[WaypointManager] ERROR: 'Large Map Waypoints Parent' is NULL. 'minimapUI' was not found or not active. Please ensure 'minimapUI' is active in Hierarchy or assign 'LargeMap_Panel' directly in Inspector!");
            }
        }
        else
        {
            Debug.Log($"[WaypointManager] 'Large Map Waypoints Parent' assigned from Inspector: {largeMapWaypointsParent.name}. Parent active in hierarchy: {largeMapWaypointsParent.gameObject.activeInHierarchy}. Path: {largeMapWaypointsParent.GetHierarchyPath()}");
        }

        // Kiểm tra các ràng buộc khác (Player Transform, Active Terrain)
        if (playerTransform == null)
        {
            // Try to find the player by tag if not assigned
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                Debug.Log($"[WaypointManager] Found 'Player Transform' by tag 'Player': {playerTransform.name}");
            }
            else
            {
                Debug.LogWarning("[WaypointManager] WARNING: 'Player Transform' is NULL and could not be found by tag 'Player'. Distance and direction features may not function. Please assign in Inspector or tag your player GameObject.");
            }
        }
        else Debug.Log($"[WaypointManager] 'Player Transform' assigned: {playerTransform.name}");

        if (activeTerrain == null)
        {
            activeTerrain = Terrain.activeTerrain;
            if (activeTerrain != null)
                Debug.Log("[WaypointManager] Found 'Active Terrain' via Terrain.activeTerrain.");
            else
                Debug.LogWarning("[WaypointManager] WARNING: 'Active Terrain' is NULL and could not be found automatically. Terrain-related features may not function.");
        }
        else Debug.Log($"[WaypointManager] 'Active Terrain' assigned: {activeTerrain.name}");

        Debug.Log("[WaypointManager] Awake finished.");
    }

    // Public method để các UI Controller khác (ví dụ: LargeMapController) gọi khi bật/tắt Large Map Panel
    public void SetLargeMapPanelActive(bool isActive)
    {
        isLargeMapPanelActive = isActive;
        Debug.Log($"[WaypointManager] Large Map Panel state changed to: {isActive}. Updating large map waypoints visibility.");

        foreach (var kvp in largeMapWaypointUIs)
        {
            if (kvp.Value != null && kvp.Value.gameObject != null)
            {
                // Only set active if the corresponding waypoint is the active one, or if all are to be shown on large map.
                // For now, if the panel is active, we show all large map waypoints.
                // If you only want the active waypoint to show on the large map, you'll need additional logic here.
                bool shouldBeActive = isActive;
                // If you want ONLY the active waypoint to be shown on large map:
                // bool shouldBeActive = isActive && (activeWaypoint != null && kvp.Key == activeWaypoint.id);
                kvp.Value.gameObject.SetActive(shouldBeActive);
            }
        }
    }


    public void AddWaypoint(Waypoint waypoint, bool setActive = false)
    {
        if (waypoint == null || string.IsNullOrEmpty(waypoint.id))
        {
            Debug.LogWarning("[WaypointManager] Waypoint null hoặc không có ID. Aborting AddWaypoint.");
            return;
        }

        if (activeWaypointsData.ContainsKey(waypoint.id))
        {
            // Waypoint đã tồn tại, cập nhật dữ liệu và UI
            Debug.Log($"[WaypointManager] Updating existing waypoint: {waypoint.id} at {waypoint.worldPosition}");
            activeWaypointsData[waypoint.id] = waypoint;

            // Cập nhật UI của waypoint hiện có
            if (minimapWaypointUIs.TryGetValue(waypoint.id, out WaypointUI miniUI))
            {
                if (miniUI == null || miniUI.gameObject == null)
                {
                    Debug.LogError($"[WaypointManager] ERROR: Minimap UI for {waypoint.id} is null or its GameObject is destroyed during update! Attempting to re-instantiate.");
                    InstantiateWaypointUI(waypoint, WaypointUIType.Minimap);
                }
                else
                {
                    miniUI.SetData(waypoint);
                    Debug.Log($"[WaypointManager] Updated existing Minimap UI for '{waypoint.id}'. Is active: {miniUI.gameObject.activeSelf}");
                }
            }
            if (compassWaypointUIs.TryGetValue(waypoint.id, out WaypointUI compUI))
            {
                if (compUI == null || compUI.gameObject == null)
                {
                    Debug.LogError($"[WaypointManager] ERROR: Compass UI for {waypoint.id} is null or its GameObject is destroyed during update! Attempting to re-instantiate.");
                    InstantiateWaypointUI(waypoint, WaypointUIType.Compass);
                }
                else
                {
                    compUI.SetData(waypoint);
                    Debug.Log($"[WaypointManager] Updated existing Compass UI for '{waypoint.id}'. Is active: {compUI.gameObject.activeSelf}");
                }
            }
            if (largeMapWaypointUIs.TryGetValue(waypoint.id, out WaypointUI largeUI))
            {
                if (largeUI == null || largeUI.gameObject == null)
                {
                    Debug.LogError($"[WaypointManager] ERROR: LargeMap UI for {waypoint.id} is null or its GameObject is destroyed during update! Attempting to re-instantiate.");
                    InstantiateWaypointUI(waypoint, WaypointUIType.LargeMap);
                }
                else
                {
                    largeUI.SetData(waypoint);
                    Debug.Log($"[WaypointManager] Updated existing Large Map UI for '{waypoint.id}'. Is active: {largeUI.gameObject.activeSelf}");
                }
            }
        }
        else
        {
            // Thêm waypoint mới
            activeWaypointsData.Add(waypoint.id, waypoint);
            Debug.Log($"[WaypointManager] Adding NEW waypoint: {waypoint.id} at {waypoint.worldPosition}");

            InstantiateWaypointUI(waypoint, WaypointUIType.Minimap);
            InstantiateWaypointUI(waypoint, WaypointUIType.Compass);
            InstantiateWaypointUI(waypoint, WaypointUIType.LargeMap);
        }

        if (setActive)
        {
            SetActiveWaypoint(waypoint.id); // Gọi SetActiveWaypoint để bật UI
        }
        else
        {
            // Nếu không setActive, đảm bảo UI của waypoint đó được ẩn (trên minimap/compass)
            // Large Map UI sẽ được quản lý bởi SetLargeMapPanelActive
            minimapWaypointUIs.TryGetValue(waypoint.id, out var miniUI);
            if (miniUI != null) miniUI.gameObject.SetActive(false);

            compassWaypointUIs.TryGetValue(waypoint.id, out var compUI);
            if (compUI != null) compUI.gameObject.SetActive(false);

            // Đối với LargeMap, chúng ta chỉ ẩn nó nếu LargeMapPanel hiện đang không active
            largeMapWaypointUIs.TryGetValue(waypoint.id, out var largeUI);
            if (largeUI != null) largeUI.gameObject.SetActive(isLargeMapPanelActive);

            Debug.Log($"[WaypointManager] Waypoint '{waypoint.id}' added/updated but not set as active (Minimap/Compass UI hidden, Large Map visibility depends on panel state).");
        }
    }

    private enum WaypointUIType { Minimap, Compass, LargeMap }

    private void InstantiateWaypointUI(Waypoint waypoint, WaypointUIType uiType)
    {
        GameObject prefab = null;
        Transform parent = null;
        Dictionary<string, WaypointUI> targetDict = null;
        string typeName = "";
        bool setActiveInitially = false;

        switch (uiType)
        {
            case WaypointUIType.Minimap:
                prefab = minimapWaypointPrefab;
                parent = minimapWaypointsParent;
                targetDict = minimapWaypointUIs;
                typeName = "Minimap";
                setActiveInitially = false; // Mặc định là false, sẽ được bật bởi SetActiveWaypoint
                break;
            case WaypointUIType.Compass:
                prefab = compassWaypointPrefab;
                parent = compassWaypointsParent;
                targetDict = compassWaypointUIs;
                typeName = "Compass";
                setActiveInitially = false; // Mặc định là false, sẽ được bật bởi SetActiveWaypoint
                break;
            case WaypointUIType.LargeMap:
                prefab = largeMapWaypointPrefab;
                parent = largeMapWaypointsParent;
                targetDict = largeMapWaypointUIs;
                typeName = "Large Map";
                setActiveInitially = isLargeMapPanelActive; // Visibility depends on panel state
                break;
        }

        if (prefab != null && parent != null)
        {
            // Kiểm tra Parent trước khi Instantiate
            if (!parent.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[WaypointManager] Parent for {typeName} UI ({parent.name}) is NOT active in hierarchy for waypoint '{waypoint.id}'. Waypoint UI might not be visible.");
            }

            GameObject uiObj = Instantiate(prefab, parent);
            Debug.Log($"[WaypointManager] Instantiated {typeName} Prefab for '{waypoint.id}'. Object name: {uiObj.name}. Parent: {parent.name}. Parent active: {parent.gameObject.activeInHierarchy}. Path: {parent.GetHierarchyPath()}");
            WaypointUI waypointUI = uiObj.GetComponent<WaypointUI>();
            if (waypointUI != null)
            {
                waypointUI.SetData(waypoint);
                targetDict[waypoint.id] = waypointUI;

                // Đặt trạng thái active ban đầu
                waypointUI.gameObject.SetActive(setActiveInitially);
                Debug.Log($"[WaypointManager] Successfully got WaypointUI for {typeName}: {waypointUI.name}. It should be under '{parent.name}' in Hierarchy. Initially active: {setActiveInitially}. Dictionary size for {typeName}: {targetDict.Count}");
            }
            else
            {
                Debug.LogError($"[WaypointManager] {typeName} prefab '{prefab.name}' is missing WaypointUI component! Destroying instantiated object: {uiObj.name}.");
                Destroy(uiObj);
            }
        }
        else
        {
            Debug.LogWarning($"[WaypointManager] Cannot instantiate {typeName} UI for '{waypoint.id}'. Prefab ({prefab?.name ?? "NULL"}) or Parent ({parent?.name ?? "NULL"}) is NULL. Please check assignments in Inspector.");
        }
    }

    public void RemoveWaypoint(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[WaypointManager] Cannot remove waypoint: Provided ID is null or empty.");
            return;
        }

        if (!activeWaypointsData.Remove(id))
        {
            Debug.LogWarning($"[WaypointManager] Attempted to remove waypoint with ID '{id}', but it was not found in data.");
        }
        else
        {
            Debug.Log($"[WaypointManager] Waypoint data for '{id}' removed.");
        }

        // Đảm bảo Destroy GameObject UI
        if (minimapWaypointUIs.TryGetValue(id, out var miniUI))
        {
            if (miniUI != null && miniUI.gameObject != null) Destroy(miniUI.gameObject);
            minimapWaypointUIs.Remove(id);
            Debug.Log($"[WaypointManager] Minimap UI for '{id}' destroyed.");
        }

        if (compassWaypointUIs.TryGetValue(id, out var compUI))
        {
            if (compUI != null && compUI.gameObject != null) Destroy(compUI.gameObject);
            compassWaypointUIs.Remove(id);
            Debug.Log($"[WaypointManager] Compass UI for '{id}' destroyed.");
        }

        if (largeMapWaypointUIs.TryGetValue(id, out var largeUI))
        {
            if (largeUI != null && largeUI.gameObject != null) Destroy(largeUI.gameObject);
            largeMapWaypointUIs.Remove(id);
            Debug.Log($"[WaypointManager] Large Map UI for '{id}' destroyed.");
        }

        if (activeWaypoint != null && activeWaypoint.id == id)
        {
            activeWaypoint = null;
            OnActiveWaypointChanged?.Invoke(null);
            Debug.Log($"[WaypointManager] Active waypoint '{id}' was removed. No waypoint is currently active.");
        }
    }

    public void SetActiveWaypoint(string id)
    {
        Debug.Log($"[WaypointManager] SetActiveWaypoint called for ID: '{id}'. Current activeWaypoint: {(activeWaypoint != null ? activeWaypoint.id : "None")}");

        Waypoint newActive = null;
        if (activeWaypointsData.TryGetValue(id, out newActive))
        {
            if (activeWaypoint == newActive) // Kiểm tra nếu đã là active
            {
                Debug.Log($"[WaypointManager] Waypoint '{id}' is already the active waypoint. No change needed.");
                return;
            }

            Debug.Log($"[WaypointManager] Attempting to set active waypoint to: '{id}'.");

            // Ẩn tất cả các UI waypoint HIỆN TẠI đang active trên minimap và compass
            // (vì chỉ muốn 1 cái duy nhất active trên minimap/compass)
            foreach (var kvp in activeWaypointsData)
            {
                // Ensure all minimap/compass UIs are off, except for the one being set active
                minimapWaypointUIs.TryGetValue(kvp.Key, out var miniUI);
                if (miniUI != null) miniUI.gameObject.SetActive(false); // Always set to false initially

                compassWaypointUIs.TryGetValue(kvp.Key, out var compUI);
                if (compUI != null) compUI.gameObject.SetActive(false); // Always set to false initially

                // Large map visibility is controlled by isLargeMapPanelActive
                largeMapWaypointUIs.TryGetValue(kvp.Key, out var largeUI);
                if (largeUI != null) largeUI.gameObject.SetActive(isLargeMapPanelActive); // All large map waypoints active if panel is active
            }


            // Chỉ bật UI của active waypoint mới được chọn
            if (minimapWaypointUIs.TryGetValue(id, out var newMiniUI))
            {
                if (newMiniUI != null)
                {
                    newMiniUI.gameObject.SetActive(true);
                    Debug.Log($"[WaypointManager] Minimap UI for '{id}' found and set to ACTIVE. Current object active: {newMiniUI.gameObject.activeSelf}. Parent active: {newMiniUI.transform.parent.gameObject.activeInHierarchy}.");
                }
                else Debug.LogWarning($"[WaypointManager] Minimap UI for '{id}' is NULL in dictionary when trying to activate.");
            }
            else Debug.LogWarning($"[WaypointManager] Minimap UI for '{id}' not found in dictionary when trying to activate.");


            if (compassWaypointUIs.TryGetValue(id, out var newCompUI))
            {
                if (newCompUI != null)
                {
                    newCompUI.gameObject.SetActive(true);
                    Debug.Log($"[WaypointManager] Compass UI for '{id}' found and set to ACTIVE. Current object active: {newCompUI.gameObject.activeSelf}. Parent active: {newCompUI.transform.parent.gameObject.activeInHierarchy}.");
                }
                else Debug.LogWarning($"[WaypointManager] Compass UI for '{id}' is NULL in dictionary when trying to activate.");
            }
            else Debug.LogWarning($"[WaypointManager] Compass UI for '{id}' not found in dictionary when trying to activate.");

            // For Large Map Waypoint UI, it should be enabled if the Large Map Panel is active, regardless of whether it's the "active" waypoint for guidance.
            // If the design intent is that ONLY the active waypoint shows on the large map, then uncomment the logic below
            // and adjust SetLargeMapPanelActive accordingly.
            if (largeMapWaypointUIs.TryGetValue(id, out var newLargeUI))
            {
                if (newLargeUI != null)
                {
                    // If large map panel is active, activate this UI. Otherwise, it will be off.
                    newLargeUI.gameObject.SetActive(isLargeMapPanelActive);
                    Debug.Log($"[WaypointManager] Large Map UI for '{id}' set to {(isLargeMapPanelActive ? "ACTIVE" : "INACTIVE")} based on Large Map Panel state. Current object active: {newLargeUI.gameObject.activeSelf}. Parent active: {newLargeUI.transform.parent.gameObject.activeInHierarchy}.");
                }
                else Debug.LogWarning($"[WaypointManager] Large Map UI for '{id}' is NULL in dictionary when trying to activate.");
            }
            else Debug.LogWarning($"[WaypointManager] Large Map UI for '{id}' not found in dictionary when trying to activate.");


            activeWaypoint = newActive;
            OnActiveWaypointChanged?.Invoke(activeWaypoint);
            Debug.Log($"[WaypointManager] Active waypoint successfully set to: '{activeWaypoint.id}'. Event fired.");
        }
        else
        {
            Debug.LogWarning($"[WaypointManager] Attempted to set active waypoint with ID '{id}', but it was not found in activeWaypointsData. Clearing current active waypoint.");
            if (activeWaypoint != null)
            {
                Waypoint oldActive = activeWaypoint;
                activeWaypoint = null;
                OnActiveWaypointChanged?.Invoke(null); // Notify that active waypoint is now null
                Debug.Log($"[WaypointManager] Active waypoint cleared (was '{oldActive.id}') as the requested ID '{id}' was not found.");
            }
        }
    }

    public Waypoint GetActiveWaypoint() => activeWaypoint;

    public float GetDistanceToActiveWaypoint()
    {
        if (activeWaypoint == null || playerTransform == null) return -1;
        return Vector3.Distance(playerTransform.position, activeWaypoint.worldPosition);
    }

    public Transform GetPlayerTransform() => playerTransform;
}

public static class TransformExtensions
{
    public static string GetHierarchyPath(this Transform current)
    {
        if (current == null) return "";
        if (current.parent == null) return "/" + current.name;
        return current.parent.GetHierarchyPath() + "/" + current.name;
    }
}
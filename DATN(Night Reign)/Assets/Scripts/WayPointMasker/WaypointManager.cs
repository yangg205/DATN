using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[DefaultExecutionOrder(-100)] // Đảm bảo WaypointManager Awake trước các script khác
public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [Header("Terrain References")]
    [SerializeField] public List<Terrain> terrains;

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
    [Tooltip("Đây phải là RectTransform của RawImage hiển thị bản đồ lớn (LargeMap_Display_Raw)")]
    [SerializeField] public Transform largeMapWaypointsParent;

    public Waypoint activeWaypoint;
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
        DontDestroyOnLoad(gameObject);

        Debug.Log("[WaypointManager] Awake started. Checking Inspector assignments:");

        LoadPrefab(ref minimapWaypointPrefab, "Waypoints/Minimap_Waypoint_UI_Prefab", "Minimap Waypoint Prefab");
        LoadPrefab(ref compassWaypointPrefab, "Waypoints/Compass_Waypoint_UI_Prefab", "Compass Waypoint Prefab");
        LoadPrefab(ref largeMapWaypointPrefab, "Waypoints/LargeMap_Waypoint_UI_Prefab", "Large Map Waypoint Prefab");

        // --- Cố gắng tìm các Parent UI nếu chưa được gán trong Inspector ---

        // Parent cho Minimap: SmallMinimapUIContainer (con của minimapUI)
        if (minimapWaypointsParent == null)
        {
            Transform minimapUIRoot = GameObject.Find("minimapUI")?.transform;
            if (minimapUIRoot != null)
            {
                minimapWaypointsParent = minimapUIRoot.Find("SmallMinimapUIContainer");
                if (minimapWaypointsParent != null)
                {
                    Debug.Log($"[WaypointManager] Found 'Minimap Waypoints Parent' ('SmallMinimapUIContainer') under 'minimapUI'. Path: {minimapWaypointsParent.GetHierarchyPath()}");
                }
                else
                {
                    Debug.LogError("[WaypointManager] ERROR: 'SmallMinimapUIContainer' not found under 'minimapUI'. Please check hierarchy or assign 'Minimap Waypoints Parent' in Inspector!");
                }
            }
            else
            {
                Debug.LogError("[WaypointManager] ERROR: 'minimapUI' (root for minimap) not found. Please check hierarchy or assign 'Minimap Waypoints Parent' in Inspector!");
            }
        }
        else Debug.Log($"[WaypointManager] 'Minimap Waypoints Parent' assigned: {minimapWaypointsParent.name}. Path: {minimapWaypointsParent.GetHierarchyPath()}");


        // Parent cho Compass: Vẫn giữ "Compass Bar" nếu bạn có
        FindParentIfNull(ref compassWaypointsParent, "Compass Bar", "Compass Waypoints Parent");

        // Parent cho Large Map: LargeMap_Display_Raw (con của LargeMap_Panel, mà LargeMap_Panel là con của minimapUI)
        if (largeMapWaypointsParent == null)
        {
            Transform minimapUIRoot = GameObject.Find("minimapUI")?.transform; // Lần này LargeMap_Panel cũng nằm dưới minimapUI
            if (minimapUIRoot != null)
            {
                Transform largeMapPanel = minimapUIRoot.Find("LargeMap_Panel");
                if (largeMapPanel != null)
                {
                    largeMapWaypointsParent = largeMapPanel.Find("LargeMap_Display_Raw");
                    if (largeMapWaypointsParent != null)
                    {
                        Debug.Log($"[WaypointManager] Found 'Large Map Waypoints Parent' ('LargeMap_Display_Raw') under 'minimapUI/LargeMap_Panel'. Path: {largeMapWaypointsParent.GetHierarchyPath()}");
                    }
                    else
                    {
                        Debug.LogError("[WaypointManager] ERROR: 'LargeMap_Display_Raw' not found under 'minimapUI/LargeMap_Panel'. Please check hierarchy or assign 'Large Map Waypoints Parent' in Inspector!");
                    }
                }
                else
                {
                    Debug.LogError("[WaypointManager] ERROR: 'LargeMap_Panel' not found under 'minimapUI'. Please check hierarchy or assign 'Large Map Waypoints Parent' in Inspector!");
                }
            }
            else
            {
                Debug.LogError("[WaypointManager] ERROR: 'minimapUI' (root for both minimap and large map) not found. Please ensure it's in your scene and active, or assign 'Large Map Waypoints Parent' in Inspector!");
            }
        }
        else
        {
            Debug.Log($"[WaypointManager] 'Large Map Waypoints Parent' assigned from Inspector: {largeMapWaypointsParent.name}. Path: {largeMapWaypointsParent.GetHierarchyPath()}");
        }

        // Kiểm tra Player Transform
        if (playerTransform == null)
        {
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

        // Kiểm tra Terrains
        if (terrains == null || terrains.Count == 0 || terrains.Any(t => t == null))
        {
            terrains = Terrain.FindObjectsOfType<Terrain>().ToList();
            if (terrains != null && terrains.Count > 0)
            {
                Debug.Log($"[WaypointManager] Found {terrains.Count} Terrains in the scene automatically.");
            }
            else
            {
                Debug.LogWarning("[WaypointManager] WARNING: No Terrains found in the scene automatically. Terrain-related features may not function.");
            }
        }
        else
        {
            Debug.Log($"[WaypointManager] {terrains.Count} Terrains assigned in Inspector.");
        }

        Debug.Log("[WaypointManager] Awake finished.");
    }

    private void LoadPrefab(ref GameObject prefabField, string resourcePath, string name)
    {
        if (prefabField == null)
        {
            prefabField = Resources.Load<GameObject>(resourcePath);
            if (prefabField != null)
                Debug.Log($"[WaypointManager] Loaded '{name}' from Resources: {resourcePath}");
            else
                Debug.LogError($"[WaypointManager] ERROR: '{name}' is NULL and could not be loaded from Resources. Please assign in Inspector or check Resources path! Path: {resourcePath}");
        }
        else Debug.Log($"[WaypointManager] '{name}' assigned: {prefabField.name}");
    }

    private void FindParentIfNull(ref Transform parentField, string objectName, string debugName)
    {
        if (parentField == null)
        {
            var obj = GameObject.Find(objectName);
            if (obj != null) parentField = obj.transform;
            if (parentField != null)
                Debug.Log($"[WaypointManager] Found '{debugName}' by name: {parentField.name}. Parent active in hierarchy: {parentField.gameObject.activeInHierarchy}. Path: {parentField.GetHierarchyPath()}");
            else
                Debug.LogError($"[WaypointManager] ERROR: '{debugName}' is NULL and could not be found by name ('{objectName}'). Please assign in Inspector!");
        }
        else Debug.Log($"[WaypointManager] '{debugName}' assigned: {parentField.name}. Parent active in hierarchy: {parentField.gameObject.activeInHierarchy}. Path: {parentField.GetHierarchyPath()}");
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
                kvp.Value.gameObject.SetActive(isActive);
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
            Debug.Log($"[WaypointManager] Updating existing waypoint: {waypoint.id} at {waypoint.worldPosition}");
            activeWaypointsData[waypoint.id] = waypoint;

            UpdateWaypointUI(waypoint, WaypointUIType.Minimap, minimapWaypointUIs, minimapWaypointPrefab, minimapWaypointsParent);
            UpdateWaypointUI(waypoint, WaypointUIType.Compass, compassWaypointUIs, compassWaypointPrefab, compassWaypointsParent);
            UpdateWaypointUI(waypoint, WaypointUIType.LargeMap, largeMapWaypointUIs, largeMapWaypointPrefab, largeMapWaypointsParent);
        }
        else
        {
            activeWaypointsData.Add(waypoint.id, waypoint);
            Debug.Log($"[WaypointManager] Adding NEW waypoint: {waypoint.id} at {waypoint.worldPosition}");

            InstantiateWaypointUI(waypoint, WaypointUIType.Minimap);
            InstantiateWaypointUI(waypoint, WaypointUIType.Compass);
            InstantiateWaypointUI(waypoint, WaypointUIType.LargeMap);
        }

        if (setActive)
        {
            SetActiveWaypoint(waypoint.id);
        }
        else
        {
            minimapWaypointUIs.TryGetValue(waypoint.id, out var miniUI);
            if (miniUI != null) miniUI.gameObject.SetActive(false);

            compassWaypointUIs.TryGetValue(waypoint.id, out var compUI);
            if (compUI != null) compUI.gameObject.SetActive(false);

            largeMapWaypointUIs.TryGetValue(waypoint.id, out var largeUI);
            if (largeUI != null) largeUI.gameObject.SetActive(isLargeMapPanelActive);

            Debug.Log($"[WaypointManager] Waypoint '{waypoint.id}' added/updated but not set as active (Minimap/Compass UI hidden, Large Map visibility depends on panel state).");
        }
    }

    private void UpdateWaypointUI(Waypoint waypoint, WaypointUIType uiType, Dictionary<string, WaypointUI> dict, GameObject prefab, Transform parent)
    {
        if (dict.TryGetValue(waypoint.id, out WaypointUI ui))
        {
            if (ui == null || ui.gameObject == null)
            {
                Debug.LogError($"[WaypointManager] ERROR: {uiType} UI for {waypoint.id} is null or its GameObject is destroyed during update! Attempting to re-instantiate.");
                InstantiateWaypointUI(waypoint, uiType);
            }
            else
            {
                ui.SetData(waypoint);
                Debug.Log($"[WaypointManager] Updated existing {uiType} UI for '{waypoint.id}'. Is active: {ui.gameObject.activeSelf}");
            }
        }
        else
        {
            Debug.LogWarning($"[WaypointManager] {uiType} UI for '{waypoint.id}' not found in dictionary during update, instantiating new one.");
            InstantiateWaypointUI(waypoint, uiType);
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
                setActiveInitially = false;
                break;
            case WaypointUIType.Compass:
                prefab = compassWaypointPrefab;
                parent = compassWaypointsParent;
                targetDict = compassWaypointUIs;
                typeName = "Compass";
                setActiveInitially = false;
                break;
            case WaypointUIType.LargeMap:
                prefab = largeMapWaypointPrefab;
                parent = largeMapWaypointsParent;
                targetDict = largeMapWaypointUIs;
                typeName = "Large Map";
                setActiveInitially = isLargeMapPanelActive;
                break;
        }

        if (prefab != null && parent != null)
        {
            if (!parent.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[WaypointManager] Parent for {typeName} UI ({parent.name}) is NOT active in hierarchy for waypoint '{waypoint.id}'. Waypoint UI might not be visible.");
            }

            if (targetDict.ContainsKey(waypoint.id) && targetDict[waypoint.id] != null && targetDict[waypoint.id].gameObject != null)
            {
                Debug.LogWarning($"[WaypointManager] {typeName} UI for '{waypoint.id}' already exists. Skipping re-instantiation.");
                return;
            }

            GameObject uiObj = Instantiate(prefab, parent);
            uiObj.name = $"{waypoint.id}_{typeName}_UI";
            Debug.Log($"[WaypointManager] Instantiated {typeName} Prefab for '{waypoint.id}'. Object name: {uiObj.name}. Parent: {parent.name}. Parent active: {parent.gameObject.activeInHierarchy}. Path: {parent.GetHierarchyPath()}");
            WaypointUI waypointUI = uiObj.GetComponent<WaypointUI>();
            if (waypointUI != null)
            {
                waypointUI.SetData(waypoint);
                targetDict[waypoint.id] = waypointUI;

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

        RemoveAndDestroyUI(id, minimapWaypointUIs, "Minimap");
        RemoveAndDestroyUI(id, compassWaypointUIs, "Compass");
        RemoveAndDestroyUI(id, largeMapWaypointUIs, "Large Map");

        if (activeWaypoint != null && activeWaypoint.id == id)
        {
            activeWaypoint = null;
            OnActiveWaypointChanged?.Invoke(null);
            Debug.Log($"[WaypointManager] Active waypoint '{id}' was removed. No waypoint is currently active.");
        }
    }

    private void RemoveAndDestroyUI(string id, Dictionary<string, WaypointUI> dict, string typeName)
    {
        if (dict.TryGetValue(id, out var ui))
        {
            if (ui != null && ui.gameObject != null)
            {
                Destroy(ui.gameObject);
                Debug.Log($"[WaypointManager] {typeName} UI for '{id}' destroyed.");
            }
            else
            {
                Debug.LogWarning($"[WaypointManager] {typeName} UI for '{id}' was found in dictionary but its GameObject was already null/destroyed.");
            }
            dict.Remove(id);
        }
    }

    public void SetActiveWaypoint(string id)
    {
        Debug.Log($"[WaypointManager] SetActiveWaypoint called for ID: '{id}'. Current activeWaypoint: {(activeWaypoint != null ? activeWaypoint.id : "None")}");

        Waypoint newActive = null;
        if (activeWaypointsData.TryGetValue(id, out newActive))
        {
            if (activeWaypoint == newActive)
            {
                Debug.Log($"[WaypointManager] Waypoint '{id}' is already the active waypoint. No change needed.");
                return;
            }

            Debug.Log($"[WaypointManager] Attempting to set active waypoint to: '{id}'.");

            foreach (var kvp in activeWaypointsData)
            {
                minimapWaypointUIs.TryGetValue(kvp.Key, out var miniUI);
                if (miniUI != null) miniUI.gameObject.SetActive(false);

                compassWaypointUIs.TryGetValue(kvp.Key, out var compUI);
                if (compUI != null) compUI.gameObject.SetActive(false);

                largeMapWaypointUIs.TryGetValue(kvp.Key, out var largeUI);
                if (largeUI != null) largeUI.gameObject.SetActive(isLargeMapPanelActive);
            }

            if (minimapWaypointUIs.TryGetValue(id, out var newMiniUI))
            {
                if (newMiniUI != null)
                {
                    newMiniUI.gameObject.SetActive(true);
                    Debug.Log($"[WaypointManager] Minimap UI for '{id}' found and set to ACTIVE. Current object active: {newMiniUI.gameObject.activeSelf}. Parent active: {newMiniUI.transform.parent?.gameObject.activeInHierarchy ?? false}.");
                }
                else Debug.LogWarning($"[WaypointManager] Minimap UI for '{id}' is NULL in dictionary when trying to activate.");
            }
            else Debug.LogWarning($"[WaypointManager] Minimap UI for '{id}' not found in dictionary when trying to activate.");


            if (compassWaypointUIs.TryGetValue(id, out var newCompUI))
            {
                if (newCompUI != null)
                {
                    newCompUI.gameObject.SetActive(true);
                    Debug.Log($"[WaypointManager] Compass UI for '{id}' found and set to ACTIVE. Current object active: {newCompUI.gameObject.activeSelf}. Parent active: {newCompUI.transform.parent?.gameObject.activeInHierarchy ?? false}.");
                }
                else Debug.LogWarning($"[WaypointManager] Compass UI for '{id}' is NULL in dictionary when trying to activate.");
            }
            else Debug.LogWarning($"[WaypointManager] Compass UI for '{id}' not found in dictionary when trying to activate.");

            if (largeMapWaypointUIs.TryGetValue(id, out var newLargeUI))
            {
                if (newLargeUI != null)
                {
                    newLargeUI.gameObject.SetActive(isLargeMapPanelActive);
                    Debug.Log($"[WaypointManager] Large Map UI for '{id}' set to {(isLargeMapPanelActive ? "ACTIVE" : "INACTIVE")} based on Large Map Panel state. Current object active: {newLargeUI.gameObject.activeSelf}. Parent active: {newLargeUI.transform.parent?.gameObject.activeInHierarchy ?? false}.");
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
                OnActiveWaypointChanged?.Invoke(null);
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
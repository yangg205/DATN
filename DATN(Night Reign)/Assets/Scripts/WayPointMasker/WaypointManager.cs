using UnityEngine;
using System.Collections.Generic;
using System; // For Guid

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [Header("Waypoint Prefabs")]
    public WaypointUI minimapWaypointPrefab; // Prefab UI cho minimap (có Image và TextMeshPro)
    public WaypointUI compassWaypointPrefab; // Prefab UI cho la bàn (có Image và TextMeshPro)

    [Header("UI Parent Transforms")]
    public RectTransform minimapWaypointsParent; // Kéo Transform cha của các icon waypoint trên minimap
    public RectTransform compassWaypointsParent; // Kéo Transform cha của các icon waypoint trên la bàn

    [Header("Player Reference")]
    public Transform playerTransform; // Transform của người chơi để tính khoảng cách

    // Dictionary để lưu trữ tất cả Waypoint đang hoạt động
    public Dictionary<string, Waypoint> activeWaypoints = new Dictionary<string, Waypoint>();
    // Dictionary để lưu trữ tham chiếu đến các UI Waypoint trên minimap
    public Dictionary<string, WaypointUI> minimapWaypointUIs = new Dictionary<string, WaypointUI>();
    // Dictionary để lưu trữ tham chiếu đến các UI Waypoint trên la bàn
    public Dictionary<string, WaypointUI> compassWaypointUIs = new Dictionary<string, WaypointUI>();

    // Event để thông báo khi Waypoint được thêm/xóa/cập nhật
    public event Action<Waypoint> OnWaypointAdded;
    public event Action<string> OnWaypointRemoved;
    public event Action<Waypoint> OnActiveWaypointChanged;

    private Waypoint currentActiveWaypoint; // Waypoint hiện tại đang được theo dõi (ví dụ: mục tiêu nhiệm vụ)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Nếu bạn muốn nó tồn tại qua các cảnh: DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (minimapWaypointPrefab == null) Debug.LogError("Minimap Waypoint Prefab is not assigned in WaypointManager!");
        if (compassWaypointPrefab == null) Debug.LogError("Compass Waypoint Prefab is not assigned in WaypointManager!");
        if (minimapWaypointsParent == null) Debug.LogError("Minimap Waypoints Parent is not assigned in WaypointManager!");
        if (compassWaypointsParent == null) Debug.LogError("Compass Waypoints Parent is not assigned in WaypointManager!");
        if (playerTransform == null) Debug.LogError("Player Transform is not assigned in WaypointManager!");
    }

    /// <summary>
    /// Thêm một Waypoint mới vào hệ thống.
    /// </summary>
    /// <param name="waypoint">Đối tượng Waypoint cần thêm.</param>
    /// <param name="makeActive">Đặt Waypoint này làm Waypoint đang được theo dõi chính không?</param>
    public void AddWaypoint(Waypoint waypoint, bool makeActive = false)
    {
        if (activeWaypoints.ContainsKey(waypoint.id))
        {
            Debug.LogWarning($"Waypoint with ID '{waypoint.id}' already exists. Updating existing waypoint.");
            RemoveWaypoint(waypoint.id); // Xóa bản cũ nếu đã tồn tại
        }

        activeWaypoints.Add(waypoint.id, waypoint);

        // Tạo UI cho minimap
        if (minimapWaypointPrefab != null && minimapWaypointsParent != null)
        {
            WaypointUI ui = Instantiate(minimapWaypointPrefab, minimapWaypointsParent);
            ui.Initialize(waypoint, playerTransform);
            minimapWaypointUIs.Add(waypoint.id, ui);
        }

        // Tạo UI cho la bàn
        if (compassWaypointPrefab != null && compassWaypointsParent != null)
        {
            WaypointUI ui = Instantiate(compassWaypointPrefab, compassWaypointsParent);
            ui.Initialize(waypoint, playerTransform);
            compassWaypointUIs.Add(waypoint.id, ui);
        }

        if (makeActive)
        {
            SetActiveWaypoint(waypoint.id);
        }

        OnWaypointAdded?.Invoke(waypoint);
        Debug.Log($"Waypoint '{waypoint.displayName}' added.");
    }

    /// <summary>
    /// Xóa một Waypoint khỏi hệ thống.
    /// </summary>
    /// <param name="id">ID của Waypoint cần xóa.</param>
    public void RemoveWaypoint(string id)
    {
        if (activeWaypoints.TryGetValue(id, out Waypoint waypoint))
        {
            activeWaypoints.Remove(id);

            if (minimapWaypointUIs.TryGetValue(id, out WaypointUI minimapUI))
            {
                Destroy(minimapUI.gameObject);
                minimapWaypointUIs.Remove(id);
            }
            if (compassWaypointUIs.TryGetValue(id, out WaypointUI compassUI))
            {
                Destroy(compassUI.gameObject);
                compassWaypointUIs.Remove(id);
            }

            if (currentActiveWaypoint != null && currentActiveWaypoint.id == id)
            {
                SetActiveWaypoint(null); // Xóa active waypoint nếu nó bị xóa
            }

            OnWaypointRemoved?.Invoke(id);
            Debug.Log($"Waypoint '{waypoint.displayName}' removed.");
        }
        else
        {
            Debug.LogWarning($"Waypoint with ID '{id}' not found for removal.");
        }
    }

    /// <summary>
    /// Đặt một Waypoint làm Waypoint đang được theo dõi chính.
    /// </summary>
    /// <param name="id">ID của Waypoint. Null để xóa waypoint đang theo dõi.</param>
    public void SetActiveWaypoint(string id)
    {
        if (id == null)
        {
            currentActiveWaypoint = null;
            OnActiveWaypointChanged?.Invoke(null);
            Debug.Log("Active Waypoint cleared.");
            return;
        }

        if (activeWaypoints.TryGetValue(id, out Waypoint waypoint))
        {
            currentActiveWaypoint = waypoint;
            OnActiveWaypointChanged?.Invoke(waypoint);
            Debug.Log($"Active Waypoint set to '{waypoint.displayName}'.");
        }
        else
        {
            Debug.LogWarning($"Waypoint with ID '{id}' not found to set as active.");
        }
    }

    /// <summary>
    /// Lấy Waypoint đang được theo dõi chính.
    /// </summary>
    public Waypoint GetActiveWaypoint()
    {
        return currentActiveWaypoint;
    }

    /// <summary>
    /// Lấy khoảng cách tới một Waypoint.
    /// </summary>
    public float GetDistanceToWaypoint(string id)
    {
        if (playerTransform == null) return -1f;

        if (activeWaypoints.TryGetValue(id, out Waypoint waypoint))
        {
            return Vector3.Distance(playerTransform.position, waypoint.worldPosition);
        }
        return -1f;
    }

    /// <summary>
    /// Lấy khoảng cách tới Waypoint đang theo dõi chính.
    /// </summary>
    public float GetDistanceToActiveWaypoint()
    {
        if (currentActiveWaypoint == null) return -1f;
        return GetDistanceToWaypoint(currentActiveWaypoint.id);
    }
}
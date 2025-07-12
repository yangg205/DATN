using Pathfinding; // Đảm bảo đã import A* Pathfinding Project
using System.Collections.Generic;
using UnityEngine;
using System; // Để sử dụng Action cho event OnActiveWaypointChanged nếu WaypointManager dùng

[RequireComponent(typeof(LineRenderer), typeof(Seeker))]
public class GlowTrailPathfinder : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform; // Kéo (drag) người chơi của bạn vào đây trong Inspector!
    public LineRenderer glowTrailLineRenderer;

    [Header("Settings")]
    [Tooltip("Thời gian giữa các lần tính toán lại đường đi (giây).")]
    public float updateInterval = 0.5f; // Thời gian giữa các lần tính toán lại đường đi
    [Tooltip("Khoảng cách người chơi phải di chuyển để kích hoạt tính toán lại đường đi.")]
    public float pathRecalculateDistance = 1.0f; // Tính toán lại đường đi nếu người chơi di chuyển xa hơn khoảng này
    [Tooltip("Khoảng cách nâng vệt sáng lên khỏi mặt đất.")]
    public float heightOffset = 0.2f;
    [Tooltip("Thời gian vệt sáng hiển thị khi kích hoạt thủ công (bằng phím V).")]
    public float trailVisibleDuration = 5f;

    private Seeker seeker;
    public Path currentPath { get; private set; } // Public getter để các script khác có thể truy cập path
    private Vector3 lastPathStartPoint; // Để theo dõi khi nào cần tính toán lại đường đi

    private Waypoint currentActiveWaypoint = null;
    private float updateTimer = 0f;
    private float trailVisibilityTimer = 0f;

    private bool trailManualActivated = false; // True nếu được kích hoạt bằng phím 'V'
    private bool trailAutoActivated = false;   // True nếu được kích hoạt bởi sự thay đổi waypoint active

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        if (glowTrailLineRenderer == null)
            glowTrailLineRenderer = GetComponent<LineRenderer>();

        // Thiết lập cơ bản cho LineRenderer
        glowTrailLineRenderer.positionCount = 0;
        glowTrailLineRenderer.useWorldSpace = true;
        glowTrailLineRenderer.alignment = LineAlignment.View; // Để vệt sáng luôn nhìn về camera
        glowTrailLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        glowTrailLineRenderer.receiveShadows = false;
        glowTrailLineRenderer.widthMultiplier = 0.2f; // Độ dày của vệt sáng
        glowTrailLineRenderer.enabled = false; // Mặc định tắt khi khởi tạo
        Debug.Log("[GlowTrailPathfinder] Awake completed. LineRenderer and Seeker initialized.");
    }

    private void OnEnable()
    {
        // Đăng ký sự kiện thay đổi waypoint active từ WaypointManager
        if (WaypointManager.Instance != null)
        {
            WaypointManager.Instance.OnActiveWaypointChanged += OnActiveWaypointChanged;
            // Kiểm tra ngay lập tức waypoint active hiện tại khi script được bật
            OnActiveWaypointChanged(WaypointManager.Instance.GetActiveWaypoint());
            Debug.Log("[GlowTrailPathfinder] Subscribed to OnActiveWaypointChanged event.");
        }
        else
        {
            Debug.LogWarning("[GlowTrailPathfinder] WaypointManager.Instance is NULL on OnEnable. Cannot subscribe to waypoint changes.");
        }
    }

    private void OnDisable()
    {
        // Hủy đăng ký để tránh rò rỉ bộ nhớ
        if (WaypointManager.Instance != null)
        {
            WaypointManager.Instance.OnActiveWaypointChanged -= OnActiveWaypointChanged;
            Debug.Log("[GlowTrailPathfinder] Unsubscribed from OnActiveWaypointChanged event.");
        }
        ClearTrail(); // Xóa vệt sáng khi script bị tắt
    }

    private void OnActiveWaypointChanged(Waypoint newWaypoint)
    {
        Debug.Log($"[GlowTrailPathfinder] OnActiveWaypointChanged received: {(newWaypoint != null ? newWaypoint.id : "NULL")}");
        currentActiveWaypoint = newWaypoint;

        if (currentActiveWaypoint != null)
        {
            // Nếu có waypoint active, tự động bật vệt sáng (trừ khi đang bật thủ công để ưu tiên thời gian)
            if (!trailManualActivated)
            {
                trailAutoActivated = true;
                // Yêu cầu path ngay lập tức khi waypoint active thay đổi
                RequestPath(playerTransform.position, currentActiveWaypoint.worldPosition);
                glowTrailLineRenderer.enabled = true;
                Debug.Log("[GlowTrailPathfinder] Trail activated automatically due to active waypoint change.");
            }
            // Nếu đang kích hoạt thủ công, giữ nguyên timer của nó.
        }
        else
        {
            // Nếu không có waypoint active, xóa vệt sáng
            ClearTrail();
            Debug.Log("[GlowTrailPathfinder] Trail cleared because no active waypoint is set.");
        }
    }

    private void Update()
    {
        // Kiểm tra các tham chiếu cần thiết
        if (WaypointManager.Instance == null || playerTransform == null)
        {
            ClearTrail();
            if (playerTransform == null)
            {
                Debug.LogWarning("[GlowTrailPathfinder] Player Transform is NULL. Please assign it in the Inspector for GlowTrailPathfinder to work.");
            }
            return;
        }

        // --- Xử lý kích hoạt thủ công (phím V) ---
        if (Input.GetKeyDown(KeyCode.V))
        {
            var activeWaypoint = WaypointManager.Instance.GetActiveWaypoint();
            if (activeWaypoint != null)
            {
                currentActiveWaypoint = activeWaypoint; // Đảm bảo currentActiveWaypoint được thiết lập
                RequestPath(playerTransform.position, activeWaypoint.worldPosition);
                trailManualActivated = true;
                trailAutoActivated = false; // Kích hoạt thủ công ghi đè tự động
                trailVisibilityTimer = trailVisibleDuration; // Bắt đầu đếm ngược thời gian hiển thị
                glowTrailLineRenderer.enabled = true;
                Debug.Log("[GlowTrailPathfinder] Trail manually activated by 'V' key.");
            }
            else
            {
                Debug.LogWarning("[GlowTrailPathfinder] Cannot activate trail with 'V' key: No active waypoint set in WaypointManager.");
                ClearTrail(); // Không có waypoint thì xóa trail
            }
        }

        // --- Xử lý hẹn giờ hiển thị vệt sáng khi kích hoạt thủ công ---
        if (trailManualActivated)
        {
            trailVisibilityTimer -= Time.deltaTime;
            if (trailVisibilityTimer <= 0f)
            {
                ClearTrail();
                Debug.Log("[GlowTrailPathfinder] Trail cleared due to manual activation timeout.");
            }
        }

        // --- Logic cập nhật đường dẫn liên tục ---
        // Vệt sáng sẽ cập nhật nếu có waypoint active VÀ đang trong trạng thái được kích hoạt (thủ công hoặc tự động)
        if (currentActiveWaypoint != null && (trailManualActivated || trailAutoActivated))
        {
            updateTimer += Time.deltaTime;
            float distanceToLastPathStart = Vector3.Distance(playerTransform.position, lastPathStartPoint);

            // Debug log để kiểm tra điều kiện cập nhật
            // Debug.Log($"[GlowTrailPathfinder] Update check: Timer={updateTimer:F2}/{updateInterval:F2}, Dist={distanceToLastPathStart:F2}/{pathRecalculateDistance:F2}");

            // Tính toán lại đường đi định kỳ HOẶC nếu người chơi đã di chuyển đáng kể
            if (updateTimer >= updateInterval || distanceToLastPathStart > pathRecalculateDistance)
            {
                updateTimer = 0f; // Reset timer
                lastPathStartPoint = playerTransform.position; // Cập nhật điểm bắt đầu path
                RequestPath(playerTransform.position, currentActiveWaypoint.worldPosition);
                // Debug.Log($"[GlowTrailPathfinder] Requesting new path due to movement/time. Start: {playerTransform.position}, End: {currentActiveWaypoint.worldPosition}");
            }

            // Luôn vẽ lại đường dẫn hiện có (nếu có), ngay cả khi không tính toán lại trong khung hình này.
            // Điều này đảm bảo LineRenderer được cập nhật vị trí các điểm nếu LineAlignment.View được sử dụng,
            // hoặc đơn giản là để vẽ lại path nếu LineRenderer bị tắt/bật lại.
            if (currentPath != null && currentPath.vectorPath != null && currentPath.vectorPath.Count > 0)
            {
                DrawGlowTrail(currentPath.vectorPath);
            }
            else if (glowTrailLineRenderer.enabled) // Nếu không có path nhưng renderer vẫn bật, hãy tắt nó
            {
                Debug.LogWarning("[GlowTrailPathfinder] No path to draw but LineRenderer is enabled. Disabling LineRenderer.");
                glowTrailLineRenderer.enabled = false;
            }
        }
        else if (glowTrailLineRenderer.enabled) // Nếu không có waypoint active hoặc không được kích hoạt, tắt vệt sáng
        {
            ClearTrail();
            // Debug.Log("[GlowTrailPathfinder] Trail disabled because no active waypoint or not activated.");
        }
    }

    private void RequestPath(Vector3 startPos, Vector3 endPos)
    {
        // Đảm bảo Seeker component đang hoạt động
        if (seeker == null || !seeker.enabled) // Đã sửa từ IsActive() sang enabled
        {
            Debug.LogWarning("[GlowTrailPathfinder] Seeker component is NULL or not enabled. Cannot request path.");
            return;
        }
        //Debug.Log($"[GlowTrailPathfinder] Requesting path from {startPos} to {endPos} for waypoint '{currentActiveWaypoint?.id}'.");
        seeker.StartPath(startPos, endPos, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            currentPath = p;
            //Debug.Log($"[GlowTrailPathfinder] Path calculated with {p.vectorPath.Count} points. Drawing trail.");
            DrawGlowTrail(p.vectorPath);
            glowTrailLineRenderer.enabled = true; // Đảm bảo renderer bật sau khi path hoàn thành
        }
        else
        {
            Debug.LogError($"[GlowTrailPathfinder] Pathfinding error: {p.errorLog}");
            ClearTrail();
        }
    }

    private void DrawGlowTrail(List<Vector3> pathPoints)
    {
        if (pathPoints == null || pathPoints.Count == 0)
        {
            ClearTrail();
            return;
        }

        glowTrailLineRenderer.positionCount = pathPoints.Count;
        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector3 pos = pathPoints[i];
            pos.y += heightOffset; // Nâng vệt sáng lên khỏi mặt đất một chút
            glowTrailLineRenderer.SetPosition(i, pos);
        }
    }

    public void ClearTrail()
    {
        if (glowTrailLineRenderer != null)
        {
            glowTrailLineRenderer.positionCount = 0;
            glowTrailLineRenderer.enabled = false;
        }
        currentPath = null;
        trailManualActivated = false;
        trailAutoActivated = false;
        trailVisibilityTimer = 0f;
        updateTimer = 0f;
        currentActiveWaypoint = null; // Xóa tham chiếu waypoint active
        Debug.Log("[GlowTrailPathfinder] Trail cleared.");
    }
}
using Pathfinding; // Đảm bảo đã import A* Pathfinding Project
using System.Collections; // Cần thiết cho Coroutine
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Seeker))]
public class GlowTrailPathfinder : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform; // Kéo (drag) người chơi của bạn vào đây trong Inspector!
    public ParticleSystem glowTrailParticleSystem; // Kéo Particle System của bạn vào đây!

    [Header("Settings")]
    [Tooltip("Thời gian giữa các lần tính toán lại đường đi (giây).")]
    public float updateInterval = 0.2f; // Tăng tần suất cập nhật để đường mượt hơn
    [Tooltip("Khoảng cách người chơi phải di chuyển để kích hoạt tính toán lại đường đi.")]
    public float pathRecalculateDistance = 0.5f; // Giảm khoảng cách để cập nhật thường xuyên hơn
    [Tooltip("Khoảng cách nâng vệt sáng lên khỏi mặt đất.")]
    public float heightOffset = 0.2f;
    [Tooltip("Thời gian vệt sáng hiển thị sau khi kích hoạt (bằng phím V hoặc tự động).")]
    public float trailVisibleDuration = 5f; // Thời gian đường dẫn tồn tại sau khi được kích hoạt
    [Tooltip("Khoảng cách giữa các hạt đom đóm trên đường đi.")]
    public float particleSpacing = 0.3f; // Giảm khoảng cách để đường mượt hơn
    [Tooltip("Số lượng hạt tối đa cho phép hiển thị.")]
    public int maxParticles = 1000; // Tăng giới hạn hạt
    [Tooltip("Thời gian để vệt sáng chạy từ đầu đến cuối đường (giây).")]
    public float trailRevealDuration = 0.7f; // Thời gian để đường đi xuất hiện hoàn chỉnh (giảm xuống để cảm giác nhanh hơn)

    private Seeker seeker;
    public Path currentPath { get; private set; } // Public getter để các script khác có thể truy cập path
    private Vector3 lastPathStartPoint; // Để theo dõi khi nào cần tính toán lại đường đi

    private Waypoint currentActiveWaypoint = null;
    private float updateTimer = 0f;
    private float trailVisibilityTimer = 0f;

    private bool trailIsActive = false; // True nếu vệt sáng đang được hiển thị (thủ công hoặc tự động)
    private Coroutine revealTrailCoroutine; // Để kiểm soát Coroutine đang chạy

    // Particle System module references
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emissionModule;

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        if (glowTrailParticleSystem == null)
        {
            Debug.LogError("[GlowTrailPathfinder] ParticleSystem reference is NULL. Please assign it in the Inspector!");
            enabled = false; // Tắt script nếu không có Particle System
            return;
        }

        mainModule = glowTrailParticleSystem.main;
        emissionModule = glowTrailParticleSystem.emission;

        // Đảm bảo Particle System ban đầu không phát hạt và sạch sẽ
        emissionModule.enabled = false; // Tắt việc phát hạt tự động
        glowTrailParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // Dừng và xóa tất cả hạt
        Debug.Log("[GlowTrailPathfinder] Awake completed. Particle System and Seeker initialized.");
    }

    private void OnEnable()
    {
        if (WaypointManager.Instance != null)
        {
            WaypointManager.Instance.OnActiveWaypointChanged += OnActiveWaypointChanged;
            OnActiveWaypointChanged(WaypointManager.Instance.GetActiveWaypoint()); // Kiểm tra waypoint hiện tại
            Debug.Log("[GlowTrailPathfinder] Subscribed to OnActiveWaypointChanged event.");
        }
        else
        {
            Debug.LogWarning("[GlowTrailPathfinder] WaypointManager.Instance is NULL on OnEnable. Cannot subscribe to waypoint changes.");
        }
    }

    private void OnDisable()
    {
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
            // Khi waypoint active thay đổi, tự động kích hoạt vệt sáng và yêu cầu path ngay lập tức
            trailIsActive = true;
            trailVisibilityTimer = trailVisibleDuration; // Bắt đầu đếm ngược thời gian hiển thị
            RequestPath(playerTransform.position, currentActiveWaypoint.worldPosition);
            Debug.Log("[GlowTrailPathfinder] Trail activated automatically due to active waypoint change.");
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
        if (WaypointManager.Instance == null || playerTransform == null || glowTrailParticleSystem == null)
        {
            ClearTrail();
            if (playerTransform == null) Debug.LogWarning("[GlowTrailPathfinder] Player Transform is NULL. Please assign it in the Inspector.");
            if (glowTrailParticleSystem == null) Debug.LogWarning("[GlowTrailPathfinder] Particle System is NULL. Please assign it in the Inspector.");
            return;
        }

        // --- Xử lý kích hoạt thủ công (phím V) ---
        if (Input.GetKeyDown(KeyCode.V))
        {
            var activeWaypoint = WaypointManager.Instance.GetActiveWaypoint();
            if (activeWaypoint != null)
            {
                currentActiveWaypoint = activeWaypoint;
                trailIsActive = true;
                trailVisibilityTimer = trailVisibleDuration; // Bắt đầu đếm ngược thời gian hiển thị
                RequestPath(playerTransform.position, activeWaypoint.worldPosition);
                Debug.Log("[GlowTrailPathfinder] Trail manually activated by 'V' key.");
            }
            else
            {
                Debug.LogWarning("[GlowTrailPathfinder] Cannot activate trail with 'V' key: No active waypoint set in WaypointManager.");
                ClearTrail();
            }
        }

        // --- Xử lý hẹn giờ hiển thị vệt sáng ---
        if (trailIsActive)
        {
            trailVisibilityTimer -= Time.deltaTime;
            if (trailVisibilityTimer <= 0f)
            {
                ClearTrail();
                Debug.Log("[GlowTrailPathfinder] Trail cleared due to timeout.");
            }
            else
            {
                // Nếu vệt sáng đang active và còn thời gian, kiểm tra để update path
                updateTimer += Time.deltaTime;
                float distanceToLastPathStart = Vector3.Distance(playerTransform.position, lastPathStartPoint);

                // Yêu cầu path nếu người chơi di chuyển đáng kể hoặc đến thời gian cập nhật
                if (currentActiveWaypoint != null && (updateTimer >= updateInterval || distanceToLastPathStart > pathRecalculateDistance))
                {
                    updateTimer = 0f; // Reset timer
                    lastPathStartPoint = playerTransform.position; // Cập nhật điểm bắt đầu path
                    RequestPath(playerTransform.position, currentActiveWaypoint.worldPosition);
                    //Debug.Log($"[GlowTrailPathfinder] Re-requesting path due to movement/time. Player: {playerTransform.position}, Waypoint: {currentActiveWaypoint.worldPosition}");
                }
            }
        }
    }

    private void RequestPath(Vector3 startPos, Vector3 endPos)
    {
        if (seeker == null || !seeker.enabled)
        {
            Debug.LogWarning("[GlowTrailPathfinder] Seeker component is NULL or not enabled. Cannot request path.");
            return;
        }
        seeker.StartPath(startPos, endPos, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            currentPath = p;
            Debug.Log($"[GlowTrailPathfinder] Path calculated with {p.vectorPath.Count} points. Starting trail reveal.");

            // Dừng coroutine cũ nếu có để tránh xung đột
            if (revealTrailCoroutine != null)
            {
                StopCoroutine(revealTrailCoroutine);
            }
            // Khởi động coroutine mới để hiển thị đường đi dần dần
            revealTrailCoroutine = StartCoroutine(RevealGlowTrailParticles(p.vectorPath));
        }
        else
        {
            Debug.LogError($"[GlowTrailPathfinder] Pathfinding error: {p.errorLog}");
            ClearTrail();
        }
    }

    private IEnumerator RevealGlowTrailParticles(List<Vector3> pathPoints)
    {
        if (pathPoints == null || pathPoints.Count < 2)
        {
            ClearTrail();
            yield break; // Thoát khỏi coroutine
        }

        // Dừng và xóa tất cả các hạt hiện có trước khi bắt đầu lộ diện đường mới
        glowTrailParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        emissionModule.enabled = false; // Đảm bảo emission tắt trước khi emit thủ công

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.startLifetime = mainModule.startLifetime.constant;
        emitParams.startSize = mainModule.startSize.constant;
        emitParams.startColor = mainModule.startColor.color;
        emitParams.velocity = Vector3.zero; // Hạt đứng yên tại chỗ sinh ra

        int emittedCount = 0;
        Vector3 tempPos;

        // Tính tổng độ dài đường đi
        float totalPathLength = 0f;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            totalPathLength += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
        }

        // Xử lý trường hợp đường quá ngắn để tránh lỗi chia cho 0 hoặc hiển thị không đúng
        if (totalPathLength < particleSpacing * 0.5f)
        {
            // Nếu đường quá ngắn, chỉ phát một hạt tại điểm đích
            tempPos = pathPoints[pathPoints.Count - 1]; // Phát tại điểm cuối cùng
            tempPos.y += heightOffset;
            emitParams.position = tempPos;
            glowTrailParticleSystem.Emit(emitParams, 1);
            glowTrailParticleSystem.Play(); // Đảm bảo Particle System đang chạy
            yield break; // Kết thúc coroutine
        }

        // --- Logic mới để vệt sáng chạy từ đầu đến cuối ---
        float timeElapsed = 0f;
        float currentLerpT = 0f; // Vị trí hiện tại trên toàn bộ đường đi (0.0 đến 1.0)

        Vector3 lastEmittedPoint = pathPoints[0]; // Điểm cuối cùng mà một hạt đã được emit

        while (timeElapsed < trailRevealDuration)
        {
            timeElapsed += Time.deltaTime;
            currentLerpT = Mathf.Clamp01(timeElapsed / trailRevealDuration);

            // Tìm điểm hiện tại trên đường đi dựa vào currentLerpT
            Vector3 currentRevealPoint = GetPointAlongPath(pathPoints, currentLerpT);

            // Phát hạt nếu đủ khoảng cách từ hạt cuối cùng được emit
            if (emittedCount == 0 || Vector3.Distance(lastEmittedPoint, currentRevealPoint) >= particleSpacing)
            {
                if (emittedCount >= maxParticles)
                {
                    Debug.LogWarning("[GlowTrailPathfinder] Reached maxParticles limit. Not emitting more particles.");
                    // Vẫn tiếp tục yield null để coroutine chạy hết thời gian
                    // nhưng không emit thêm hạt
                }
                else
                {
                    tempPos = currentRevealPoint;
                    tempPos.y += heightOffset;
                    emitParams.position = tempPos;
                    glowTrailParticleSystem.Emit(emitParams, 1);
                    emittedCount++;
                    lastEmittedPoint = currentRevealPoint; // Cập nhật điểm cuối cùng đã phát
                    glowTrailParticleSystem.Play(); // Đảm bảo Particle System đang chơi
                }
            }
            yield return null; // Chờ 1 frame trước khi tiếp tục
        }

        // Đảm bảo tất cả các hạt trên đường đã được phát khi kết thúc thời gian reveal
        // (Trong trường hợp path dài và trailRevealDuration ngắn)
        tempPos = GetPointAlongPath(pathPoints, 1.0f); // Điểm cuối cùng của path
        if (Vector3.Distance(lastEmittedPoint, tempPos) >= particleSpacing || emittedCount == 0) // Emit hạt cuối nếu cần
        {
            if (emittedCount < maxParticles)
            {
                tempPos.y += heightOffset;
                emitParams.position = tempPos;
                glowTrailParticleSystem.Emit(emitParams, 1);
                glowTrailParticleSystem.Play();
            }
        }

        Debug.Log("[GlowTrailPathfinder] Trail reveal completed.");
    }

    /// <summary>
    /// Lấy một điểm trên đường đi dựa trên tỷ lệ (0.0 đến 1.0)
    /// </summary>
    /// <param name="path">Danh sách các điểm của đường đi.</param>
    /// <param name="t">Tỷ lệ (0.0 là bắt đầu, 1.0 là kết thúc).</param>
    /// <returns>Vị trí 3D trên đường đi.</returns>
    private Vector3 GetPointAlongPath(List<Vector3> path, float t)
    {
        if (path == null || path.Count < 2) return Vector3.zero;

        // Nếu t = 0, trả về điểm đầu tiên
        if (t <= 0f) return path[0];
        // Nếu t = 1, trả về điểm cuối cùng
        if (t >= 1f) return path[path.Count - 1];

        float totalLength = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            totalLength += Vector3.Distance(path[i], path[i + 1]);
        }

        float targetLength = t * totalLength;
        float currentLength = 0f;

        for (int i = 0; i < path.Count - 1; i++)
        {
            float segmentLength = Vector3.Distance(path[i], path[i + 1]);
            if (currentLength + segmentLength >= targetLength)
            {
                // Điểm nằm trong đoạn hiện tại
                float segmentT = (targetLength - currentLength) / segmentLength;
                return Vector3.Lerp(path[i], path[i + 1], segmentT);
            }
            currentLength += segmentLength;
        }

        // Trường hợp fallback (không nên xảy ra nếu logic đúng)
        return path[path.Count - 1];
    }


    public void ClearTrail()
    {
        // Dừng coroutine đang chạy nếu có
        if (revealTrailCoroutine != null)
        {
            StopCoroutine(revealTrailCoroutine);
            revealTrailCoroutine = null;
        }

        // Dừng Particle System và xóa tất cả các hạt đang hiển thị
        if (glowTrailParticleSystem != null)
        {
            glowTrailParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            emissionModule.enabled = false; // Đảm bảo emission tắt
        }
        currentPath = null;
        trailIsActive = false; // Đặt lại trạng thái không active
        trailVisibilityTimer = 0f;
        updateTimer = 0f;
        currentActiveWaypoint = null; // Xóa tham chiếu waypoint active
        Debug.Log("[GlowTrailPathfinder] Trail cleared.");
    }
}
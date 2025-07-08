using UnityEngine;

// Không có namespace

public class WorldWaypoint : MonoBehaviour
{
    public string waypointId;
    public Vector3 worldPosition;

    [Tooltip("Thời gian tồn tại của waypoint 3D (ví dụ: custom marker). Đặt 0 để tồn tại vĩnh viễn.")]
    public float lifetime = 0f;

    private float _timer;

    public void Initialize(string id, Vector3 pos, float life = 0f)
    {
        this.waypointId = id;
        this.worldPosition = pos;
        this.lifetime = life;
        this.transform.position = pos; // Đảm bảo vị trí được thiết lập
        _timer = 0f;

        Debug.Log($"[WorldWaypoint] Initialized Waypoint '{id}' at {pos}. Lifetime: {lifetime}");
    }

    void Update()
    {
        if (lifetime > 0)
        {
            _timer += Time.deltaTime;
            if (_timer >= lifetime)
            {
                // Gọi WaypointManager để xóa cả 3D và 2D UI của waypoint này
                if (WaypointManager.Instance != null)
                {
                    WaypointManager.Instance.RemoveWaypoint(waypointId);
                }
                else
                {
                    Destroy(gameObject); // Fallback: tự hủy nếu manager không tồn tại
                }
            }
        }
    }
}
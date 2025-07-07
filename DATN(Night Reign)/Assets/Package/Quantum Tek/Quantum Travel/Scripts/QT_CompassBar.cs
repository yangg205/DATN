using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System; // For events

namespace QuantumTek.QuantumTravel
{
    // Cần phải là MonoBehavior để nhận Awake, Update
    public class QT_CompassBar : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform = null;
        [SerializeField] private RectTransform barBackground = null;
        [SerializeField] private RawImage image = null;

        // BỎ `public QT_MapObject ReferenceObject;` vì giờ lấy từ WaypointManager
        // BỎ `public List<QT_MapObject> Objects = new List<QT_MapObject>();`
        // BỎ `public QT_MapMarker MarkerPrefab;`
        // BỎ `public List<QT_MapMarker> Markers { get; set; } = new List<QT_MapMarker>();`

        // Thay thế bằng WaypointManager và WaypointUI
        [Header("Waypoint UI Parent")]
        [SerializeField] private RectTransform waypointUIParent; // Kéo Compass_WaypointsParent vào đây

        [Header("Compass Bar Variables")]
        public Vector2 CompassSize = new Vector2(200, 25);
        public Vector2 ShownCompassSize = new Vector2(100, 25);
        public float MaxRenderDistance = 100f; // Khoảng cách tối đa marker hiển thị trên la bàn
        public float MarkerSize = 20; // Kích thước cơ bản của marker
        public float MinScale = 0.5f; // Tỉ lệ nhỏ nhất khi ở xa
        public float MaxScale = 1.0f; // Tỉ lệ lớn nhất khi ở gần

        [Tooltip("Adjust this to center 'North' (or your desired default direction) on the compass texture at 0 degrees. " +
                 "If North is at the very left edge of your seamless texture, this might be 0. " +
                 "If North is at the center of the texture's visible part when player camera is facing North (Z+), this might be 0.5. " +
                 "Experiment until North aligns correctly.")]
        [Range(-1f, 1f)]
        public float compassTextureInitialOffset = 0f;

        [Header("Camera Reference")]
        public Transform playerMainCameraTransform; // Kéo Camera chính của người chơi vào đây

        // SharedMapRotationAngle giờ đây chỉ còn là biến để tính toán UV,
        // Nó KHÔNG ĐỒNG BỘ với góc xoay của UI nữa.
        // QT_CompassBar TỰ TÍNH UV của nó.
        // SmallMinimapController sẽ lấy angle trực tiếp từ playerMainCameraTransform.
        // Có thể biến này không cần static nếu chỉ dùng trong script này.
        // Để nhất quán với tên ban đầu, tôi sẽ giữ nó là static và để SmallMinimapController dùng chung.
        public static float SharedMapRotationAngle_ForUV = 0f; // Đổi tên để rõ ràng hơn

        private void Awake()
        {
            if (rectTransform == null) Debug.LogError("RectTransform is null!");
            if (barBackground == null) Debug.LogError("BarBackground is null!");
            if (image == null) Debug.LogError("Image is null!");
            if (playerMainCameraTransform == null) Debug.LogError("Player Main Camera Transform is null! Compass will not rotate correctly.");
            if (waypointUIParent == null) Debug.LogError("Waypoint UI Parent (Compass_WaypointsParent) is null!");

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ShownCompassSize.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ShownCompassSize.y);
            barBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CompassSize.x);
            barBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CompassSize.y);
        }

        private void OnEnable()
        {
            // Đăng ký sự kiện khi waypoint được thêm/xóa để cập nhật UI
            if (WaypointManager.Instance != null)
            {
                WaypointManager.Instance.OnWaypointAdded += HandleWaypointAdded;
                WaypointManager.Instance.OnWaypointRemoved += HandleWaypointRemoved;
            }
        }

        private void OnDisable()
        {
            if (WaypointManager.Instance != null)
            {
                WaypointManager.Instance.OnWaypointAdded -= HandleWaypointAdded;
                WaypointManager.Instance.OnWaypointRemoved -= HandleWaypointRemoved;
            }
        }

        private void HandleWaypointAdded(Waypoint waypoint)
        {
            // WaypointManager tự tạo UI, QT_CompassBar không cần tạo lại.
            // Nhưng cần đảm bảo QT_MapMarker (cũ) không bị nhầm lẫn với WaypointUI (mới).
            // WaypointManager sẽ tự động thêm WaypointUI vào `compassWaypointsParent`.
        }

        private void HandleWaypointRemoved(string waypointId)
        {
            // WaypointManager tự hủy UI, QT_CompassBar không cần xử lý.
        }

        private void Update()
        {
            if (playerMainCameraTransform == null) return;

            // Cập nhật SharedMapRotationAngle_ForUV LẤY TỪ GÓC XOAY Y CỦA CAMERA NGƯỜI CHƠI
            // Góc của camera (0-360) theo chiều kim đồng hồ từ trục Z+.
            // Dấu trừ để la bàn dịch chuyển UV ngược lại với góc quay của camera.
            SharedMapRotationAngle_ForUV = Mathf.Repeat(-playerMainCameraTransform.eulerAngles.y, 360f);

            // Cập nhật UV Rect của la bàn để nó xoay
            float uvX = (SharedMapRotationAngle_ForUV / 360f) + compassTextureInitialOffset;
            uvX = Mathf.Repeat(uvX, 1f); // Đảm bảo uvX nằm trong khoảng [0, 1)

            image.uvRect = new Rect(uvX, 0, 1, 1);

            // Cập nhật vị trí và tỉ lệ của các marker (UI Waypoint)
            // Lấy các UI Waypoint từ WaypointManager
            if (WaypointManager.Instance != null)
            {
                foreach (var entry in WaypointManager.Instance.activeWaypoints) // Truy cập public dictionary
                {
                    Waypoint waypoint = entry.Value;
                    if (WaypointManager.Instance.compassWaypointUIs.TryGetValue(waypoint.id, out WaypointUI markerUI))
                    {
                        // Kiểm tra khoảng cách để quyết định có hiển thị hay không
                        float distance = Vector3.Distance(WaypointManager.Instance.playerTransform.position, waypoint.worldPosition);

                        if (distance <= MaxRenderDistance)
                        {
                            markerUI.gameObject.SetActive(true);
                            markerUI.GetComponent<RectTransform>().anchoredPosition = CalculatePosition(waypoint);
                            // SetScale giờ là ở WaypointUI
                            // markerUI.SetScale(CalculateScale(waypoint)); // Removed, handled by WaypointUI if needed or simpler here
                            markerUI.GetComponent<RectTransform>().localScale = Vector3.one * CalculateScale(waypoint);
                        }
                        else
                        {
                            markerUI.gameObject.SetActive(false); // Ẩn nếu quá xa
                        }
                    }
                }
            }
        }

        // Tính toán vị trí X của marker trên la bàn
        private Vector2 CalculatePosition(Waypoint waypoint)
        {
            float compassDegree = ShownCompassSize.x / 360f; // Số pixel cho 1 độ trên la bàn hiển thị

            // Hướng từ người chơi đến waypoint trong mặt phẳng XZ (y=0)
            Vector3 directionToWaypoint = waypoint.worldPosition - WaypointManager.Instance.playerTransform.position;
            Vector2 direction2D = new Vector2(directionToWaypoint.x, directionToWaypoint.z).normalized;

            // Hướng nhìn của camera trong mặt phẳng XZ
            Vector2 playerForward2D = new Vector2(playerMainCameraTransform.forward.x, playerMainCameraTransform.forward.z).normalized;

            // Góc tương đối của waypoint so với hướng nhìn của người chơi (từ -180 đến 180)
            float markerRelativeAngle = Vector2.SignedAngle(playerForward2D, direction2D);

            // Chuyển đổi góc thành vị trí X trên UI la bàn
            // -ShownCompassSize.x / 2 là điểm cực trái, ShownCompassSize.x / 2 là điểm cực phải
            float xPos = markerRelativeAngle * compassDegree;

            // Đảm bảo marker nằm trong phạm vi hiển thị của la bàn (-ShownCompassSize.x / 2 đến +ShownCompassSize.x / 2)
            xPos = Mathf.Clamp(xPos, -ShownCompassSize.x / 2f, ShownCompassSize.x / 2f);

            return new Vector2(xPos, 0); // Vị trí Y cố định trên la bàn
        }


        private float CalculateScale(Waypoint waypoint)
        {
            float distance = Vector3.Distance(WaypointManager.Instance.playerTransform.position, waypoint.worldPosition);
            float scale = 0;

            if (distance < MaxRenderDistance)
            {
                // Tỉ lệ scale giảm dần khi khoảng cách tăng
                scale = Mathf.Lerp(MaxScale, MinScale, distance / MaxRenderDistance);
                scale = Mathf.Clamp(scale, MinScale, MaxScale);
            }
            return scale;
        }

        // public void AddMarker(QT_MapObject obj) và các hàm liên quan đến QT_MapObject CŨ đã bị loại bỏ.
        // Giờ đây WaypointManager sẽ quản lý việc thêm marker.
    }
}
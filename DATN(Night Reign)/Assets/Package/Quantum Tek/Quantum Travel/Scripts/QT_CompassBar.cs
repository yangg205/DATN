using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System; // For events

// QT_CompassBar giữ nguyên namespace QuantumTek.QuantumTravel theo yêu cầu của bạn trước đó.
// Tuy nhiên, nếu bạn thực sự muốn KHÔNG CÓ BẤT KỲ namespace nào, kể cả cho QT_CompassBar,
// thì bạn có thể xóa dòng "namespace QuantumTek.QuantumTravel" và dấu "}" cuối cùng.
// Tôi sẽ để nó như cũ vì bạn chỉ nói "t kh cần namespace" nói chung, nhưng QT_CompassBar ban đầu có.
// Nếu bạn muốn xóa namespace này, hãy nói rõ.

namespace QuantumTek.QuantumTravel
{
    public class QT_CompassBar : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform = null;
        [SerializeField] private RawImage image = null;

        [Header("Compass Bar Variables")]
        public Vector2 CompassSize = new Vector2(200, 25); // Kích thước texture của la bàn (ví dụ: 200px = 360 độ)
        public Vector2 ShownCompassSize = new Vector2(100, 25); // Kích thước hiển thị của thanh la bàn (ví dụ: 100px = 180 độ)
        public float MaxRenderDistance = 100f; // Khoảng cách tối đa marker hiển thị trên la bàn
        public float MinScale = 0.5f; // Tỉ lệ nhỏ nhất của marker khi ở xa
        public float MaxScale = 1.0f; // Tỉ lệ lớn nhất của marker khi ở gần

        [Tooltip("Adjust this to center 'North' (or your desired default direction) on the compass texture at 0 degrees. " +
                 "If North is at the very left edge of your seamless texture, this might be 0. " +
                 "If North is at the center of the texture's visible part when player camera is facing North (Z+), this might be 0.5. " +
                 "Experiment until North aligns correctly.")]
        [Range(-1f, 1f)]
        public float compassTextureInitialOffset = 0f;

        [Header("Camera Reference")]
        public Transform playerMainCameraTransform; // Kéo Camera chính của người chơi vào đây

        // Biến này được sử dụng để đồng bộ góc quay của bản đồ nhỏ nếu cần.
        // QT_CompassBar cập nhật nó, các script khác đọc nó (ví dụ: MinimapController có thể dùng để xoay minimap)
        public static float SharedMapRotationAngle_ForUV = 0f;

        private void Awake()
        {
            if (rectTransform == null) Debug.LogError("RectTransform is null on QT_CompassBar!");
            if (image == null) Debug.LogError("RawImage is null on QT_CompassBar!");
            if (playerMainCameraTransform == null) Debug.LogError("Player Main Camera Transform is null on QT_CompassBar! Compass will not rotate correctly.");

            // Kích thước của thanh la bàn hiển thị (RectTransform của QT_CompassBar)
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ShownCompassSize.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ShownCompassSize.y);

            // Kích thước của RawImage (background chứa texture la bàn)
            // Đảm bảo image có cùng RectTransform với QT_CompassBar hoặc là con của nó và fill toàn bộ.
            // Ở đây, tôi giả định image là component trên cùng GameObject với QT_CompassBar.
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CompassSize.x);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CompassSize.y);
        }

        private void Update()
        {
            if (playerMainCameraTransform == null) return;

            // Cập nhật SharedMapRotationAngle_ForUV LẤY TỪ GÓC XOAY Y CỦA CAMERA NGƯỜI CHƠI
            // Góc của camera (0-360) theo chiều kim đồng hồ từ trục Z+.
            // Dấu trừ để la bàn dịch chuyển UV ngược lại với góc quay của camera, tạo hiệu ứng la bàn quay.
            SharedMapRotationAngle_ForUV = Mathf.Repeat(-playerMainCameraTransform.eulerAngles.y, 360f);

            // Cập nhật UV Rect của la bàn để nó xoay
            float uvX = (SharedMapRotationAngle_ForUV / 360f) + compassTextureInitialOffset;
            uvX = Mathf.Repeat(uvX, 1f); // Đảm bảo uvX nằm trong khoảng [0, 1)

            image.uvRect = new Rect(uvX, 0, 1, 1);

            // Cập nhật vị trí và tỉ lệ của các marker (WaypointUI) trên la bàn
            if (WaypointManager.Instance != null && WaypointManager.Instance.playerTransform != null)
            {
                // Lấy tất cả các WaypointUI đang active trên Compass
                foreach (var entry in WaypointManager.Instance.compassWaypointUIs)
                {
                    string waypointId = entry.Key;
                    WaypointUI markerUI = entry.Value;

                    // Lấy dữ liệu Waypoint gốc từ WaypointManager
                    if (WaypointManager.Instance.activeWaypointsData.TryGetValue(waypointId, out Waypoint waypointData))
                    {
                        // Kiểm tra khoảng cách để quyết định có hiển thị hay không
                        float distance = Vector3.Distance(WaypointManager.Instance.playerTransform.position, waypointData.worldPosition);

                        if (distance <= MaxRenderDistance)
                        {
                            markerUI.gameObject.SetActive(true);
                            // Tính toán vị trí X trên thanh la bàn
                            Vector2 anchoredPos = CalculatePosition(waypointData);
                            // QT_CompassBar sẽ đặt vị trí và scale cho markerUI
                            markerUI.SetMapUIPosition(anchoredPos, CalculateScale(waypointData));
                        }
                        else
                        {
                            markerUI.gameObject.SetActive(false); // Ẩn nếu quá xa
                        }
                    }
                    else
                    {
                        // Waypoint Data không tồn tại, có thể do đã bị xóa từ nơi khác.
                        // Đảm bảo markerUI cũng bị hủy (đã được xử lý bởi WaypointManager.RemoveWaypoint)
                        if (markerUI != null) markerUI.gameObject.SetActive(false); // Ẩn phòng trường hợp
                    }
                }
            }
        }

        // Tính toán vị trí X của marker trên la bàn
        private Vector2 CalculatePosition(Waypoint waypoint)
        {
            // CompassSize.x là tổng chiều rộng của texture la bàn (ví dụ: 200px = 360 độ)
            // ShownCompassSize.x là chiều rộng của phần la bàn hiển thị (ví dụ: 100px = 180 độ)
            // Tỷ lệ pixel / độ cho thanh la bàn hiển thị (ví dụ: 100px / 180 độ = 0.555 px/deg)
            float pixelsPerDegreeOnShownCompass = ShownCompassSize.x / 180f; // Số pixel trên 1 độ hiển thị trên thanh la bàn

            // Hướng từ người chơi đến waypoint trong mặt phẳng XZ (y=0)
            Vector3 directionToWaypoint = waypoint.worldPosition - WaypointManager.Instance.playerTransform.position;
            Vector2 direction2D = new Vector2(directionToWaypoint.x, directionToWaypoint.z).normalized;

            // Hướng nhìn của camera trong mặt phẳng XZ
            Vector2 playerForward2D = new Vector2(playerMainCameraTransform.forward.x, playerMainCameraTransform.forward.z).normalized;

            // Góc tương đối của waypoint so với hướng nhìn của người chơi (từ -180 đến 180)
            // Góc 0 là thẳng phía trước, -90 là bên trái, 90 là bên phải.
            float markerRelativeAngle = Vector2.SignedAngle(playerForward2D, direction2D);

            // Chuyển đổi góc thành vị trí X trên UI la bàn
            // Giả sử tâm của thanh la bàn là 0.
            // markerRelativeAngle chạy từ -90 đến 90 (nếu chỉ hiển thị 180 độ).
            float xPos = markerRelativeAngle * pixelsPerDegreeOnShownCompass;

            // Đảm bảo marker nằm trong phạm vi hiển thị của la bàn
            // Thanh la bàn hiển thị có chiều rộng là ShownCompassSize.x. Tâm là 0.
            // Do đó, giới hạn là từ -ShownCompassSize.x / 2f đến +ShownCompassSize.x / 2f.
            xPos = Mathf.Clamp(xPos, -ShownCompassSize.x / 2f, ShownCompassSize.x / 2f);

            return new Vector2(xPos, 0); // Vị trí Y cố định trên la bàn (tâm Y)
        }

        private float CalculateScale(Waypoint waypoint)
        {
            float distance = Vector3.Distance(WaypointManager.Instance.playerTransform.position, waypoint.worldPosition);

            // Tỉ lệ scale giảm dần khi khoảng cách tăng
            // Normalized distance từ 0 (gần nhất) đến 1 (MaxRenderDistance)
            float normalizedDistance = Mathf.InverseLerp(0, MaxRenderDistance, distance);
            float scale = Mathf.Lerp(MaxScale, MinScale, normalizedDistance);

            // Đảm bảo scale nằm trong khoảng MinScale và MaxScale
            scale = Mathf.Clamp(scale, MinScale, MaxScale);
            return scale;
        }
    }
}
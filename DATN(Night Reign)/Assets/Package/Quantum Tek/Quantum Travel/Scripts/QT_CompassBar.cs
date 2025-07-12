using UnityEngine;
using System.Collections.Generic;

namespace QuantumTek.QuantumTravel
{
    public class QT_CompassBar : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform = null; // RectTransform của QT_CompassBar (khung hiển thị)
        [SerializeField] private UnityEngine.UI.RawImage image = null; // RawImage của texture la bàn cuộn

        [Header("Compass Bar Variables")]
        public Vector2 CompassSize = new Vector2(200, 25); // Kích thước của texture la bàn đầy đủ (chiều dài cuộn)
        public Vector2 ShownCompassSize = new Vector2(100, 25); // Kích thước hiển thị thực tế của la bàn trên UI

        public float MaxRenderDistance = 100f; // Khoảng cách tối đa để waypoint hiển thị trên la bàn
        public float MinScale = 0.5f;
        public float MaxScale = 1.0f;

        [Range(-1f, 1f)]
        public float compassTextureInitialOffset = 0f;

        [Header("Camera Reference")]
        public Transform playerMainCameraTransform;

        public static float SharedMapRotationAngle_ForUV = 0f; // Góc xoay của bản đồ/la bàn cho UV texture

        // Tham chiếu đến RectTransform của Player Marker trên Compass (để đảm bảo không bị đè)
        [Header("Player Marker Reference")]
        public RectTransform playerCompassMarkerRectTransform; // Kéo thả WaypointUI_PlayerCustomMarker vào đây

        private void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (image == null) image = GetComponent<UnityEngine.UI.RawImage>();
            if (playerMainCameraTransform == null)
                Debug.LogError("[QT_CompassBar] Player Main Camera Transform is not assigned!");
            if (playerCompassMarkerRectTransform == null)
                Debug.LogWarning("[QT_CompassBar] Player Compass Marker RectTransform is not assigned. Waypoints might overlay player marker.");

            // Đảm bảo kích thước của rectTransform phù hợp với ShownCompassSize (kích thước khung nhìn)
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ShownCompassSize.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ShownCompassSize.y);

            // Kích thước của RawImage (texture cuộn) lớn hơn ShownCompassSize để cho phép cuộn
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CompassSize.x);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CompassSize.y);
            image.rectTransform.anchoredPosition = Vector2.zero; // Đảm bảo RawImage ở tâm của rectTransform
        }

        private void Update()
        {
            if (playerMainCameraTransform == null) return;

            // Cập nhật UV của texture la bàn để xoay theo hướng của người chơi
            SharedMapRotationAngle_ForUV = Mathf.Repeat(-playerMainCameraTransform.eulerAngles.y, 360f);
            float uvX = (SharedMapRotationAngle_ForUV / 360f) + compassTextureInitialOffset;
            uvX = Mathf.Repeat(uvX, 1f);
            image.uvRect = new Rect(uvX, 0, 1, 1);

            if (WaypointManager.Instance == null || WaypointManager.Instance.playerTransform == null)
                return;

            // Đảm bảo player marker luôn ở giữa la bàn (0,0) và luôn active
            if (playerCompassMarkerRectTransform != null)
            {
                playerCompassMarkerRectTransform.anchoredPosition = Vector2.zero;
                if (!playerCompassMarkerRectTransform.gameObject.activeSelf)
                    playerCompassMarkerRectTransform.gameObject.SetActive(true);
            }

            UpdateWaypointUIsOnCompass();
        }

        private void UpdateWaypointUIsOnCompass()
        {
            if (WaypointManager.Instance.compassWaypointUIs == null || WaypointManager.Instance.compassWaypointsParent == null) return;

            // Kích thước thực tế của khung hiển thị la bàn UI
            float compassUIHalfWidth = rectTransform.rect.width / 2f;

            // Duyệt qua TẤT CẢ các waypoint trong từ điển của WaypointManager
            foreach (var entry in WaypointManager.Instance.compassWaypointUIs)
            {
                WaypointUI markerUI = entry.Value;
                Waypoint waypointData = entry.Value.GetWaypointData(); // Lấy dữ liệu từ WaypointUI

                if (markerUI == null || markerUI.gameObject == null)
                {
                    Debug.LogWarning($"[QT_CompassBar] compassWaypointUI for ID '{entry.Key}' is null or its GameObject is null. Skipping.");
                    continue;
                }

                float distance = Vector3.Distance(WaypointManager.Instance.playerTransform.position, waypointData.worldPosition);

                // Kiểm tra khoảng cách: chỉ hiển thị waypoint nếu nó nằm trong MaxRenderDistance
                if (distance <= MaxRenderDistance)
                {
                    Vector2 anchoredPos = CalculatePosition(waypointData);
                    float scale = CalculateScale(distance); // Scale dựa trên khoảng cách

                    // Đảm bảo waypoint được bật
                    if (!markerUI.gameObject.activeSelf) markerUI.gameObject.SetActive(true);

                    markerUI.SetMapUIPosition(anchoredPos, scale); // Gửi scale cho WaypointUI để điều chỉnh kích thước
                    markerUI.SetDistanceText(distance);
                    // Debug.Log($"[QT_CompassBar] Updating waypoint '{waypointData.id}' at position {anchoredPos}, scale {scale}. Distance: {distance:F0}m");
                }
                else
                {
                    // Ẩn waypoint nếu quá xa
                    if (markerUI.gameObject.activeSelf) markerUI.gameObject.SetActive(false);
                }
            }
        }

        private Vector2 CalculatePosition(Waypoint waypoint)
        {
            float compassDegrees = 180f; // La bàn hiển thị 180 độ (từ -90 đến 90)
            float pixelsPerDegree = ShownCompassSize.x / compassDegrees; // Số pixel cho mỗi độ trên la bàn hiển thị

            Vector3 dirToWaypoint = waypoint.worldPosition - WaypointManager.Instance.playerTransform.position;
            // Chỉ quan tâm đến hướng trên mặt phẳng XZ (bỏ qua chiều cao)
            Vector2 dir2D = new Vector2(dirToWaypoint.x, dirToWaypoint.z).normalized;
            Vector2 forward2D = new Vector2(playerMainCameraTransform.forward.x, playerMainCameraTransform.forward.z).normalized;

            // Tính góc tương đối so với hướng nhìn của người chơi (trên mặt phẳng ngang)
            float relativeAngle = Vector2.SignedAngle(forward2D, dir2D); // Góc từ -180 đến 180

            // Clamp góc vào phạm vi hiển thị của la bàn (-90 đến 90 độ)
            // Nếu waypoint nằm ngoài phạm vi này, nó sẽ được hiển thị ở rìa
            relativeAngle = Mathf.Clamp(relativeAngle, -90f, 90f);

            // Chuyển đổi góc sang vị trí X trên UI
            // Vị trí X sẽ là `relativeAngle * pixelsPerDegree`
            // Và sau đó điều chỉnh để 0 là trung tâm của ShownCompassSize.x
            float xPos = relativeAngle * pixelsPerDegree;

            // Giới hạn xPos trong nửa chiều rộng của la bàn để nó nằm trong khung hiển thị
            float halfShownWidth = ShownCompassSize.x / 2f;
            xPos = Mathf.Clamp(xPos, -halfShownWidth, halfShownWidth);

            // Vị trí Y cố định cho waypoint trên la bàn. Thường là 0 nếu la bàn là dải ngang ở giữa.
            // Điều này đảm bảo chúng không đè lên Player Marker nếu Player Marker cũng ở Y=0.
            // Nếu Player Marker của bạn có kích thước và nằm ở một vị trí khác (ví dụ: hơi cao hơn/thấp hơn),
            // bạn có thể điều chỉnh yPos ở đây để tránh va chạm.
            float yPos = 0f; // Để test, có thể thay đổi thành 5f hoặc -5f nếu vẫn bị đè

            return new Vector2(xPos, yPos);
        }

        private float CalculateScale(float distance)
        {
            // Lerp scale từ MaxScale (gần) đến MinScale (xa)
            // Khi distance = 0, t = 0, scale = MaxScale
            // Khi distance = MaxRenderDistance, t = 1, scale = MinScale
            float t = Mathf.InverseLerp(0, MaxRenderDistance, distance);
            return Mathf.Lerp(MaxScale, MinScale, t);
        }
    }
}
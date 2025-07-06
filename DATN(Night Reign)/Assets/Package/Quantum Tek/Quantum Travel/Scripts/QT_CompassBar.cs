using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuantumTek.QuantumTravel
{
    /// <summary>
    /// QT_CompassBar is used as a compass bar, showing travel direction in 3D space and any important markers.
    /// </summary>
    [AddComponentMenu("Quantum Tek/Quantum Travel/Compass Bar")]
    [DisallowMultipleComponent]
    public class QT_CompassBar : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform = null;
        [SerializeField] private RectTransform barBackground = null;
        [SerializeField] private RectTransform markersTransform = null;
        [SerializeField] private RawImage image = null;

        [Header("Object References")]
        public QT_MapObject ReferenceObject;
        public List<QT_MapObject> Objects = new List<QT_MapObject>();
        public QT_MapMarker MarkerPrefab;
        public List<QT_MapMarker> Markers { get; set; } = new List<QT_MapMarker>();

        [Header("Compass Bar Variables")]
        public Vector2 CompassSize = new Vector2(200, 25);
        public Vector2 ShownCompassSize = new Vector2(100, 25);
        public float MaxRenderDistance = 5;
        public float MarkerSize = 20;
        public float MinScale = 0.5f;
        public float MaxScale = 2f;

        // --- Bỏ biến compassRotationSpeed vì không còn xoay bằng chuột độc lập ---

        [Tooltip("Adjust this to center 'North' (or your desired default direction) on the compass texture at 0 degrees. " +
                 "If North is at the very left edge of your seamless texture, this might be 0. " +
                 "If North is at the center of the texture's visible part when SharedMapRotationAngle is 0, this might be 0.5. " +
                 "Experiment until North aligns correctly when SharedMapRotationAngle is 0.")]
        [Range(-1f, 1f)]
        public float compassTextureInitialOffset = 0f;

        // !!! THÊM THAM CHIẾU NÀY !!!
        [Header("Camera Reference")]
        public Transform playerMainCameraTransform; // Kéo Camera chính của người chơi vào đây

        // Biến static để đồng bộ góc xoay của bản đồ/la bàn UI giữa các script
        // GIỜ ĐÂY NÓ ĐỒNG BỘ VỚI GÓC XOAY Y CỦA CAMERA NGƯỜI CHƠI
        public static float SharedMapRotationAngle = 0f;

        private void Awake()
        {
            if (rectTransform == null) Debug.LogError("RectTransform is null!");
            if (barBackground == null) Debug.LogError("BarBackground is null!");
            if (markersTransform == null) Debug.LogError("MarkersTransform is null!");
            if (image == null) Debug.LogError("Image is null!");
            if (ReferenceObject == null) Debug.LogError("ReferenceObject is null! Please assign the player object.");
            if (MarkerPrefab == null) Debug.LogError("MarkerPrefab is null!");
            if (playerMainCameraTransform == null) Debug.LogError("Player Main Camera Transform is null! Compass will not rotate correctly."); // Thêm kiểm tra

            foreach (var obj in Objects)
            {
                if (obj.Data.ShowOnCompass)
                {
                    AddMarker(obj);
                }
            }

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ShownCompassSize.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ShownCompassSize.y);
            barBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CompassSize.x);
            barBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CompassSize.y);
        }

        private void Update()
        {
            if (playerMainCameraTransform == null) return;

            // Cập nhật SharedMapRotationAngle LẤY TỪ GÓC XOAY Y CỦA CAMERA NGƯỜI CHƠI
            // EulerAngles.y cho góc quay quanh trục Y của camera.
            // Mathf.Repeat(angle, 360f) để đảm bảo góc luôn trong khoảng [0, 360).
            // Dấu trừ là để khi camera quay theo chiều kim đồng hồ (góc y tăng),
            // thì la bàn sẽ dịch chuyển UV để các hướng vẫn đúng.
            SharedMapRotationAngle = Mathf.Repeat(-playerMainCameraTransform.eulerAngles.y, 360f);

            // Cập nhật UV Rect của la bàn để nó xoay
            // Tính toán offset UV dựa trên SharedMapRotationAngle (đã lấy từ camera)
            float uvX = (SharedMapRotationAngle / 360f) + compassTextureInitialOffset;
            uvX = Mathf.Repeat(uvX, 1f);

            image.uvRect = new Rect(uvX, 0, 1, 1);

            // Cập nhật vị trí và tỉ lệ của các marker
            foreach (var marker in Markers)
            {
                marker.SetPosition(CalculatePosition(marker));
                marker.SetScale(CalculateScale(marker));
            }
        }

        private Vector2 CalculatePosition(QT_MapMarker marker)
        {
            float compassDegree = CompassSize.x / 360;
            Vector2 referencePosition = ReferenceObject.Position(QT_MapType.Map3D);

            // Hướng Bắc cố định trong thế giới Unity (trục Z dương là Bắc)
            // Vector3.forward tương ứng với Vector2.up trong mặt phẳng XZ
            Vector2 worldNorth = Vector2.up;

            Vector2 directionToMarker = marker.Object.Position(QT_MapType.Map3D) - referencePosition;

            float absoluteAngleToMarker = Vector2.SignedAngle(worldNorth, directionToMarker);

            // Bù trừ góc xoay của la bàn (SharedMapRotationAngle) để marker hiển thị đúng vị trí tương đối.
            // SharedMapRotationAngle ở đây đã là góc dịch chuyển của texture la bàn,
            // nên marker chỉ cần góc tuyệt đối của nó.
            // KHÔNG CẦN TRỪ SharedMapRotationAngle NỮA VÌ SharedMapRotationAngle ĐÃ TRỞ THÀNH GÓC CỦA CAMERA
            // markerDisplayAngle = absoluteAngleToMarker - SharedMapRotationAngle; // CŨ

            // Logic mới: La bàn đã xoay theo camera. Marker cần vị trí tương đối so với hướng camera.
            // SharedMapRotationAngle giờ đây là góc xoay của CAMERA, không phải offset UV nữa.
            // uvX = (SharedMapRotationAngle / 360f) + compassTextureInitialOffset;

            // Góc hiển thị của marker trên la bàn sẽ là góc tương đối của nó so với hướng nhìn của camera
            // Tức là: (góc tuyệt đối của marker) - (góc tuyệt đối của camera)
            // Player camera eulerAngles.y là góc theo chiều kim đồng hồ từ trục Z+ (Bắc).
            // SignedAngle trả về từ -180 đến 180.
            float playerCameraYaw = playerMainCameraTransform.eulerAngles.y;
            // Chuẩn hóa playerCameraYaw về -180 đến 180 nếu cần để tính SignedAngle
            playerCameraYaw = Mathf.Repeat(playerCameraYaw + 180f, 360f) - 180f;

            // MarkerDisplayAngle: Góc từ player camera forward đến marker.
            // (Đảm bảo marker hiển thị đúng so với tâm la bàn, tức là tâm la bàn là hướng nhìn của player)
            Vector2 playerForward2D = new Vector2(playerMainCameraTransform.forward.x, playerMainCameraTransform.forward.z).normalized;
            float markerDisplayAngle = Vector2.SignedAngle(playerForward2D, directionToMarker);


            markerDisplayAngle = Mathf.Repeat(markerDisplayAngle + 180f, 360f) - 180f; // Chuẩn hóa

            return new Vector2(compassDegree * markerDisplayAngle, 0);
        }

        private Vector2 CalculateScale(QT_MapMarker marker)
        {
            float distance = Vector2.Distance(ReferenceObject.Position(QT_MapType.Map3D), marker.Object.Position(QT_MapType.Map3D));
            float scale = 0;

            if (distance < MaxRenderDistance)
                scale = Mathf.Clamp(1 - distance / MaxRenderDistance, MinScale, MaxScale);

            return new Vector2(scale, scale);
        }

        public void AddMarker(QT_MapObject obj)
        {
            if (MarkerPrefab == null)
            {
                Debug.LogError("MarkerPrefab is null, cannot add marker for " + obj.name);
                return;
            }
            QT_MapMarker marker = Instantiate(MarkerPrefab, markersTransform);
            marker.Initialize(obj, MarkerSize);
            Markers.Add(marker);
        }
    }
}
using UnityEngine;

namespace Dat.Water
{
    public class WaterRippleEffect : MonoBehaviour
    {
        [Header("References")]
        public Transform rippleTarget;           // Kéo Ripple (trong RippleCamera prefab) vào đây
        public ParticleSystem rippleParticle;    // Kéo ParticleSystem ripple vào đây (cục hiệu ứng nước)

        [Header("Settings")]
        public LayerMask waterLayer;             // Layer mặt nước
        public float checkHeightOffset = 2f;     // Chiều cao raycast từ trên xuống
        public float emitCooldown = 0.15f;       // Thời gian giãn cách giữa các lần emit
        public float movementThreshold = 0.02f;  // Mức di chuyển tối thiểu mới emit ripple

        private Vector3 lastPosition;
        private float lastEmitTime;

        void Start()
        {
            if (rippleTarget == null || rippleParticle == null)
            {
                Debug.LogError("[WaterRippleEffect] Chưa gán đầy đủ rippleTarget hoặc rippleParticle!");
                enabled = false;
                return;
            }

            lastPosition = transform.position;
        }

        void Update()
        {
            Vector3 rayOrigin = transform.position + Vector3.up * checkHeightOffset;

            // Kiểm tra nhân vật có đứng trong vùng nước không
            if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, checkHeightOffset * 2f, waterLayer))
                return;

            // Kiểm tra tốc độ di chuyển trên mặt phẳng (XZ)
            float moved = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(lastPosition.x, 0, lastPosition.z)
            );

            if (moved > movementThreshold && Time.time - lastEmitTime >= emitCooldown)
            {
                rippleTarget.position = hit.point;
                rippleParticle.Emit(1);
                lastEmitTime = Time.time;
            }

            lastPosition = transform.position;
        }
    }
}

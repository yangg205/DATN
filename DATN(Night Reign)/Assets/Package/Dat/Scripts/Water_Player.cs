using UnityEngine;

namespace Dat.Water
{
    public class RippleEffect : MonoBehaviour
    {
        [Header("References")]
        public Transform playerTransform;
        public ParticleSystem rippleParticle;

        [Header("Settings")]
        public float checkHeightOffset = 2f;
        public LayerMask waterLayer;
        public float movementThreshold = 0.025f;
        public float emitCooldown = 0.1f;

        private Vector3 lastPlayerPosition;
        private float lastEmitTime;

        private void Start()
        {
            if (playerTransform == null)
            {
                Debug.LogError("Player Transform not assigned!");
                enabled = false;
                return;
            }
            lastPlayerPosition = playerTransform.position;
        }

        private void Update()
        {
            EmitRipplesIfMoving();
        }

        void EmitRipplesIfMoving()
        {
            // Kiểm tra nếu player đang trong vùng nước
            Vector3 checkPos = playerTransform.position + Vector3.up * checkHeightOffset;
            if (!Physics.Raycast(checkPos, Vector3.down, checkHeightOffset * 2f, waterLayer)) return;

            float velocityXZ = Vector3.Distance(
                new Vector3(playerTransform.position.x, 0, playerTransform.position.z),
                new Vector3(lastPlayerPosition.x, 0, lastPlayerPosition.z)
            );

            if (velocityXZ > movementThreshold && Time.time - lastEmitTime >= emitCooldown)
            {
                // Raycast từ vị trí chân xuống để đặt ripple đúng mặt nước
                Vector3 rayOrigin = playerTransform.position + Vector3.up * 0.5f;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1.5f, waterLayer))
                {
                    rippleParticle.transform.position = hit.point;
                    rippleParticle.Emit(1);
                    lastEmitTime = Time.time;
                }
            }

            lastPlayerPosition = playerTransform.position;
        }
    }
}

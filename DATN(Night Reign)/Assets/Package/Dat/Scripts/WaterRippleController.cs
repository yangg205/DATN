using UnityEngine;

public class WaterRippleController : MonoBehaviour
{
    public Transform target; // Player mới
    public ParticleSystem rippleEffect;
    public LayerMask waterLayer;
    public float checkDistance = 1.0f;

    private void Update()
    {
        // Luôn theo dõi vị trí player
        if (target != null)
        {
            transform.position = target.position;

            // Kiểm tra nếu player đang đứng trên mặt nước
            RaycastHit hit;
            if (Physics.Raycast(target.position + Vector3.up * 0.5f, Vector3.down, out hit, checkDistance, waterLayer))
            {
                if (!rippleEffect.isPlaying)
                    rippleEffect.Play();
            }
            else
            {
                if (rippleEffect.isPlaying)
                    rippleEffect.Stop();
            }
        }
    }
}

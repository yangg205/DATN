using UnityEngine;

public class RippleEmitter : MonoBehaviour
{
    public ParticleSystem ripple;
    public LayerMask waterMask;
    public float checkDistance = 0.5f;

    void FixedUpdate()
    {
        // Raycast từ chân xuống
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, checkDistance, waterMask))
        {
            ripple.transform.position = hit.point; // Di chuyển ripple tới điểm chạm nước
            ripple.Emit(1); // Phát 1 ripple
        }
    }
}

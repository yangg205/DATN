using UnityEngine;

public class FaceCameraFollow : MonoBehaviour
{
    public Transform followTarget; // vị trí camera mount
    public Transform lookTarget;   // vị trí đầu player
    public float followSpeed = 5f;

    void LateUpdate()
    {
        if (followTarget == null || lookTarget == null) return;

        // Di chuyển mượt đến điểm mount
        transform.position = Vector3.Lerp(
            transform.position,
            followTarget.position,
            Time.deltaTime * followSpeed
        );

        // Nhìn thẳng vào đầu player
        transform.LookAt(lookTarget.position);
    }
}

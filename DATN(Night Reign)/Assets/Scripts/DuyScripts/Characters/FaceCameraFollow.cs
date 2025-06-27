using UnityEngine;

public class FaceCameraFollow : MonoBehaviour
{
    public Transform followTarget; // Gán CameraMountPoint vào
    public float followSpeed = 5f;

    void LateUpdate()
    {
        if (followTarget == null) return;

        // Camera di chuyển đến đúng điểm
        transform.position = Vector3.Lerp(transform.position, followTarget.position, Time.deltaTime * followSpeed);

        // Camera nhìn về hướng nhân vật (hoặc gốc đầu)
        transform.LookAt(followTarget.parent.position + Vector3.up * 0.05f);
    }
}

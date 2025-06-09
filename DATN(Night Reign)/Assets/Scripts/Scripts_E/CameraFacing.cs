using UnityEngine;

public class CameraFacing : MonoBehaviour
{
    public Transform playerTransform; // Gán Player trong Inspector hoặc tìm tự động
    public float visibleDistance = 5f; // Khoảng cách tối đa để hiện canvas
    private Canvas canvas; // Canvas component

    void Start()
    {
        canvas = GetComponent<Canvas>();
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
    }

    void LateUpdate()
    {
        // Luôn quay mặt về phía camera
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0);

        // Kiểm tra khoảng cách với player
        if (playerTransform != null && canvas != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            canvas.enabled = distance <= visibleDistance;
        }
    }
}

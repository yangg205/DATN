using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target; // Nhân vật
    public float rotateSpeed = 5f;

    private Vector2 rotation = Vector2.zero;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        rotation.x += Input.GetAxis("Mouse X") * rotateSpeed;
        rotation.y -= Input.GetAxis("Mouse Y") * rotateSpeed;
        rotation.y = Mathf.Clamp(rotation.y, -20f, 70f);

        Quaternion xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        Quaternion yQuat = Quaternion.AngleAxis(rotation.y, Vector3.right);

        transform.position = target.position;
        transform.rotation = xQuat * yQuat;
    }
}

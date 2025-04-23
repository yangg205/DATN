using Fusion;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    public Transform cameraTransform;
    public Transform cameraHolder; // Một empty object giữ camera trong Player
    public float mouseSensitivity = 2.0f;
    private float verticalRotation = 0f;

    public override void Spawned()
    {
        if (!Object.HasInputAuthority)
        {
            cameraTransform.gameObject.SetActive(false);
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!Object.HasInputAuthority) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Xoay Player theo trục Y (ngang)
        transform.Rotate(Vector3.up * mouseX);

        // Xoay camera theo trục X (dọc)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
}

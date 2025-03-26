using Fusion;
using Unity.Cinemachine;
using UnityEngine;

public class CamFPS : NetworkBehaviour
{
    public CinemachineCamera FPSCam;
    public float mouseSensitivity = 300f;

    private float verticalRotation = 0f; // Xoay dọc (pitch)
    private Transform playerBody;       // Tham chiếu đến playerBody để xoay ngang
    private float mouseXInput;          // Lưu input chuột để sử dụng trong FixedUpdateNetwork
    private Transform gun;              // Súng gắn vào Camera

    private bool isChatActive = false;  // Biến theo dõi trạng thái chat

    public void Setup(Transform playerBody, Transform headTransform, Transform gun)
    {
        if (!Object.HasInputAuthority) return; // Kiểm tra quyền chỉ một lần trong Setup

        this.playerBody = playerBody;
        this.gun = gun;

        // Gắn Camera vào đầu (Head)
        FPSCam.transform.SetParent(headTransform);
        FPSCam.transform.localPosition = Vector3.zero;
        FPSCam.transform.localRotation = Quaternion.identity;
        FPSCam.Follow = null;
        FPSCam.LookAt = null;
        FPSCam.Priority = 10;

        // Gắn súng vào Camera
        if (gun != null)
        {
            gun.SetParent(FPSCam.transform); // Gắn trực tiếp vào camera
            gun.localPosition = new Vector3(0.2f, -0.1f, 0.3f); // Điều chỉnh vị trí súng
            gun.localRotation = Quaternion.identity; // Đặt góc quay ban đầu
        }

        LockCursor(); // Khóa chuột khi bắt đầu
    }

    void Update()
    {
        if (playerBody == null) return;

        // Chỉ nhận input từ chuột nếu chat đang tắt
        if (!isChatActive)
        {
            // Lấy input từ chuột
            mouseXInput = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Xoay dọc: Camera (pitch)
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -70f, 70f);
            FPSCam.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (playerBody == null) return;

        // Xoay ngang: Xoay playerBody
        if (!isChatActive)
        {
            playerBody.Rotate(Vector3.up * mouseXInput);
        }
    }

    public void SetChatState(bool isActive)
    {
        isChatActive = isActive;

        // Chỉ khóa hoặc mở chuột khi trạng thái chat thay đổi
        if (isChatActive)
        {
            UnlockCursor(); // Mở chuột khi chat bật
        }
        else
        {
            LockCursor(); // Khóa chuột khi chat tắt
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // Khóa chuột ở giữa màn hình
        Cursor.visible = false; // Ẩn con trỏ
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; // Mở khóa chuột
        Cursor.visible = true; // Hiển thị con trỏ
    }
}

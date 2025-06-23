using UnityEngine;
using UnityEngine.UI;

public class SmallMinimapController : MonoBehaviour
{
    public Camera minimapCamera; // Camera của minimap nhỏ
    public RectTransform minimapUIRectTransform; // RectTransform của SmallMinimap_MapDisplay_Raw (hoặc SmallMinimap_CircularMask nếu bạn muốn xoay cả mask)
    public RectTransform playerIconRectTransform; // RectTransform của SmallMinimap_PlayerIcon

    public Transform playerTransform; // Transform của người chơi
    public Transform playerCameraTransform; // Camera của người chơi

    public float cameraHeight = 50f; // Độ cao camera minimap
    public float zoomOrthographicSize = 37.5f; // Mức zoom

    public RotationMode rotationMode = RotationMode.RotateMapInsteadOfPlayerIcon;

    public enum RotationMode
    {
        RotateWithPlayerIcon,
        RotateMapInsteadOfPlayerIcon
    }

    void Start()
    {
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = zoomOrthographicSize;
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null || playerCameraTransform == null || minimapCamera == null)
        {
            Debug.LogWarning("Tham chiếu cho Small Minimap Controller chưa được gán đầy đủ!");
            return;
        }

        // Di chuyển Camera Minimap theo người chơi
        Vector3 newCameraPos = playerTransform.position;
        newCameraPos.y = cameraHeight;
        minimapCamera.transform.position = newCameraPos;
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Luôn nhìn thẳng xuống

        // Xử lý xoay minimap
        HandleRotation();
    }

    void HandleRotation()
    {
        switch (rotationMode)
        {
            case RotationMode.RotateWithPlayerIcon:
                // Minimap không xoay, icon xoay theo hướng người chơi
                minimapUIRectTransform.localEulerAngles = Vector3.zero;

                Vector3 playerForward = playerCameraTransform.forward;
                playerForward.y = 0;
                playerForward.Normalize();

                if (playerForward != Vector3.zero)
                {
                    float angle = Vector3.SignedAngle(Vector3.forward, playerForward, Vector3.up);
                    playerIconRectTransform.localEulerAngles = new Vector3(0, 0, -angle);
                }
                break;

            case RotationMode.RotateMapInsteadOfPlayerIcon:
                // Icon không xoay, minimap xoay ngược hướng người chơi
                playerIconRectTransform.localEulerAngles = Vector3.zero;

                float playerRotationY = playerCameraTransform.eulerAngles.y;
                minimapUIRectTransform.localEulerAngles = new Vector3(0, 0, playerRotationY);
                break;
        }
    }
}
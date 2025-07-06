using UnityEngine;
using UnityEngine.UI;

public class SmallMinimapController : MonoBehaviour
{
    public Camera minimapCamera;
    public RectTransform minimapUIRectTransform;
    public RectTransform playerIconRectTransform;

    public Transform playerTransform;
    public Transform playerMainCameraTransform; // Kéo Camera chính của người chơi vào đây

    public float cameraHeight = 50f;
    public float zoomOrthographicSize = 37.5f;

    void Start()
    {
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = zoomOrthographicSize;
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null || minimapCamera == null || minimapUIRectTransform == null || playerIconRectTransform == null || playerMainCameraTransform == null)
        {
            Debug.LogWarning("Tham chiếu cho Small Minimap Controller chưa được gán đầy đủ! Vui lòng gán Player Transform, Player Main Camera Transform, Minimap Camera, Minimap UI Rect Transform và Player Icon Rect Transform trong Inspector.");
            return;
        }

        // 1. Di chuyển Camera Minimap theo người chơi
        Vector3 newCameraPos = playerTransform.position;
        newCameraPos.y = cameraHeight;
        minimapCamera.transform.position = newCameraPos;
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Luôn nhìn thẳng xuống

        // 2. Xoay minimap UI theo góc xoay của CAMERA NGƯỜI CHƠI
        // Lấy góc Y của camera (quay quanh trục Y), đây là góc tuyệt đối của hướng nhìn.
        // Dấu trừ để đảm bảo khi camera quay chiều kim đồng hồ (góc Y tăng),
        // bản đồ UI quay ngược chiều kim đồng hồ để giữ cho "phía trước" của bản đồ là hướng camera đang nhìn.
        minimapUIRectTransform.localEulerAngles = new Vector3(0, 0, playerMainCameraTransform.eulerAngles.y); // Không có dấu trừ nếu bạn muốn bản đồ xoay cùng chiều camera

        // Đảo ngược dấu của góc Y của camera để bản đồ xoay ngược lại với camera,
        // làm cho hướng nhìn của camera luôn "hướng lên" trên minimap.
        // Unity UI Z-rotation: dương là ngược chiều kim đồng hồ, âm là chiều kim đồng hồ.
        // Camera Y-rotation: dương là chiều kim đồng hồ (nếu nhìn từ trên xuống).
        // Vậy, để bản đồ xoay ngược lại với camera, ta dùng góc của camera.
        minimapUIRectTransform.localEulerAngles = new Vector3(0, 0, -playerMainCameraTransform.eulerAngles.y);

        // 3. Icon người chơi trên minimap: LUÔN CHỈ THẲNG LÊN TRÊN (MŨI TÊN HƯỚNG LÊN TRÊN)
        // Vì bản đồ đã được xoay để "phía trên" của nó là hướng nhìn của người chơi,
        // icon người chơi chỉ cần đứng yên và hướng lên trên là đủ.
        playerIconRectTransform.localEulerAngles = Vector3.zero;
    }
}
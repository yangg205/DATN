using UnityEngine;
using UnityEngine.UI;
using QuantumTek.QuantumTravel;
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
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // 2. Xoay minimap UI theo góc xoay chuột chung
        minimapUIRectTransform.localEulerAngles = new Vector3(0, 0, -QT_CompassBar.SharedMapRotationAngle);

        // 3. Xoay icon người chơi trên minimap để chỉ đúng hướng nhìn của player trên BẢN ĐỒ ĐÃ XOAY
        Vector3 playerForwardWorld = new Vector3(playerMainCameraTransform.forward.x, 0, playerMainCameraTransform.forward.z).normalized;
        float playerAbsoluteAngleFromNorth = Vector3.SignedAngle(Vector3.forward, playerForwardWorld, Vector3.up);

        float finalIconRotationZ_UI = playerAbsoluteAngleFromNorth + QT_CompassBar.SharedMapRotationAngle;

        playerIconRectTransform.localEulerAngles = new Vector3(0, 0, -finalIconRotationZ_UI);
    }
}
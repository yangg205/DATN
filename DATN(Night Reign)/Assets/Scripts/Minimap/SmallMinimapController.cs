using UnityEngine;
using UnityEngine.UI;
// using QuantumTek.QuantumTravel; // Không cần thiết nếu chỉ dùng cho SmallMinimap

public class SmallMinimapController : MonoBehaviour
{
    public Camera minimapCamera;
    public RectTransform minimapUIRectTransform;
    public RectTransform playerIconRectTransform;

    public Transform playerTransform;
    public Transform playerMainCameraTransform; // Kéo Camera chính của người chơi vào đây

    [Header("Minimap Waypoint UI")]
    public RectTransform minimapWaypointsParent; // Kéo Minimap_WaypointsParent vào đây

    public float cameraHeight = 50f;
    public float zoomOrthographicSize = 37.5f;
    public float minimapScaleFactor = 1.0f; // Tỉ lệ giữa đơn vị thế giới và pixel trên minimap (VD: 1 mét = 1 pixel)
                                            // Tùy chỉnh để các marker hiển thị đúng khoảng cách trên UI

    void Start()
    {
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = zoomOrthographicSize;
        }

        if (minimapWaypointsParent == null) Debug.LogError("Minimap Waypoints Parent is not assigned in SmallMinimapController!");
    }

    void LateUpdate()
    {
        if (playerTransform == null || minimapCamera == null || minimapUIRectTransform == null || playerIconRectTransform == null || playerMainCameraTransform == null || minimapWaypointsParent == null)
        {
            Debug.LogWarning("Tham chiếu cho Small Minimap Controller chưa được gán đầy đủ! Vui lòng gán Player Transform, Player Main Camera Transform, Minimap Camera, Minimap UI Rect Transform, Player Icon Rect Transform và Minimap Waypoints Parent trong Inspector.");
            return;
        }

        // 1. Di chuyển Camera Minimap theo người chơi
        Vector3 newCameraPos = playerTransform.position;
        newCameraPos.y = cameraHeight;
        minimapCamera.transform.position = newCameraPos;
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // 2. Xoay minimap UI theo góc xoay của CAMERA NGƯỜI CHƠI
        // Đảm bảo playerMainCameraTransform.eulerAngles.y là góc quay chuẩn từ Bắc (trục Z+).
        minimapUIRectTransform.localEulerAngles = new Vector3(0, 0, -playerMainCameraTransform.eulerAngles.y);

        // 3. Icon người chơi trên minimap: LUÔN CHỈ THẲNG LÊN TRÊN (MŨI TÊN HƯỚNG LÊN TRÊN)
        // Vì bản đồ đã được xoay để "phía trên" của nó là hướng nhìn của người chơi,
        // icon người chơi chỉ cần giữ nguyên góc xoay mặc định (0,0,0) là đủ.
        playerIconRectTransform.localEulerAngles = Vector3.zero;


        // 4. Cập nhật vị trí các Waypoint UI trên minimap
        // Lấy các UI Waypoint từ WaypointManager
        if (WaypointManager.Instance != null)
        {
            foreach (var entry in WaypointManager.Instance.activeWaypoints) // Truy cập public dictionary
            {
                Waypoint waypoint = entry.Value;
                if (WaypointManager.Instance.minimapWaypointUIs.TryGetValue(waypoint.id, out WaypointUI markerUI))
                {
                    Vector3 directionToWaypoint = waypoint.worldPosition - playerTransform.position;
                    // Chuyển đổi vị trí thế giới sang vị trí trên UI Minimap
                    // Sử dụng vector2 cho mặt phẳng XZ
                    Vector2 waypointPositionOnMap = new Vector2(directionToWaypoint.x, directionToWaypoint.z);

                    // Xoay vị trí waypoint ngược lại với góc xoay của map UI để nó đứng yên tương đối trên UI
                    // Angle của minimapUIRectTransform là -playerCamera.eulerAngles.y
                    // Để xoay vector point ngược lại, ta xoay nó với +playerCamera.eulerAngles.y
                    float angle = playerMainCameraTransform.eulerAngles.y; // Góc xoay của camera
                    float radians = angle * Mathf.Deg2Rad;

                    float rotatedX = waypointPositionOnMap.x * Mathf.Cos(radians) - waypointPositionOnMap.y * Mathf.Sin(radians);
                    float rotatedY = waypointPositionOnMap.x * Mathf.Sin(radians) + waypointPositionOnMap.y * Mathf.Cos(radians);

                    Vector2 finalUIPosition = new Vector2(rotatedX, rotatedY) * minimapScaleFactor;

                    // Giới hạn marker trong phạm vi minimap
                    float mapRadius = minimapUIRectTransform.rect.width / 2f;
                    if (finalUIPosition.magnitude > mapRadius)
                    {
                        finalUIPosition = finalUIPosition.normalized * mapRadius;
                    }

                    markerUI.gameObject.SetActive(true); // Luôn hiển thị nếu trong phạm vi
                    markerUI.GetComponent<RectTransform>().anchoredPosition = finalUIPosition;

                    // Icon của marker trên minimap thường không xoay, giữ thẳng đứng.
                    markerUI.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
                }
            }
        }
    }
}
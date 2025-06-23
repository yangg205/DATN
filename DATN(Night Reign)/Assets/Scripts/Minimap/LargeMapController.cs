using UnityEngine;
using UnityEngine.UI; // Cần thiết cho RectTransform, RawImage

public class LargeMapController : MonoBehaviour
{
    public GameObject largeMapPanel; // Panel chứa bản đồ lớn (LargeMap_Panel)
    public Camera largeMapCamera; // Camera cho bản đồ lớn (LargeMapCamera)
    public RectTransform playerIconLargeMapRectTransform; // Player_LargeMapIcon

    public Transform playerTransform; // Transform của người chơi

    // Kích thước thực tế của Terrain của bạn (150x150)
    public float terrainWidth = 150f;
    public float terrainLength = 150f;
    // Vị trí gốc (min X, min Z) của Terrain trong thế giới.
    // Nếu Terrain của bạn bắt đầu từ (0,0,0) và kéo dài đến (150,Y,150), thì terrainMinX = 0, terrainMinZ = 0
    // Nếu Terrain có tâm là (75,Y,75), thì terrainMinX = 0, terrainMinZ = 0
    public float terrainMinX = 0f;
    public float terrainMinZ = 0f;

    // Tham chiếu đến Raw Image hiển thị bản đồ lớn
    public RawImage largeMapDisplayRaw;

    void Start()
    {
        // Ẩn bản đồ lớn khi bắt đầu
        if (largeMapPanel != null)
        {
            largeMapPanel.SetActive(false);
            if (largeMapCamera != null) largeMapCamera.enabled = false;
        }
    }

    void Update()
    {
        // Xử lý bật/tắt bản đồ lớn khi nhấn 'M'
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (largeMapPanel != null)
            {
                bool isActive = !largeMapPanel.activeSelf;
                largeMapPanel.SetActive(isActive);
                if (largeMapCamera != null) largeMapCamera.enabled = isActive;

                // Cập nhật vị trí icon ngay khi bật bản đồ
                if (isActive)
                {
                    UpdatePlayerIconPosition();
                }
            }
        }
    }

    void LateUpdate()
    {
        // Cập nhật vị trí biểu tượng người chơi trên bản đồ lớn nếu nó đang hiển thị
        if (largeMapPanel != null && largeMapPanel.activeSelf)
        {
            UpdatePlayerIconPosition();
        }
    }

    void UpdatePlayerIconPosition()
    {
        if (playerTransform == null || playerIconLargeMapRectTransform == null || largeMapDisplayRaw == null)
        {
            Debug.LogWarning("Tham chiếu cho Large Map Controller chưa đầy đủ để cập nhật icon người chơi.");
            return;
        }

        // Lấy kích thước của Raw Image bản đồ lớn trên UI
        Rect largeMapRect = largeMapDisplayRaw.rectTransform.rect;

        // Tính toán vị trí tương đối của người chơi trên Terrain (từ 0 đến 1)
        // Giả sử Terrain bắt đầu từ terrainMinX, terrainMinZ
        float normalizedX = (playerTransform.position.x - terrainMinX) / terrainWidth;
        float normalizedY = (playerTransform.position.z - terrainMinZ) / terrainLength;
        // Lưu ý: Trục Z của thế giới 3D thường ánh xạ sang trục Y của UI 2D

        // Chuyển đổi từ tọa độ normalized (0-1) sang tọa độ pixel UI của Raw Image
        // Gốc của RectTransform là trung tâm của RawImage
        float uiX = (normalizedX - 0.5f) * largeMapRect.width;
        float uiY = (normalizedY - 0.5f) * largeMapRect.height;

        playerIconLargeMapRectTransform.anchoredPosition = new Vector2(uiX, uiY);

        // Icon người chơi trên bản đồ lớn thường không xoay
        playerIconLargeMapRectTransform.localEulerAngles = Vector3.zero;
    }
}
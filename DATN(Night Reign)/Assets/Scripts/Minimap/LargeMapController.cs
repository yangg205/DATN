using UnityEngine;
using UnityEngine.UI; // Cần thiết cho RectTransform, RawImage

public class LargeMapController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject largeMapPanel; // Panel chứa bản đồ lớn (LargeMap_Panel)
    public RawImage largeMapDisplayRaw; // Tham chiếu đến Raw Image hiển thị bản đồ lớn
    public RectTransform playerIconLargeMapRectTransform; // Player_LargeMapIcon

    [Header("World References")]
    public Camera largeMapCamera; // Camera cho bản đồ lớn (LargeMapCamera)
    public Transform playerTransform; // Transform của người chơi

    [Tooltip("Terrain GameObject if you want to automatically get its size and position.")]
    public Terrain activeTerrain; // Kéo Terrain của bạn vào đây nếu có

    [Header("Terrain Dimensions (Manual if no Terrain GameObject assigned)")]
    // Kích thước thực tế của Terrain của bạn (ví dụ: 150x150)
    public float terrainWidth = 150f;
    public float terrainLength = 150f;
    // Vị trí gốc (min X, min Z) của Terrain trong thế giới.
    // Nếu Terrain của bạn bắt đầu từ (0,0,0) và kéo dài đến (150,Y,150), thì terrainMinX = 0, terrainMinZ = 0
    // Nếu Terrain có tâm là (75,Y,75), thì terrainMinX = 0, terrainMinZ = 0
    public float terrainMinX = 0f;
    public float terrainMinZ = 0f;

    void Awake()
    {
        // Tự động lấy thông tin Terrain nếu được gán
        if (activeTerrain != null)
        {
            terrainWidth = activeTerrain.terrainData.size.x;
            terrainLength = activeTerrain.terrainData.size.z;
            terrainMinX = activeTerrain.transform.position.x;
            terrainMinZ = activeTerrain.transform.position.z;
        }
        else
        {
            Debug.LogWarning("Active Terrain is not assigned. Please manually set Terrain Dimensions (Width, Length, Min X, Min Z) in LargeMapController.");
        }
    }

    void Start()
    {
        // Kiểm tra các tham chiếu cần thiết
        if (largeMapPanel == null) Debug.LogError("LargeMapPanel is not assigned!");
        if (largeMapCamera == null) Debug.LogError("LargeMapCamera is not assigned!");
        if (largeMapDisplayRaw == null) Debug.LogError("LargeMapDisplayRaw is not assigned!");
        if (playerIconLargeMapRectTransform == null) Debug.LogError("PlayerIconLargeMapRectTransform is not assigned!");
        if (playerTransform == null) Debug.LogError("PlayerTransform is not assigned!");

        // Ẩn bản đồ lớn khi bắt đầu
        if (largeMapPanel != null)
        {
            largeMapPanel.SetActive(false);
            // Vô hiệu hóa camera để nó không render khi bản đồ tắt
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
                if (largeMapCamera != null) largeMapCamera.enabled = isActive; // Bật/tắt camera cùng với panel

                // Cập nhật vị trí icon ngay khi bật bản đồ để tránh hiển thị sai vị trí ban đầu
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
        // Dùng LateUpdate để đảm bảo vị trí người chơi đã được cập nhật sau tất cả các chuyển động khác
        if (largeMapPanel != null && largeMapPanel.activeSelf)
        {
            UpdatePlayerIconPosition();
        }
    }

    void UpdatePlayerIconPosition()
    {
        // Đảm bảo tất cả các tham chiếu cần thiết không bị null
        if (playerTransform == null || playerIconLargeMapRectTransform == null || largeMapDisplayRaw == null || terrainWidth <= 0 || terrainLength <= 0)
        {
            // Debug.LogWarning("Tham chiếu hoặc kích thước Terrain chưa đủ để cập nhật icon người chơi trên bản đồ lớn.");
            return; // Thoát nếu thiếu dữ liệu
        }

        // Lấy kích thước của Raw Image bản đồ lớn trên UI
        // Rect của RectTransform cung cấp kích thước cục bộ của phần tử UI
        Rect largeMapDisplayRect = largeMapDisplayRaw.rectTransform.rect;

        // Tính toán vị trí tương đối của người chơi trên Terrain (từ 0 đến 1)
        // normalizedX: Vị trí của người chơi trên trục X, chuẩn hóa từ 0 đến 1 trên chiều rộng của terrain.
        // (playerTransform.position.x - terrainMinX) là khoảng cách từ gốc terrain đến player trên trục X.
        float normalizedX = (playerTransform.position.x - terrainMinX) / terrainWidth;

        // normalizedY: Vị trí của người chơi trên trục Z của thế giới, ánh xạ sang trục Y của UI, chuẩn hóa từ 0 đến 1 trên chiều dài của terrain.
        // (playerTransform.position.z - terrainMinZ) là khoảng cách từ gốc terrain đến player trên trục Z.
        float normalizedY = (playerTransform.position.z - terrainMinZ) / terrainLength;

        // Chuyển đổi từ tọa độ normalized (0-1) sang tọa độ pixel UI của Raw Image
        // RectTransform của RawImage thường có gốc là trung tâm của nó.
        // Do đó, normalized 0.5f sẽ là 0 pixel, 0f sẽ là -Width/2, 1f sẽ là +Width/2.
        float uiX = (normalizedX - 0.5f) * largeMapDisplayRect.width;
        float uiY = (normalizedY - 0.5f) * largeMapDisplayRect.height;

        // Đặt vị trí anchoredPosition của icon người chơi trên RectTransform của nó
        playerIconLargeMapRectTransform.anchoredPosition = new Vector2(uiX, uiY);

        // Icon người chơi trên bản đồ lớn thường không xoay theo hướng nhìn của player,
        // vì bản đồ lớn thường là bản đồ cố định hướng Bắc.
        // Nếu bạn muốn icon xoay để luôn chỉ hướng Bắc trên bản đồ, bạn có thể để nó là Vector3.zero.
        // Nếu bạn muốn nó xoay theo hướng nhìn của người chơi trên bản đồ cố định, logic phức tạp hơn một chút
        // và sẽ liên quan đến playerMainCameraTransform.eulerAngles.y
        playerIconLargeMapRectTransform.localEulerAngles = Vector3.zero; // Giữ icon không xoay
    }
}
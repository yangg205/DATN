using UnityEngine;
using UnityEngine.UI;
using TMPro; // Nếu bạn dùng TextMeshPro

public class WaypointUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI distanceText; // Để hiển thị khoảng cách

    private Waypoint myWaypoint;
    private Transform playerTransform; // Tham chiếu đến người chơi để tính khoảng cách

    public void Initialize(Waypoint waypoint, Transform player)
    {
        myWaypoint = waypoint;
        playerTransform = player;

        if (iconImage == null) Debug.LogError("Icon Image is not assigned in WaypointUI for " + gameObject.name);
        if (distanceText == null) Debug.LogError("Distance Text is not assigned in WaypointUI for " + gameObject.name);

        if (iconImage != null && waypoint.uiIcon != null)
        {
            iconImage.sprite = waypoint.uiIcon;
            iconImage.enabled = true; // Đảm bảo icon hiển thị
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false; // Ẩn icon nếu không có sprite
        }
        UpdateUI(); // Cập nhật ngay lần đầu
    }

    void Update()
    {
        if (myWaypoint == null || playerTransform == null)
        {
            Destroy(gameObject); // Tự hủy nếu waypoint hoặc player bị mất
            return;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        // Cập nhật khoảng cách
        if (distanceText != null)
        {
            float distance = Vector3.Distance(playerTransform.position, myWaypoint.worldPosition);
            distanceText.text = $"{Mathf.RoundToInt(distance)}m";
        }

        // --- Logic xoay và định vị UI cho Minimap và La Bàn sẽ nằm trong các controller tương ứng ---
        // Script này chỉ quản lý icon và text cho riêng nó.
        // Vị trí và xoay của RectTransform này sẽ được SmallMinimapController và QT_CompassBar xử lý.
    }

    public Waypoint GetWaypoint()
    {
        return myWaypoint;
    }

    public Image GetIconImage()
    {
        return iconImage;
    }
}
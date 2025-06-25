using UnityEngine;

// Đây là script để điều khiển việc bật/tắt một đối tượng UI, ví dụ như panel bản đồ.
public class MinimapController : MonoBehaviour
{
    // Tạo một biến để bạn có thể kéo và thả đối tượng Panel bản đồ vào trong Unity Editor.
    // 'public' để biến này hiển thị trên cửa sổ Inspector.
    public GameObject minimapPanel;

    // Hàm này sẽ được gọi khi bạn bấm vào nút (Button).
    // Nó sẽ kiểm tra xem panel đang bật hay tắt và làm điều ngược lại.
    // Nếu đang bật, nó sẽ tắt. Nếu đang tắt, nó sẽ bật.
    public void ToggleMinimapPanel()
    {
        // Kiểm tra xem minimapPanel đã được gán trong Inspector chưa để tránh lỗi.
        if (minimapPanel != null)
        {
            // Lấy trạng thái active hiện tại của panel.
            bool isActive = minimapPanel.activeSelf;

            // Đặt trạng thái active của panel thành giá trị ngược lại.
            // !isActive có nghĩa là "not active" (phủ định của isActive).
            minimapPanel.SetActive(!isActive);
        }
        else
        {
            // In ra một cảnh báo nếu bạn quên gán panel.
            Debug.LogWarning("Minimap Panel chưa được gán vào script MinimapController.");
        }
    }

    // Bạn cũng có thể tạo một hàm chỉ để tắt panel nếu muốn.
    public void CloseMinimapPanel()
    {
        if (minimapPanel != null)
        {
            minimapPanel.SetActive(false); // Luôn luôn đặt thành false (tắt).
        }
    }
}
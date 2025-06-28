using UnityEngine;
using UnityEngine.UI; // Thêm dòng này để làm việc với UI
using System.Collections.Generic; // Để sử dụng List

public class PanelToggler : MonoBehaviour
{
    // Kéo thả tất cả các Panel bạn có vào đây trong Inspector
    // Đảm bảo thứ tự trong List này khớp với thứ tự của Button khi bạn gán hàm OnClick
    public List<GameObject> panels;

    // Hàm này sẽ được gọi khi một Button bất kỳ được click
    // truyền vào index của panel mà button đó quản lý
    public void TogglePanel(int panelIndex)
    {
        // Kiểm tra xem index có hợp lệ không
        if (panelIndex >= 0 && panelIndex < panels.Count)
        {
            GameObject currentPanel = panels[panelIndex];

            // In ra thông tin button đã được nhấn
            Debug.Log($"Button {panelIndex} clicked, toggling panel: {currentPanel.name}");

            // Nếu Panel đang hiển thị, ẩn nó đi
            if (currentPanel.activeSelf)
            {
                currentPanel.SetActive(false);
            }
            else
            {
                // Ẩn tất cả các Panel khác trước khi hiển thị Panel hiện tại
                HideAllPanels();
                currentPanel.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Panel index out of range: " + panelIndex);
        }
    }

    // Hàm này để ẩn tất cả các Panel. 
    // Hữu ích khi bạn muốn đảm bảo chỉ có một Panel được hiển thị tại một thời điểm.
    private void HideAllPanels()
    {
        foreach (GameObject panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }

    // Tùy chọn: Khởi tạo tất cả các panel ở trạng thái ẩn khi game bắt đầu
    void Start()
    {
        HideAllPanels();
    }
}
using UnityEngine;

public class ShowChiSoPanel : MonoBehaviour
{
    public GameObject panelChiSo;        // Panel cần hiện
    public GameObject[] buttonsToHide;   // Các button cần ẩn

    public void ShowPanel()
    {
        // Ẩn các button
        foreach (var btn in buttonsToHide)
        {
            if (btn != null) btn.SetActive(false);
        }

        // Hiện panel
        panelChiSo.SetActive(true);

        // Đưa panel lên trên cùng
        panelChiSo.transform.SetAsLastSibling();
    }
}

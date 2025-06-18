using UnityEngine;

public class Uicskn2 : MonoBehaviour
{
    public GameObject panelChiSo;
    public GameObject panelKiNang;

    // Hàm gọi khi bấm nút Chỉ số
    public void ShowChiSoPanel()
    {
        panelChiSo.SetActive(true);
        panelKiNang.SetActive(false);
        panelChiSo.transform.SetAsLastSibling();
    }

    // Hàm gọi khi bấm nút Kỹ năng
    public void ShowKiNangPanel()
    {
        panelChiSo.SetActive(false);
        panelKiNang.SetActive(true);
        panelKiNang.transform.SetAsLastSibling();
    }
}

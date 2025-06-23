using UnityEngine;
using UnityEngine.UI;

public class NaptienUI : MonoBehaviour
{
    public Button napTienButton;

    void Start()
    {
        napTienButton.onClick.AddListener(OnNapTienClick);
    }

    void OnNapTienClick()
    {
        Debug.Log("Nút Nạp tiền được bấm!");
        // goi api nap tien
    }
}

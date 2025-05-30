using UnityEngine;
using TMPro;

public class FPSSettingManager : MonoBehaviour
{
    public TMP_Dropdown fpsDropdown;

    void Start()
    {
        // Load lại lựa chọn cũ nếu có
        int savedFPSIndex = PlayerPrefs.GetInt("FPSIndex", 0);
        fpsDropdown.value = savedFPSIndex;
        ApplyFPSSetting(savedFPSIndex);

        // Gán callback khi người dùng chọn dropdown
        fpsDropdown.onValueChanged.AddListener(OnFPSDropdownChanged);
    }

    public void OnFPSDropdownChanged(int index)
    {
        ApplyFPSSetting(index);
        PlayerPrefs.SetInt("FPSIndex", index); // Lưu lại lựa chọn
    }

    void ApplyFPSSetting(int index)
    {
        switch (index)
        {
            case 0: // 60 FPS
                Application.targetFrameRate = 60;
                break;
            case 1: // 90 FPS
                Application.targetFrameRate = 90;
                break;
            case 2: // 120 FPS
                Application.targetFrameRate = 120;
                break;
            default:
                Application.targetFrameRate = 60;
                break;
        }
    }
}

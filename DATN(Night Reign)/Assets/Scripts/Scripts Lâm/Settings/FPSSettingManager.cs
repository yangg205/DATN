using UnityEngine;
using TMPro;

public class FPSSettingManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown fpsDropdown;
    public TextMeshProUGUI fpsDisplay; // Text hiển thị FPS thực tế

    private float deltaTime = 0.0f;

    void Start()
    {
        // Load lại lựa chọn FPS cũ
        int savedFPSIndex = PlayerPrefs.GetInt("FPSIndex", 0);
        fpsDropdown.value = savedFPSIndex;
        ApplyFPSSetting(savedFPSIndex);

        // Gán callback khi đổi dropdown
        fpsDropdown.onValueChanged.AddListener(OnFPSDropdownChanged);
    }

    void Update()
    {
        // Đo FPS thực tế
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        // Hiển thị FPS thực tế
        if (fpsDisplay != null)
            fpsDisplay.text = $"{Mathf.RoundToInt(fps)} FPS";
    }

    public void OnFPSDropdownChanged(int index)
    {
        ApplyFPSSetting(index);
        PlayerPrefs.SetInt("FPSIndex", index);
    }

    void ApplyFPSSetting(int index)
    {
        switch (index)
        {
            case 0: Application.targetFrameRate = 30; break;
            case 1: Application.targetFrameRate = 60; break;
            case 2: Application.targetFrameRate = 90; break;
            case 3: Application.targetFrameRate = 120; break;
            case 4: Application.targetFrameRate = -1; break; // Unlimited FPS
            default: Application.targetFrameRate = 60; break;
        }
    }
}

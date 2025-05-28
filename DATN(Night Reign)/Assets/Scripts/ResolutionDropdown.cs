using UnityEngine;
using TMPro;

public class ResolutionDropdown : MonoBehaviour
{
    public TMP_Dropdown fpsDropdown;
    public TMP_Dropdown resolutionDropdown;

    void Start()
    {
        // FPS setup
        int savedFPSIndex = PlayerPrefs.GetInt("FPSIndex", 0);
        fpsDropdown.value = savedFPSIndex;
        ApplyFPSSetting(savedFPSIndex);
        fpsDropdown.onValueChanged.AddListener(OnFPSDropdownChanged);

        // Resolution setup
        int savedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 1);
        resolutionDropdown.value = savedResolutionIndex;
        ApplyResolutionSetting(savedResolutionIndex);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
    }

    public void OnFPSDropdownChanged(int index)
    {
        ApplyFPSSetting(index);
        PlayerPrefs.SetInt("FPSIndex", index);
    }

    public void OnResolutionDropdownChanged(int index)
    {
        ApplyResolutionSetting(index);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    void ApplyFPSSetting(int index)
    {
        switch (index)
        {
            case 0: Application.targetFrameRate = 60; break;
            case 1: Application.targetFrameRate = 90; break;
            case 2: Application.targetFrameRate = 120; break;
            default: Application.targetFrameRate = 60; break;
        }
    }

    void ApplyResolutionSetting(int index)
    {
        switch (index)
        {
            case 0: Screen.SetResolution(1280, 720, FullScreenMode.FullScreenWindow); break;
            case 1: Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow); break;
            default: Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow); break;
        }
    }
}

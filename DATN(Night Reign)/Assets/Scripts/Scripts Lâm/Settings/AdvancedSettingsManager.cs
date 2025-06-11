using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdvancedSettingsManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fpsDropdown;
    public Toggle fullscreenToggle;
    public Button applyButton;
    public Button cancelButton;

    Resolution[] availableResolutions;
    List<Resolution> filteredResolutions = new List<Resolution>();

    void Start()
    {
        SetupResolutionDropdown();
        SetupFPSDropdown();
        LoadSettings();

        applyButton.onClick.AddListener(ApplySettings);
        cancelButton.onClick.AddListener(LoadSettings);
    }

    void SetupResolutionDropdown()
    {
        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        filteredResolutions.Clear();

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            Resolution res = availableResolutions[i];
            string option = $"{res.width} x {res.height} @ {res.refreshRate}Hz";
            if (!options.Contains(option))
            {
                options.Add(option);
                filteredResolutions.Add(res);
            }
        }

        resolutionDropdown.AddOptions(options);
    }

    void SetupFPSDropdown()
    {
        fpsDropdown.ClearOptions();
        fpsDropdown.AddOptions(new List<string> { "60 FPS", "90 FPS", "120 FPS" });
    }

    void ApplySettings()
    {
        int resIndex = resolutionDropdown.value;
        int fpsIndex = fpsDropdown.value;
        bool isFullscreen = fullscreenToggle.isOn;

        // Apply resolution and fullscreen mode
        Resolution res = filteredResolutions[resIndex];
        Screen.SetResolution(res.width, res.height, isFullscreen, res.refreshRate);

        // Apply FPS limit
        int fps = 60;
        if (fpsIndex == 1) fps = 90;
        else if (fpsIndex == 2) fps = 120;

        Application.targetFrameRate = fps;

        // Save settings
        PlayerPrefs.SetInt("ResolutionIndex", resIndex);
        PlayerPrefs.SetInt("FPSIndex", fpsIndex);
        PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
    }

    void LoadSettings()
    {
        int resIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        int fpsIndex = PlayerPrefs.GetInt("FPSIndex", 0);
        bool isFullscreen = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;

        resolutionDropdown.value = resIndex;
        fpsDropdown.value = fpsIndex;
        fullscreenToggle.isOn = isFullscreen;

        ApplySettings(); // Áp dụng ngay khi tải
    }
}

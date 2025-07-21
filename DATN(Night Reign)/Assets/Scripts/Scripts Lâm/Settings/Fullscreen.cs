using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class Fullsreen : MonoBehaviour
{
    public TMP_Dropdown fpsDropdown;
    //public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private Resolution[] availableResolutions;
    private List<Resolution> filteredResolutions = new List<Resolution>();
    private int currentResolutionIndex = 0;

    void Start()
    {
        SetupFPSDropdown();

        SetupResolutionDropdown();

        SetupFullscreenToggle();

        ApplySavedSettings();
    }

    void SetupFPSDropdown()
    {
        int savedFPSIndex = PlayerPrefs.GetInt("FPSIndex", 0);
        fpsDropdown.value = savedFPSIndex;
        ApplyFPSSetting(savedFPSIndex);
        fpsDropdown.onValueChanged.AddListener(OnFPSDropdownChanged);
    }

    void SetupResolutionDropdown()
    {
        availableResolutions = Screen.resolutions;
        //resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            Resolution res = availableResolutions[i];
            string option = res.width + " x " + res.height + " @ " + res.refreshRate + "Hz";
            if (!options.Contains(option)) // Tránh trùng
            {
                options.Add(option);
                filteredResolutions.Add(res);
                if (res.width == Screen.currentResolution.width &&
                    res.height == Screen.currentResolution.height &&
                    res.refreshRate == Screen.currentResolution.refreshRate)
                {
                    currentResolutionIndex = i;
                }
            }
        }

        //resolutionDropdown.AddOptions(options);
        //resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        //resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    void SetupFullscreenToggle()
    {
        bool isFullscreen = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;
        fullscreenToggle.isOn = isFullscreen;
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);
    }

    void OnFPSDropdownChanged(int index)
    {
        ApplyFPSSetting(index);
        PlayerPrefs.SetInt("FPSIndex", index);
    }

    void OnResolutionChanged(int index)
    {
        ApplyResolutionSetting(index);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    void OnFullscreenToggle(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
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
        Resolution res = filteredResolutions[index];
        Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn, res.refreshRate);
    }

    void ApplySavedSettings()
    {
        ApplyFPSSetting(PlayerPrefs.GetInt("FPSIndex", 0));
        ApplyResolutionSetting(PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex));
        Screen.fullScreen = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;
    }
}
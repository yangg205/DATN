using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicSlider;
    public Slider effectsSlider;
    public Toggle fullscreenToggle;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fpsDropdown;

    [Header("Mouse Sensitivity")]
    public Slider mouseSlider;
    public TextMeshProUGUI mouseValueText;

    [Header("Music & SFX Text")]
    public TextMeshProUGUI musicValueText;
    public TextMeshProUGUI effectsValueText;

    [Header("Language")]
    public TMP_Dropdown languageDropdown;

    private readonly Resolution[] customResolutions = new Resolution[]
    {
        new Resolution { width = 1280, height = 720, refreshRate = 60 },
        new Resolution { width = 1920, height = 1080, refreshRate = 60 }
    };

    // Giá trị tạm thời
    private float tempMusicVolume;
    private float tempEffectsVolume;
    private bool tempFullscreen;
    private int tempResolutionIndex;
    private int tempFPSIndex;
    private float tempMouseSensitivity;
    private int tempLanguageIndex;

    // Giá trị đã lưu
    private float savedMouseSensitivity;
    private int savedLanguageIndex;
    private float defaultMouseSensitivity = 5.0f;

    void Start()
    {
        InitResolutionDropdown();
        InitFPSDropdown();
        LoadSettings();
    }

    private void InitResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        foreach (var res in customResolutions)
            options.Add(res.width + " x " + res.height);
        resolutionDropdown.AddOptions(options);

        resolutionDropdown.onValueChanged.AddListener((index) =>
        {
            tempResolutionIndex = index;
            ApplyTempResolution();
        });
    }

    private void InitFPSDropdown()
    {
        fpsDropdown.ClearOptions();
        fpsDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "30 FPS", "60 FPS", "90 FPS", "120 FPS", "Unlimited"
        });

        fpsDropdown.onValueChanged.AddListener((index) =>
        {
            tempFPSIndex = index;
            ApplyTempFPS();
        });
    }

    public void LoadSettings(int defaultResIndex = 0)
    {
        // Đọc từ PlayerPrefs
        tempMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        tempEffectsVolume = PlayerPrefs.GetFloat("EffectsVolume", 1f);
        tempFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        tempResolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt("ResolutionIndex", defaultResIndex), 0, customResolutions.Length - 1);
        tempFPSIndex = PlayerPrefs.GetInt("FPSIndex", 1); // mặc định 60 FPS
        tempMouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultMouseSensitivity);
        tempLanguageIndex = PlayerPrefs.GetInt("LanguageIndex", 0);

        savedMouseSensitivity = tempMouseSensitivity;
        savedLanguageIndex = tempLanguageIndex;

        // Gán vào UI
        fullscreenToggle.isOn = tempFullscreen;
        fullscreenToggle.onValueChanged.AddListener((isFull) =>
        {
            tempFullscreen = isFull;
            ApplyTempResolution();
        });

        resolutionDropdown.value = tempResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        fpsDropdown.value = tempFPSIndex;
        fpsDropdown.RefreshShownValue();

        musicSlider.value = tempMusicVolume;
        musicSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.AddListener((value) =>
        {
            tempMusicVolume = value;
            UpdateMusicValueText(value);
            ApplyTempMusic();
        });
        UpdateMusicValueText(tempMusicVolume);

        effectsSlider.value = tempEffectsVolume;
        effectsSlider.onValueChanged.RemoveAllListeners();
        effectsSlider.onValueChanged.AddListener((value) =>
        {
            tempEffectsVolume = value;
            UpdateEffectsValueText(value);
            ApplyTempSFX();
        });
        UpdateEffectsValueText(tempEffectsVolume);

        mouseSlider.value = tempMouseSensitivity;
        mouseSlider.onValueChanged.RemoveAllListeners();
        mouseSlider.onValueChanged.AddListener((value) =>
        {
            tempMouseSensitivity = value;
            UpdateMouseValueText(value);
        });
        UpdateMouseValueText(tempMouseSensitivity);

        languageDropdown.value = tempLanguageIndex;
        languageDropdown.onValueChanged.RemoveAllListeners();
        languageDropdown.onValueChanged.AddListener(OnLanguageChangedTemp);

        // Áp dụng tạm
        ApplyTempResolution();
        ApplyTempFPS();
        ApplyTempMusic();
        ApplyTempSFX();
    }

    private void OnLanguageChangedTemp(int index)
    {
        tempLanguageIndex = index;
        if (LocalizationSettings.AvailableLocales.Locales.Count > index)
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }

    public void ApplySettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", tempMusicVolume);
        PlayerPrefs.SetFloat("EffectsVolume", tempEffectsVolume);
        PlayerPrefs.SetInt("Fullscreen", tempFullscreen ? 1 : 0);
        PlayerPrefs.SetInt("ResolutionIndex", tempResolutionIndex);
        PlayerPrefs.SetInt("FPSIndex", tempFPSIndex);
        PlayerPrefs.SetFloat("MouseSensitivity", tempMouseSensitivity);
        PlayerPrefs.SetInt("LanguageIndex", tempLanguageIndex);
        PlayerPrefs.Save();

        savedMouseSensitivity = tempMouseSensitivity;
        savedLanguageIndex = tempLanguageIndex;

        Debug.Log("Settings Applied");
    }

    public void CancelSettings()
    {
        LoadSettings();
        Debug.Log("Settings Cancelled");
    }

    private void ApplyTempResolution()
    {
        Resolution res = customResolutions[tempResolutionIndex];
        Screen.SetResolution(res.width, res.height, tempFullscreen);
    }

    private void ApplyTempFPS()
    {
        switch (tempFPSIndex)
        {
            case 0: Application.targetFrameRate = 30; break;
            case 1: Application.targetFrameRate = 60; break;
            case 2: Application.targetFrameRate = 90; break;
            case 3: Application.targetFrameRate = 120; break;
            case 4: Application.targetFrameRate = -1; break;
        }
    }

    private void ApplyTempMusic()
    {
        float vol = tempMusicVolume <= 0.0001f ? -80f : Mathf.Log10(tempMusicVolume) * 20;
        AudioListener.volume = tempMusicVolume; // Hoặc myMixer.SetFloat("music", vol);
    }

    private void ApplyTempSFX()
    {
        float vol = tempEffectsVolume <= 0.0001f ? -80f : Mathf.Log10(tempEffectsVolume) * 20;
        // myMixer.SetFloat("SFX", vol);
    }

    private void UpdateMouseValueText(float value)
    {
        if (mouseValueText != null)
            mouseValueText.text = value.ToString("F2");
    }

    private void UpdateMusicValueText(float value)
    {
        if (musicValueText != null)
            musicValueText.text = Mathf.RoundToInt(value * 100).ToString();
    }

    private void UpdateEffectsValueText(float value)
    {
        if (effectsValueText != null)
            effectsValueText.text = Mathf.RoundToInt(value * 100).ToString();
    }
}

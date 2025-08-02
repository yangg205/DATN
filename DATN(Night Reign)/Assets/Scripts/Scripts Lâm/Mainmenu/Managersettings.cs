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

    private float tempMusicVolume;
    private float tempEffectsVolume;
    private bool tempFullscreen;
    private int tempResolutionIndex;
    private int tempFPS;
    private float tempMouseSensitivity;
    private int tempLanguageIndex;

    private float savedMouseSensitivity;
    private int savedLanguageIndex;

    private float defaultMouseSensitivity = 5.0f;

    void Start()
    {
        InitResolutionDropdown();
        LoadSettings();
    }

    private void InitResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        foreach (var res in customResolutions)
        {
            options.Add(res.width + " x " + res.height);
        }
        resolutionDropdown.AddOptions(options);
    }

    public void LoadSettings(int defaultResIndex = 0)
    {
        // Đọc từ PlayerPrefs
        tempMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        tempEffectsVolume = PlayerPrefs.GetFloat("EffectsVolume", 1f);
        tempFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        tempResolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt("ResolutionIndex", defaultResIndex), 0, customResolutions.Length - 1);
        tempFPS = PlayerPrefs.GetInt("TargetFPS", 60);
        tempMouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultMouseSensitivity);
        tempLanguageIndex = PlayerPrefs.GetInt("LanguageIndex", 0);

        savedMouseSensitivity = tempMouseSensitivity;
        savedLanguageIndex = tempLanguageIndex;

        // Gán vào UI
        fullscreenToggle.isOn = tempFullscreen;
        resolutionDropdown.value = tempResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        for (int i = 0; i < fpsDropdown.options.Count; i++)
        {
            if (int.Parse(fpsDropdown.options[i].text.Replace(" FPS", "")) == tempFPS)
            {
                fpsDropdown.value = i;
                fpsDropdown.RefreshShownValue();
                break;
            }
        }

        // Music slider
        musicSlider.minValue = 0f;
        musicSlider.maxValue = 1f;
        musicSlider.value = tempMusicVolume;
        musicSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.AddListener((value) =>
        {
            tempMusicVolume = value;
            UpdateMusicValueText(value);
        });
        UpdateMusicValueText(tempMusicVolume);

        // Effects slider
        effectsSlider.minValue = 0f;
        effectsSlider.maxValue = 1f;
        effectsSlider.value = tempEffectsVolume;
        effectsSlider.onValueChanged.RemoveAllListeners();
        effectsSlider.onValueChanged.AddListener((value) =>
        {
            tempEffectsVolume = value;
            UpdateEffectsValueText(value);
        });
        UpdateEffectsValueText(tempEffectsVolume);

        // Mouse sensitivity
        mouseSlider.minValue = 0.1f;
        mouseSlider.maxValue = 10f;
        mouseSlider.value = tempMouseSensitivity;
        mouseSlider.onValueChanged.RemoveAllListeners();
        mouseSlider.onValueChanged.AddListener((value) =>
        {
            tempMouseSensitivity = value;
            UpdateMouseValueText(value);
        });
        UpdateMouseValueText(tempMouseSensitivity);

        // Language
        languageDropdown.value = tempLanguageIndex;
        languageDropdown.RefreshShownValue();
        languageDropdown.onValueChanged.RemoveAllListeners();
        languageDropdown.onValueChanged.AddListener(OnLanguageChangedTemp);
    }

    private void OnLanguageChangedTemp(int index)
    {
        tempLanguageIndex = index;
        if (LocalizationSettings.AvailableLocales.Locales.Count > index)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        }
    }

    public void ApplySettings()
    {
        tempMusicVolume = musicSlider.value;
        tempEffectsVolume = effectsSlider.value;
        tempFullscreen = fullscreenToggle.isOn;
        tempResolutionIndex = resolutionDropdown.value;
        tempFPS = int.Parse(fpsDropdown.options[fpsDropdown.value].text.Replace(" FPS", ""));
        tempMouseSensitivity = mouseSlider.value;
        tempLanguageIndex = languageDropdown.value;

        // Áp dụng độ phân giải
        Screen.fullScreen = tempFullscreen;
        Resolution res = customResolutions[tempResolutionIndex];
        Screen.SetResolution(res.width, res.height, tempFullscreen);
        Application.targetFrameRate = tempFPS;

        // Lưu PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", tempMusicVolume);
        PlayerPrefs.SetFloat("EffectsVolume", tempEffectsVolume);
        PlayerPrefs.SetInt("Fullscreen", tempFullscreen ? 1 : 0);
        PlayerPrefs.SetInt("ResolutionIndex", tempResolutionIndex);
        PlayerPrefs.SetInt("TargetFPS", tempFPS);
        PlayerPrefs.SetFloat("MouseSensitivity", tempMouseSensitivity);
        PlayerPrefs.SetInt("LanguageIndex", tempLanguageIndex);
        PlayerPrefs.Save();

        savedMouseSensitivity = tempMouseSensitivity;
        savedLanguageIndex = tempLanguageIndex;

        Debug.Log("Settings Applied");
    }

    public void CancelSettings()
    {
        // Khôi phục ngôn ngữ
        tempLanguageIndex = savedLanguageIndex;
        if (LocalizationSettings.AvailableLocales.Locales.Count > savedLanguageIndex)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[savedLanguageIndex];
        }

        // Khôi phục chuột
        tempMouseSensitivity = savedMouseSensitivity;
        mouseSlider.value = tempMouseSensitivity;
        UpdateMouseValueText(tempMouseSensitivity);

        // Khôi phục các thiết lập còn lại
        LoadSettings();

        Debug.Log("Settings Cancelled");
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

using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Language")]
    public TMP_Dropdown languageDropdown;
    public LocalizationManager localizationManager;

    private Resolution[] resolutions;

    // Biến tạm
    private float tempMusicVolume;
    private float tempEffectsVolume;
    private bool tempFullscreen;
    private int tempResolutionIndex;
    private int tempFPS;
    private float tempMouseSensitivity;
    private int tempLanguageIndex;

    private float defaultMouseSensitivity = 5.0f;

    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;
        var options = new System.Collections.Generic.List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            string res = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(res);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);

        LoadSettings(currentResolutionIndex);
    }

    public void LoadSettings(int defaultResIndex = 0)
    {
        // Đọc từ PlayerPrefs
        tempMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        tempEffectsVolume = PlayerPrefs.GetFloat("EffectsVolume", 1f);
        tempFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        tempResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", defaultResIndex);
        tempFPS = PlayerPrefs.GetInt("TargetFPS", 60);
        tempMouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultMouseSensitivity);
        tempLanguageIndex = PlayerPrefs.GetInt("LanguageIndex", 0);

        // Gán vào UI
        musicSlider.value = tempMusicVolume;
        effectsSlider.value = tempEffectsVolume;
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

        // Mouse sensitivity
        mouseSlider.minValue = 0.1f;
        mouseSlider.maxValue = 10f;
        mouseSlider.value = tempMouseSensitivity;
        UpdateMouseValueText(tempMouseSensitivity);

        mouseSlider.onValueChanged.RemoveAllListeners();
        mouseSlider.onValueChanged.AddListener((value) =>
        {
            tempMouseSensitivity = value;
            UpdateMouseValueText(value);
        });

        // Language
        languageDropdown.value = tempLanguageIndex;
        languageDropdown.RefreshShownValue();
        languageDropdown.onValueChanged.RemoveAllListeners();
        languageDropdown.onValueChanged.AddListener((index) =>
        {
            tempLanguageIndex = index;
        });
    }

    public void ApplySettings()
    {
        // Lấy dữ liệu mới
        tempMusicVolume = musicSlider.value;
        tempEffectsVolume = effectsSlider.value;
        tempFullscreen = fullscreenToggle.isOn;
        tempResolutionIndex = resolutionDropdown.value;
        tempFPS = int.Parse(fpsDropdown.options[fpsDropdown.value].text.Replace(" FPS", ""));
        tempMouseSensitivity = mouseSlider.value;
        tempLanguageIndex = languageDropdown.value;

        // Áp dụng vào hệ thống
        Screen.fullScreen = tempFullscreen;
        Resolution res = resolutions[tempResolutionIndex];
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

        // Gọi đổi ngôn ngữ nếu có
        if (localizationManager != null)
        {
            localizationManager.ChangeLanguageImmediate(tempLanguageIndex);
        }

        Debug.Log("Settings Applied");
    }

    public void CancelSettings()
    {
        LoadSettings(); // Khôi phục lại giá trị đã lưu
        Debug.Log("Settings Cancelled");
    }

    private void UpdateMouseValueText(float value)
    {
        if (mouseValueText != null)
            mouseValueText.text = value.ToString("F2");
    }
}

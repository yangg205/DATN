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

    private Resolution[] resolutions;

    // Biến tạm
    private float tempMusicVolume;
    private float tempEffectsVolume;
    private bool tempFullscreen;
    private int tempResolutionIndex;
    private int tempFPS;

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
        // Đọc từ PlayerPrefs và lưu vào biến tạm
        tempMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        tempEffectsVolume = PlayerPrefs.GetFloat("EffectsVolume", 1f);
        tempFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        tempResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", defaultResIndex);
        tempFPS = PlayerPrefs.GetInt("TargetFPS", 60);

        // Update UI
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
    }

    public void ApplySettings()
    {
        // Lấy giá trị mới từ UI
        tempMusicVolume = musicSlider.value;
        tempEffectsVolume = effectsSlider.value;
        tempFullscreen = fullscreenToggle.isOn;
        tempResolutionIndex = resolutionDropdown.value;
        tempFPS = int.Parse(fpsDropdown.options[fpsDropdown.value].text.Replace(" FPS", ""));

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
        PlayerPrefs.Save();

        Debug.Log("Settings Applied");
    }

    public void CancelSettings()
    {
        // Quay về các giá trị đã lưu (không lưu gì hết)
        LoadSettings();
    }
}

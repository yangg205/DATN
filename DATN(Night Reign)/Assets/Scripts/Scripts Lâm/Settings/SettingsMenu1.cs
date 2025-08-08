//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using UnityEngine.Audio;
//using System.Linq;

//public class SettingsMenu : MonoBehaviour
//{
//    [Header("Audio")]
//    public AudioMixer audioMixer;
//    public Slider musicSlider;
//    public Slider sfxSlider;

//    [Header("Mouse")]
//    public Slider mouseSlider;
//    public TextMeshProUGUI mouseValueText;

//    [Header("Display")]
//    public TMP_Dropdown resolutionDropdown;
//    public TMP_Dropdown fpsDropdown;
//    public Toggle fullscreenToggle;

//    [Header("Language")]
//    public TMP_Dropdown languageDropdown;

//    private Resolution[] resolutions;
//    private readonly int[] fpsValues = { 30, 60, 120, 144, 240 };

//    void Start()
//    {
//        LoadResolutions();
//        LoadFPSOptions();
//        LoadSettings();

//        // Gán sự kiện OnValueChanged trực tiếp để lưu ngay khi đổi
//        musicSlider.onValueChanged.AddListener(SetMusicVolume);
//        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
//        mouseSlider.onValueChanged.AddListener(SetMouseSensitivity);
//        resolutionDropdown.onValueChanged.AddListener(SetResolution);
//        fpsDropdown.onValueChanged.AddListener(SetFPS);
//        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
//    }

//    void LoadResolutions()
//    {
//        resolutions = Screen.resolutions
//            .Select(res => new Resolution { width = res.width, height = res.height, refreshRate = res.refreshRate })
//            .Distinct()
//            .ToArray();

//        resolutionDropdown.ClearOptions();

//        int currentResolutionIndex = 0;
//        var options = resolutions.Select((res, index) =>
//        {
//            if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
//                currentResolutionIndex = index;
//            return $"{res.width} x {res.height} @ {res.refreshRate}Hz";
//        }).ToList();

//        resolutionDropdown.AddOptions(options);
//        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
//        resolutionDropdown.RefreshShownValue();
//    }

//    void LoadFPSOptions()
//    {
//        fpsDropdown.ClearOptions();
//        var fpsOptions = fpsValues.Select(f => f + " FPS").ToList();
//        fpsDropdown.AddOptions(fpsOptions);
//        fpsDropdown.value = PlayerPrefs.GetInt("FPSIndex", 1);
//    }

//    public void SetMusicVolume(float volume)
//    {
//        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
//        PlayerPrefs.SetFloat("MusicVolume", volume);
//        PlayerPrefs.Save();
//    }

//    public void SetSFXVolume(float volume)
//    {
//        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
//        PlayerPrefs.SetFloat("SFXVolume", volume);
//        PlayerPrefs.Save();
//    }

//    public void SetMouseSensitivity(float value)
//    {
//        mouseValueText.text = value.ToString("F0");
//        PlayerPrefs.SetFloat("MouseSensitivity", value);
//        PlayerPrefs.Save();
//    }

//    public void SetResolution(int index)
//    {
//        Resolution res = resolutions[index];
//        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
//        PlayerPrefs.SetInt("ResolutionIndex", index);
//        PlayerPrefs.Save();
//    }

//    public void SetFPS(int index)
//    {
//        Application.targetFrameRate = fpsValues[index];
//        PlayerPrefs.SetInt("FPSIndex", index);
//        PlayerPrefs.Save();
//    }

//    public void SetFullscreen(bool isFullscreen)
//    {
//        Screen.fullScreen = isFullscreen;
//        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
//        PlayerPrefs.Save();
//    }

//    void LoadSettings()
//    {
//        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
//        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
//        mouseSlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 50);

//        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
//    }
//}

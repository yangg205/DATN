
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // <- THÊM DÒNG NÀY

public class settingAudio : MonoBehaviour
{
    [SerializeField] Slider Slider;
    [SerializeField] Text textUI;
    [SerializeField] Image buttonImage;
    [SerializeField] Sprite spriteOn;
    [SerializeField] Sprite spriteOff;

    float num;
    float previousVolume = 1f;
    bool isMuted = false;

    void Start()
    {
        if (PlayerPrefs.HasKey("sldkey"))
            load();
        else
        {
            PlayerPrefs.SetFloat("sldkey", 1);
            load();
        }

        UpdateAudioUI();
        UpdateButtonImage();
    }

    public void Change()
    {
        AudioListener.volume = Slider.value;
        num = Slider.value * 100;
        textUI.text = num.ToString("F0");

        isMuted = (Slider.value <= 0);
        if (!isMuted)
            previousVolume = Slider.value;

        save();
        UpdateButtonImage();
    }

    public void save()
    {
        PlayerPrefs.SetFloat("sldkey", Slider.value);
        load();
    }

    public void load()
    {
        Slider.value = PlayerPrefs.GetFloat("sldkey");
        AudioListener.volume = Slider.value;
        isMuted = (Slider.value <= 0);
        if (!isMuted)
            previousVolume = Slider.value;

        UpdateAudioUI();
        UpdateButtonImage();
    }

    public void ToggleMute()
    {
        if (!isMuted)
        {
            previousVolume = Slider.value;
            AudioListener.volume = 0;
            Slider.value = 0;
            isMuted = true;
        }
        else
        {
            Slider.value = previousVolume;
            AudioListener.volume = previousVolume;
            isMuted = false;
        }

        textUI.text = (Slider.value * 100).ToString("F0");
        save();
        UpdateButtonImage();
    }

    private void UpdateAudioUI()
    {
        num = Slider.value * 100;
        textUI.text = num.ToString("F0");
    }

    private void UpdateButtonImage()
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = isMuted ? spriteOff : spriteOn;
        }
    }

    // ✅ Hàm mới: Quay lại scene Menu
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu"); // Thay "Menu" bằng tên scene menu thực tế nếu khác
    }
}


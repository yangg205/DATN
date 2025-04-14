//using UnityEngine;
//using UnityEngine.UI;
//using Fusion;

//public class settingAudio : MonoBehaviour         //              (1)or(2) -->  (3)
//{                                                //Start +... --> SetFloat -->  GetFloat
//                                                 //               Save           load (*.value is get)
//    [SerializeField] Slider Slider;
//    [SerializeField] Text textUI;
//    float num;

//    void Start()
//    {   
//        if (PlayerPrefs.HasKey("sldkey"))// is value save before (2)
//            load();//3
//        else
//        {
//            PlayerPrefs.SetFloat("sldkey", 1);// setFloat is value first time (1)" we can chose value 
//            load();//(3)
//        }
//    }
//    public void Change()
//    {
//        AudioListener.volume = Slider.value;
//        num = Slider.value * 100;
//        textUI.text = num.ToString();
//        save(); // change and save in the same time ;
//    }
//    public void save()
//    {
//        PlayerPrefs.SetFloat("sldkey",Slider.value);
//        load();
//    }
//    public void load()
//    {
//        Slider.value = PlayerPrefs.GetFloat("sldkey");
//    }
//}











//using UnityEngine;
//using UnityEngine.UI;
//using Fusion;

//public class settingAudio : MonoBehaviour
//{
//    [SerializeField] Slider Slider;
//    [SerializeField] Text textUI;
//    [SerializeField] Image buttonImage;
//    [SerializeField] Sprite spriteOn;     // hình khi bật âm
//    [SerializeField] Sprite spriteOff;    // hình khi tắt âm

//    float num;
//    float previousVolume = 1f; // lưu volume trước khi tắt
//    bool isMuted = false;

//    void Start()
//    {
//        if (PlayerPrefs.HasKey("sldkey"))
//            load();
//        else
//        {
//            PlayerPrefs.SetFloat("sldkey", 1);
//            load();
//        }

//        UpdateAudioUI();
//        UpdateButtonImage();
//    }

//    public void Change()
//    {
//        AudioListener.volume = Slider.value;
//        num = Slider.value * 100;
//        textUI.text = num.ToString("F0");

//        isMuted = (Slider.value <= 0);
//        if (!isMuted)
//            previousVolume = Slider.value;

//        save();
//        UpdateButtonImage();
//    }

//    public void save()
//    {
//        PlayerPrefs.SetFloat("sldkey", Slider.value);
//        load();
//    }

//    public void load()
//    {
//        Slider.value = PlayerPrefs.GetFloat("sldkey");
//        AudioListener.volume = Slider.value;
//        isMuted = (Slider.value <= 0);
//        if (!isMuted)
//            previousVolume = Slider.value;

//        UpdateAudioUI();
//        UpdateButtonImage();
//    }

//    // Toggle bật / tắt nhạc
//    public void ToggleMute()
//    {
//        if (!isMuted)
//        {
//            // Lưu lại volume trước khi tắt
//            previousVolume = Slider.value;
//            AudioListener.volume = 0;
//            Slider.value = 0;
//            isMuted = true;
//        }
//        else
//        {
//            // Bật lại âm thanh
//            Slider.value = previousVolume;
//            AudioListener.volume = previousVolume;
//            isMuted = false;
//        }

//        textUI.text = (Slider.value * 100).ToString("F0");
//        save();
//        UpdateButtonImage();
//    }

//    private void UpdateAudioUI()
//    {
//        num = Slider.value * 100;
//        textUI.text = num.ToString("F0");
//    }

//    private void UpdateButtonImage()
//    {
//        if (buttonImage != null)
//        {
//            buttonImage.sprite = isMuted ? spriteOff : spriteOn;
//        }
//    }
//}










using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // <- THÊM DÒNG NÀY
using Fusion;

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


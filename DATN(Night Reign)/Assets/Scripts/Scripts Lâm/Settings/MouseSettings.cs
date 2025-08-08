using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ND; // 📌 Namespace của CameraHandler

public class MouseSettings : MonoBehaviour
{
    [Header("Mouse Sensitivity UI")]
    public Slider mouseSlider;
    public TextMeshProUGUI mouseValueText;

    public float currentMouseSensitivity { get; private set; }

    private float defaultSensitivity = 5.0f;

    void Start()
    {
        mouseSlider.minValue = 0f;
        mouseSlider.maxValue = 10f;

        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultSensitivity);
        mouseSlider.value = savedSensitivity;
        currentMouseSensitivity = savedSensitivity;

        UpdateMouseValue(savedSensitivity);

        // Lắng nghe thay đổi slider
        mouseSlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

        // Cập nhật ngay lookSpeed & pivotSpeed khi mở menu
        ApplyToCameraHandler(savedSensitivity);
    }

    void OnMouseSensitivityChanged(float value)
    {
        currentMouseSensitivity = value;
        UpdateMouseValue(value);

        // Lưu ngay lập tức
        PlayerPrefs.SetFloat("MouseSensitivity", currentMouseSensitivity);
        PlayerPrefs.Save();

        // Cập nhật CameraHandler
        ApplyToCameraHandler(value);
    }

    void UpdateMouseValue(float value)
    {
        if (mouseValueText != null)
            mouseValueText.text = value.ToString("F2");
    }

    void ApplyToCameraHandler(float sensitivity)
    {
        if (CameraHandler.singleton != null)
        {
            CameraHandler.singleton.lookSpeed = sensitivity * 0.02f;
            CameraHandler.singleton.pivotSpeed = sensitivity * 0.02f;
        }
        else
        {
            Debug.LogWarning("Không tìm thấy CameraHandler trong scene!");
        }
    }

    // Cho script khác gọi để lấy giá trị
    public static float GetSavedSensitivity()
    {
        return PlayerPrefs.GetFloat("MouseSensitivity", 5.0f);
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MouseSettings : MonoBehaviour
{
    [Header("Mouse Sensitivity UI")]
    public Slider mouseSlider;
    public TextMeshProUGUI mouseValueText;

    public float currentMouseSensitivity { get; private set; }

    private float defaultSensitivity = 5.0f;

    void Start()
    {
        // Đặt giới hạn giá trị slider
        mouseSlider.minValue = 0.1f;
        mouseSlider.maxValue = 10f;

        // Load từ PlayerPrefs hoặc dùng mặc định
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultSensitivity);
        mouseSlider.value = savedSensitivity;
        currentMouseSensitivity = savedSensitivity;

        UpdateMouseValue(savedSensitivity);

        // Bắt sự kiện thay đổi slider
        mouseSlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    void OnMouseSensitivityChanged(float value)
    {
        currentMouseSensitivity = value;
        UpdateMouseValue(value);
        // KHÔNG lưu ở đây nữa, chỉ lưu khi bấm Apply
    }

    void UpdateMouseValue(float value)
    {
        if (mouseValueText != null)
            mouseValueText.text = value.ToString("F2");
    }

    // Gọi từ ApplyButton
    public void ApplyMouseSensitivity()
    {
        PlayerPrefs.SetFloat("MouseSensitivity", currentMouseSensitivity);
    }
}

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
        // Thiết lập giới hạn slider
        mouseSlider.minValue = 0.1f;
        mouseSlider.maxValue = 10f;

        // Gắn listener trước
        mouseSlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

        // Load từ PlayerPrefs hoặc dùng mặc định
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultSensitivity);

        // Gán giá trị sau để tự trigger OnMouseSensitivityChanged và cập nhật text
        mouseSlider.value = savedSensitivity;
    }

    void OnMouseSensitivityChanged(float value)
    {
        currentMouseSensitivity = value;
        UpdateMouseValue(value);
        // Không lưu ở đây, chỉ lưu khi nhấn Apply
    }

    void UpdateMouseValue(float value)
    {
        if (mouseValueText != null)
            mouseValueText.text = value.ToString("F2");
    }

    // Gọi từ nút Apply
    public void ApplyMouseSensitivity()
    {
        PlayerPrefs.SetFloat("MouseSensitivity", currentMouseSensitivity);
        PlayerPrefs.Save(); // Lưu ngay lập tức (tùy chọn)
    }
}

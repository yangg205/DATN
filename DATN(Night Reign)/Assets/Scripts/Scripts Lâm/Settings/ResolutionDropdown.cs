using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class ResolutionDropdownManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;

    // Chỉ giữ 2 độ phân giải mong muốn
    private Resolution[] filteredResolutions = new Resolution[]
    {
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1920, height = 1080 }
    };

    void Start()
    {
        SetupResolutionDropdown();
        AdjustDropdownLayout();

        // Load lại độ phân giải đã lưu (nếu có)
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            int savedIndex = PlayerPrefs.GetInt("ResolutionIndex");
            SetResolution(savedIndex);
            resolutionDropdown.value = savedIndex;
            resolutionDropdown.RefreshShownValue();
        }

        // Gắn sự kiện khi đổi dropdown
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < filteredResolutions.Length; i++)
        {
            string option = $"{filteredResolutions[i].width} x {filteredResolutions[i].height}";
            options.Add(option);

            if (Screen.currentResolution.width == filteredResolutions[i].width &&
                Screen.currentResolution.height == filteredResolutions[i].height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void AdjustDropdownLayout()
    {
        RectTransform rect = resolutionDropdown.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, rect.anchorMin.y);
        rect.anchorMax = new Vector2(0.5f, rect.anchorMax.y);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    public void SetResolution(int index)
    {
        Resolution res = filteredResolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);

        // Lưu lại
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }
}

using UnityEngine;
using UnityEngine.UI;
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

            // Nếu khớp độ phân giải hiện tại của máy
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

    // Tự động canh giữa và chỉnh layout hiển thị dropdown
    private void AdjustDropdownLayout()
    {
        RectTransform rect = resolutionDropdown.GetComponent<RectTransform>();
        //rect.sizeDelta = new Vector2(220, 30); // chiều rộng, chiều cao dropdown
        //rect.anchoredPosition = new Vector2(0, rect.anchoredPosition.y); // canh giữa theo chiều ngang
        rect.anchorMin = new Vector2(0.5f, rect.anchorMin.y);
        rect.anchorMax = new Vector2(0.5f, rect.anchorMax.y);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    public void SetResolution(int index)
    {
        Resolution res = filteredResolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}

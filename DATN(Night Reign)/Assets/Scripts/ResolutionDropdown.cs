using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResolutionDropdownManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;

    private Resolution[] filteredResolutions = new Resolution[]
    {
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1920, height = 1080 }
    };

    void Start()
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

    public void SetResolution(int index)
    {
        Resolution res = filteredResolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}

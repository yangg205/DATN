using UnityEngine;
using UnityEngine.UI; // Thêm dòng này để làm việc với UI
using System.Collections.Generic; // Để sử dụng List

public class Panel_Toggler : MonoBehaviour
{
    public List<GameObject> panels;

    public void TogglePanel(int panelIndex)
    {
        if (panelIndex >= 0 && panelIndex < panels.Count)
        {
            GameObject currentPanel = panels[panelIndex];
            Debug.Log($"Button {panelIndex} clicked, toggling panel: {currentPanel.name}");

            if (currentPanel.activeSelf && panelIndex==3)
            {
                currentPanel.SetActive(false);
            }
            else
            {
                HideAllPanels();
                currentPanel.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Panel index out of range: " + panelIndex);
        }
    }
    private void HideAllPanels()
    {
        foreach (GameObject panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
    void Start()
    {
        HideAllPanels();
    }
}
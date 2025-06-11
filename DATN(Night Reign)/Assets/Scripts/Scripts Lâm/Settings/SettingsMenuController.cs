using UnityEngine;
using DG.Tweening;

public class SettingsMenuController : MonoBehaviour
{
    public RectTransform settingsPanel;

    public void ShowSettings()
    {
        settingsPanel.gameObject.SetActive(true);
        settingsPanel.anchoredPosition = new Vector2(1000, 0);
        settingsPanel.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutExpo);
    }

    public void HideSettings()
    {
        settingsPanel.DOAnchorPos(new Vector2(1000, 0), 0.5f).SetEase(Ease.InExpo)
            .OnComplete(() => settingsPanel.gameObject.SetActive(false));
    }
}

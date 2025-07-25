using UnityEngine;
using DG.Tweening;

public class SettingsMenuController : MonoBehaviour
{
    public RectTransform settingsPanel;
    public RectTransform rankPanel;

    // Hiện Settings Panel
    public void ShowSettings()
    {
        rankPanel.gameObject.SetActive(false); // Ẩn rank nếu đang mở
        settingsPanel.gameObject.SetActive(true);
        settingsPanel.anchoredPosition = new Vector2(1000, 0);
        settingsPanel.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutExpo);
    }

    // Ẩn Settings Panel
    public void HideSettings()
    {
        settingsPanel.DOAnchorPos(new Vector2(1000, 0), 0.5f).SetEase(Ease.InExpo)
            .OnComplete(() => settingsPanel.gameObject.SetActive(false));
    }

    // Hiện Rank Panel
    public void ShowRank()
    {
        settingsPanel.gameObject.SetActive(false); // Ẩn settings nếu đang mở
        rankPanel.gameObject.SetActive(true);
        rankPanel.anchoredPosition = new Vector2(1000, 0);
        rankPanel.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutExpo);
    }

    // Ẩn Rank Panel
    public void HideRank()
    {
        rankPanel.DOAnchorPos(new Vector2(1000, 0), 0.5f).SetEase(Ease.InExpo)
            .OnComplete(() => rankPanel.gameObject.SetActive(false));
    }
}

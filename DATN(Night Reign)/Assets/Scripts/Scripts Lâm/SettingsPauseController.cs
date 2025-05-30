using UnityEngine;
using DG.Tweening;

public class SettingsPauseController : MonoBehaviour
{
    public RectTransform settingsPanel;

    public void ShowSettings()
    {
        settingsPanel.gameObject.SetActive(true);
        settingsPanel.anchoredPosition = new Vector2(0, 0);
        settingsPanel.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutExpo);
    }

    public void HideSettings()
    {

        // Animate ẩn panel bằng DOTween, dùng SetUpdate(true) để chạy ngay cả khi game đang pause
        settingsPanel.DOAnchorPos(new Vector2(1920, 0), 0.5f)
            .SetEase(Ease.InExpo)
            .SetUpdate(true) // BẮT BUỘC nếu Time.timeScale = 0
            .OnComplete(() =>
            {
                settingsPanel.gameObject.SetActive(false);
                Debug.Log("Đã ẩn SettingsPanel");
            });
    }


}

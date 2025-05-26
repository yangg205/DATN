using UnityEngine;
using DG.Tweening; // Nhớ import DOTween

public class MainMenuAnimator : MonoBehaviour
{
    public RectTransform menuPanel;
    public CanvasGroup canvasGroup;

    void Start()
    {
        // Ẩn ban đầu
        menuPanel.localScale = Vector3.zero;
        canvasGroup.alpha = 0;

        // Gọi hiệu ứng mở menu
        ShowMenu();
    }

    public void ShowMenu()
    {
        // Scale từ nhỏ tới bình thường
        menuPanel.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        // Làm hiện dần
        canvasGroup.DOFade(1f, 0.5f);
    }

    public void HideMenu()
    {
        // Ẩn dần
        canvasGroup.DOFade(0f, 0.3f);
        menuPanel.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
    }
}

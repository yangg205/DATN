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
        menuPanel.DOScale(Vector3.one, 10f).SetEase(Ease.OutBack);

        // Làm hiện dần
        canvasGroup.DOFade(2f, 2f);
    }

    public void HideMenu()
    {
        // Ẩn dần
        canvasGroup.DOFade(1f, 1f);
        menuPanel.DOScale(Vector3.zero, 1f).SetEase(Ease.InBack);
    }
}

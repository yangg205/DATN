using UnityEngine;
using DG.Tweening;

public class PanelFadeIn : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Chỉ làm trong suốt, không tắt hẳn GameObject
        canvasGroup.alpha = 0f;
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);  // Đảm bảo panel bật
        canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutCubic);
    }

    public void HidePanel()
    {
        canvasGroup.DOFade(0f, fadeDuration);
    }
}

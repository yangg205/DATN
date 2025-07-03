using UnityEngine;
using DG.Tweening;

public class MenuButtonsAnimator : MonoBehaviour
{
    [System.Serializable]
    public class MenuButton
    {
        public RectTransform rect;
        public CanvasGroup canvasGroup;
    }

    public MenuButton[] buttons;
    public float duration = 0.5f;

    [Header("Slide Settings")]
    public float slideFromY = -100f; // 👈 Trượt từ vị trí Y thấp hơn bao nhiêu pixel

    void Awake()
    {
        foreach (var btn in buttons)
        {
            // Đặt vị trí ban đầu thấp hơn (từ dưới lên)
            Vector2 startPos = btn.rect.anchoredPosition + new Vector2(0, slideFromY);
            btn.rect.anchoredPosition = startPos;

            btn.canvasGroup.alpha = 0f;
            btn.canvasGroup.interactable = false;
            btn.canvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowAllButtons()
    {
        foreach (var btn in buttons)
        {
            // Trượt lên lại đúng vị trí gốc
            btn.rect.DOAnchorPosY(btn.rect.anchoredPosition.y - slideFromY, duration)
                   .SetEase(Ease.OutCubic);

            // Fade in mượt
            btn.canvasGroup.DOFade(1f, duration).SetEase(Ease.Linear);

            btn.canvasGroup.interactable = true;
            btn.canvasGroup.blocksRaycasts = true;
        }
    }
}

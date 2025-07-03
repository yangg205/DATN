using UnityEngine;
using DG.Tweening;

public class PanelShowEffect : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public RectTransform panel;
    public PanelShowEffect menuPanel;
    public GameObject buttonToHide;         // Nút bấm sẽ ẩn sau khi bấm

    public float duration = 0.4f;
    private Vector2 startPos;
    private Vector2 endPos;

    private Vector3 startScale = new Vector3(0.8f, 0.8f, 0.8f);
    private Vector3 endScale = Vector3.one;

    void Awake()
    {
        endPos = panel.anchoredPosition;
        startPos = endPos + new Vector2(0,100); // trượt lên

        panel.anchoredPosition = startPos;
        panel.localScale = startScale;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    public void OnClickOpenPanel()
    {
        menuPanel.Show();
        buttonToHide.SetActive(false);      // Ẩn nút sau khi bấm
    }

    public void Show()
    {
        canvasGroup.DOFade(1, duration);
        panel.DOAnchorPos(endPos, duration).SetEase(Ease.OutCubic);
        panel.DOScale(endScale, duration).SetEase(Ease.OutBack);

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
}

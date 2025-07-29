using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class killcoinebossPanel : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f;

    private Canvas panelCanvas;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Tạo canvas riêng nếu chưa có
        panelCanvas = GetComponent<Canvas>();
        if (panelCanvas == null) panelCanvas = gameObject.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 0; // Mặc định thấp, khi ShowPanel sẽ tăng

        canvasGroup.alpha = 0f;
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);

        // Đẩy panel lên trên cùng
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 900;

        canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutCubic);
    }

    public void HidePanel()
    {
        canvasGroup.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            // Đưa sort order về mặc định nếu cần
            panelCanvas.sortingOrder = 0;
        });
    }
}

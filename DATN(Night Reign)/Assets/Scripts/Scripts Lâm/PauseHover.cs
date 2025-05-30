using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class PauseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TextMeshProUGUI text;
    public Color normalColor = Color.gray;
    public Color hoverColor = new Color(0f, 1f, 1f); // Neon cyan
    public float scaleAmount = 1.05f;
    public float transitionSpeed = 5f;

    private Vector3 originalScale;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;
        text.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        StopAllCoroutines();
        StartCoroutine(HoverEffect(hoverColor, originalScale * scaleAmount));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        StopAllCoroutines();
        StartCoroutine(HoverEffect(normalColor, originalScale));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StopAllCoroutines();

        // Reset về mặc định
        transform.localScale = originalScale;
        text.color = normalColor;

        // Sau khi reset, nếu chuột vẫn đang hover, khởi động lại hiệu ứng hover
        if (isHovered && gameObject.activeInHierarchy)
        {
            StartCoroutine(HoverEffect(hoverColor, originalScale * scaleAmount));
        }
    }

    System.Collections.IEnumerator HoverEffect(Color targetColor, Vector3 targetScale)
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.001f ||
               text.color != targetColor)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
            text.color = Color.Lerp(text.color, targetColor, Time.deltaTime * transitionSpeed);
            yield return null;
        }

        // Đảm bảo chính xác giá trị cuối cùng
        transform.localScale = targetScale;
        text.color = targetColor;
    }
}

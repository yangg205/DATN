using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MenuHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI text;
    public Color normalColor = Color.gray;
    //public Color hoverColor = new Color(1f, 0.8f, 0.3f); // vàng nhạt
    public Color hoverColor = new Color(0f, 1f, 1f); // Neon cyan
    public float scaleAmount = 1.05f;
    public float transitionSpeed = 5f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        text.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(HoverEffect(hoverColor, originalScale * scaleAmount));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(HoverEffect(normalColor, originalScale));
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
    }
}

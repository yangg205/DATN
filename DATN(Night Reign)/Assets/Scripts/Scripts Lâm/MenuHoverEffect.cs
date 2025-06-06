using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MenuHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text text;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public float scaleAmount = 1.05f;
    public float transitionSpeed = 5f;

    private Vector3 originalScale;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;
        if (text != null)
            text.color = normalColor;
    }

    void Update()
    {
        // Smooth scale animation
        Vector3 targetScale = isHovered ? originalScale * scaleAmount : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (text != null)
            text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (text != null)
            text.color = normalColor;
    }

    void OnDisable()
    {
        // Reset khi menu bị tắt
        isHovered = false;
        transform.localScale = originalScale;
        if (text != null)
            text.color = normalColor;
    }
}

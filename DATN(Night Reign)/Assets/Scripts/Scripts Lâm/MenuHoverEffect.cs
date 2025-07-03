using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class MenuHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text Settings")]
    public TMP_Text text;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    [Header("Glow Settings")]
    public RawImage glowImage; // ảnh phát sáng bên dưới text
    public float glowFadeSpeed = 5f;
    private Color glowColorTransparent;
    private Color glowColorVisible;

    [Header("Scale Settings")]
    public float scaleAmount = 1.05f;
    public float transitionSpeed = 5f;

    private Vector3 originalScale;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;

        if (text != null)
            text.color = normalColor;

        if (glowImage != null)
        {
            glowColorVisible = glowImage.color;
            glowColorTransparent = new Color(glowColorVisible.r, glowColorVisible.g, glowColorVisible.b, 0);
            glowImage.color = glowColorTransparent;
        }
    }

    void Update()
    {
        // Smooth scale
        Vector3 targetScale = isHovered ? originalScale * scaleAmount : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * transitionSpeed);

        // Smooth glow alpha
        if (glowImage != null)
        {
            Color targetColor = isHovered ? glowColorVisible : glowColorTransparent;
            glowImage.color = Color.Lerp(glowImage.color, targetColor, Time.unscaledDeltaTime * glowFadeSpeed);
        }
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
        isHovered = false;
        transform.localScale = originalScale;

        if (text != null)
            text.color = normalColor;

        if (glowImage != null)
            glowImage.color = glowColorTransparent;
    }
}

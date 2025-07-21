using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class MenuHoverEffect : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler
{
     [Header("Text Settings")]
    public TMP_Text text;
    public Color textColor = Color.white;

    [Header("Glow Settings")]
    public RawImage glowImage; // ảnh phát sáng bên dưới text
    public float glowSpeed = 2f; // tốc độ nhấp nháy
    public float minAlpha = 0.1f;
    public float maxAlpha = 1f;

    [Header("Scale Settings (Optional)")]
    public float scaleAmount = 1.05f;
    public float scaleSpeed = 2f;

    private Vector3 originalScale;
    private float glowTimer = 0f;

    void Start()
    {
        originalScale = transform.localScale;

        if (text != null)
            text.color = textColor;

        if (glowImage != null)
        {
            Color color = glowImage.color;
            color.a = minAlpha;
            glowImage.color = color;
        }
    }

    void Update()
    {
        // Glow alpha oscillation
        if (glowImage != null)
        {
            glowTimer += Time.unscaledDeltaTime * glowSpeed;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(glowTimer, 1f));
            Color color = glowImage.color;
            color.a = alpha;
            glowImage.color = color;
        }

        // Optional: continuous pulsing scale
        transform.localScale = originalScale * (1f + 0.02f * Mathf.Sin(Time.unscaledTime * scaleSpeed));
    }

    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     isHovered = true;
    //     if (text != null)
    //         text.color = hoverColor;
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     isHovered = false;
    //     if (text != null)
    //         text.color = normalColor;
    // }

    // void OnDisable()
    // {
    //     isHovered = false;
    //     transform.localScale = originalScale;

    //     if (text != null)
    //         text.color = normalColor;

    //     if (glowImage != null)
    //         glowImage.color = glowColorTransparent;
    // }
}

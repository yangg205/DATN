using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class buttonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f);
    public Color hoverColor = Color.yellow;

    private Vector3 originalScale;
    private Color originalColor;
    private Image buttonImage;

    public AudioClip hoverSound;
    private AudioSource audioSource;

    // Các trường public để kéo thả trong Inspector
    public Image glowImage1; // Tấm ảnh ánh sáng thứ nhất
    public Image glowImage2; // Tấm ảnh ánh sáng thứ hai
    private Color glowOriginalColor1;
    private Color glowOriginalColor2;

    void Start()
    {
        originalScale = transform.localScale;
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        audioSource = gameObject.AddComponent<AudioSource>();

        // Lưu màu gốc và đặt alpha ban đầu là 0 (ẩn) cho các tấm ảnh ánh sáng
        if (glowImage1 != null)
        {
            glowOriginalColor1 = glowImage1.color;
            glowImage1.color = new Color(glowOriginalColor1.r, glowOriginalColor1.g, glowOriginalColor1.b, 0f);
        }
        else
        {
            Debug.LogWarning("GlowImage1 chưa được gán trong Inspector!");
        }

        if (glowImage2 != null)
        {
            glowOriginalColor2 = glowImage2.color;
            glowImage2.color = new Color(glowOriginalColor2.r, glowOriginalColor2.g, glowOriginalColor2.b, 0f);
        }
        else
        {
            Debug.LogWarning("GlowImage2 chưa được gán trong Inspector!");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = hoverScale;
        if (buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }
        if (hoverSound != null) audioSource.PlayOneShot(hoverSound);

        // Hiện các tấm ảnh ánh sáng
        if (glowImage1 != null)
        {
            glowImage1.color = new Color(glowOriginalColor1.r, glowOriginalColor1.g, glowOriginalColor1.b, 1f);
        }
        if (glowImage2 != null)
        {
            glowImage2.color = new Color(glowOriginalColor2.r, glowOriginalColor2.g, glowOriginalColor2.b, 1f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }

        // Ẩn các tấm ảnh ánh sáng
        if (glowImage1 != null)
        {
            glowImage1.color = new Color(glowOriginalColor1.r, glowOriginalColor1.g, glowOriginalColor1.b, 0f);
        }
        if (glowImage2 != null)
        {
            glowImage2.color = new Color(glowOriginalColor2.r, glowOriginalColor2.g, glowOriginalColor2.b, 0f);
        }
    }
}
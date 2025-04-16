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

    void Start()
    {
        originalScale = transform.localScale;
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = hoverScale;
        if (buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }
        if (hoverSound != null) audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
    }

}

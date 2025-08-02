using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterHoverImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Image")]
    public RawImage hoverImage;

    void Awake()
    {
        if (hoverImage != null)
            hoverImage.enabled = false; // Ẩn mặc định
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverImage != null)
            hoverImage.enabled = true; // Hiện ảnh khi hover
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverImage != null)
            hoverImage.enabled = false; // Ẩn ảnh khi rời chuột
    }
}

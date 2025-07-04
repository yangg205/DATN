using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterHoverOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Outline outline;

    void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false; // Ẩn mặc định
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (outline != null)
            outline.enabled = true; // Hiện khi hover
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (outline != null)
            outline.enabled = false; // Ẩn khi rời chuột
    }
}

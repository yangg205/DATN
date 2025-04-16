using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class uiHoverLight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image img;
    public float hoverAlpha = 1f; // Alpha khi hover (1 = rõ nét)
    public float normalAlpha = 0.5f; // Alpha mặc định (0.5 = hơi mờ)

    void Start()
    {
        img = GetComponent<Image>();

        if (img != null)
        {
            Color c = img.color;
            c.a = normalAlpha;
            img.color = c;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (img != null)
        {
            Color c = img.color;
            c.a = hoverAlpha;
            img.color = c;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (img != null)
        {
            Color c = img.color;
            c.a = normalAlpha;
            img.color = c;
        }
    }
}
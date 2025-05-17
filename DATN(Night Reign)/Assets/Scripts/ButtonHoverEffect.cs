using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour
{
    private Image buttonImage;
    private Outline outline;
    private static ButtonHoverEffect lastClickedButton;

    void Awake()
    {
        buttonImage = GetComponent<Image>();
        outline = gameObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.effectColor = Color.yellow;
        outline.effectDistance = new Vector2(3, 3);
        outline.enabled = false;
    }

    // Được gán từ EventTrigger
    public void OnPointerEnterEvent(BaseEventData data)
    {
        outline.enabled = true;
    }

    public void OnPointerExitEvent(BaseEventData data)
    {
        if (this != lastClickedButton)
        {
            outline.enabled = false;
        }
    }

    public void OnPointerClickEvent(BaseEventData data)
    {
        if (lastClickedButton != null && lastClickedButton != this)
        {
            lastClickedButton.outline.enabled = false;
        }
        outline.enabled = true;
        lastClickedButton = this;
    }
}

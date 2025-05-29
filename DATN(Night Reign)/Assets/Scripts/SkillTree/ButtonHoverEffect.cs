using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class ButtonHoverEffect : MonoBehaviour
{
    private Image buttonImage;
    private Outline outline;
    private static ButtonHoverEffect lastClickedButton;
    public int videoIndex; // Gán index video cho mỗi nút
    public int SkillIndex;
    public VideoDisplayManager videoManager;


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

        // Gọi video tương ứng
        if (videoManager != null)
        {
            videoManager.PlayVideo(videoIndex);
            videoManager.ShowText(SkillIndex);
        }
    }

}

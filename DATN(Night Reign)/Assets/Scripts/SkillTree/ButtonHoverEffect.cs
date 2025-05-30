using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverEffect : MonoBehaviour
{
    private Image buttonImage;
    private Outline outline;
    private static ButtonHoverEffect lastClickedButton;
    [SerializeField] int videoIndex; // Gán index video cho mỗi nút
    [SerializeField] int skillIndex; // Gán index kỹ năng cho mỗi nút
    [SerializeField] int skillId;    // ID của kỹ năng
    [SerializeField] SkillTreeManager manager;

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

    public void OnPointerEnter(BaseEventData data)
    {
        outline.enabled = true;
    }

    public void OnPointerExit(BaseEventData data)
    {
        if (this != lastClickedButton)
        {
            outline.enabled = false;
        }
    }

    public void OnPointerClick(BaseEventData data)
    {
        if (lastClickedButton != null && lastClickedButton != this)
        {
            lastClickedButton.outline.enabled = false;
        }

        outline.enabled = true;
        lastClickedButton = this;

        // Gọi video và hiển thị thông tin kỹ năng
        if (manager != null)
        {
            manager.PlayVideo(videoIndex);
            manager.ShowText(skillIndex);
        }
    }

    public void OnClickSendSkillId()
    {
        if (manager != null)
        {
            manager.SelectSkill(skillId);
        }
        else
        {
            Debug.LogError("SkillTreeManager is not assigned!");
        }
    }
}

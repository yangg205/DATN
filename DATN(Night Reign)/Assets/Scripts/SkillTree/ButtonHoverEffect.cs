using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System;

public class ButtonHoverEffect : MonoBehaviour
{
    private Image buttonImage;
    private Outline outline;
    private static ButtonHoverEffect lastClickedButton;
    [SerializeField] int videoIndex; // Gán index video cho mỗi nút
    [SerializeField] int SkillIndex;
    [SerializeField] int skillId;
    [SerializeField] SkillTreeManager Manager;


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
        if (Manager != null)
        {
            Manager.PlayVideo(videoIndex);
            Manager.ShowText(SkillIndex);
        }
    }
    public void OnClickUpdateSkill1_1(BaseEventData data)
    {
        if(Manager == null)
        {
            Console.WriteLine("Manager khong ton tai");
        }
        Manager.UpdateSkill(1,skillId);
    }

}

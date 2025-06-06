using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Image buttonImage;
    private Outline outline;
    private static ButtonHoverEffect lastClickedButton;
    [SerializeField] private int videoIndex; // Gán index video cho mỗi nút
    [SerializeField] private int skillIndex; // Gán index kỹ năng cho mỗi nút
    [SerializeField] public int skillId;    // ID của kỹ năng
    [SerializeField] private SkillTreeManager manager;
    private bool isOwned = false;            // Trạng thái sở hữu kỹ năng

    void Awake()
    {
        // Lấy hoặc thêm Image component
        buttonImage = GetComponent<Image>();
        if (buttonImage == null)
        {
            Debug.LogError("Image component is missing on " + gameObject.name);
        }

        // Lấy hoặc thêm Outline component
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.effectColor = Color.yellow;
        outline.effectDistance = new Vector2(3, 3);
        outline.enabled = false;

        // Đảm bảo có Button component
        if (GetComponent<Button>() == null)
        {
            Debug.LogWarning("Button component is missing on " + gameObject.name + ". Adding one...");
            gameObject.AddComponent<Button>();
        }

        // Đảm bảo Raycast Target được bật
        if (buttonImage != null)
        {
            buttonImage.raycastTarget = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isOwned && outline != null)
        {
            outline.enabled = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isOwned && this != lastClickedButton && outline != null)
        {
            outline.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (lastClickedButton != null && lastClickedButton != this && !lastClickedButton.isOwned)
        {
            lastClickedButton.outline.enabled = false;
        }

        if (outline != null)
        {
            outline.enabled = true;
        }
        lastClickedButton = this;

        // Gọi video và hiển thị thông tin kỹ năng
        if (manager != null)
        {
            manager.PlayVideo(videoIndex);
            manager.ShowText(skillIndex);
        }
        else
        {
            Debug.LogError("SkillTreeManager is not assigned on " + gameObject.name);
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
            Debug.LogError("SkillTreeManager is not assigned on " + gameObject.name);
        }
    }

    public void SetOutline(bool enable)
    {
        if (outline != null)
        {
            outline.enabled = enable;
        }
    }

    public void SetOwned(bool owned)
    {
        isOwned = owned;
        SetOutline(owned); // Đảm bảo bật/tắt outline dựa trên trạng thái sở hữu
    }
}

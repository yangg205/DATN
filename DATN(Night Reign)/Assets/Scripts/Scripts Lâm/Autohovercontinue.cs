using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoOutline : MonoBehaviour
{
    private Outline outline;
    private bool isSelected = false;
    [SerializeField] bool status= false;

    private void Start()
    {
        outline = GetComponent<Outline>();    }

    private void Update()
    {
        if (status == true)
        {
            if (outline != null)
            {
                outline.enabled = true; // Hiển thị sẵn Outline
                outline.effectColor = Color.yellow; // Có thể đổi màu nếu muốn khác lúc hover
            }
        }

    }

    public void Deselect()
    {
        isSelected = false;
        if (outline != null)
        {
            outline.effectColor = Color.yellow; // Quay về màu hover ban đầu
        }
    }
}

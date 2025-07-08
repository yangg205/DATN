using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class AutoResizePanel : MonoBehaviour
{
    [Header("Text cần đo kích thước")]
    public TMP_Text tmpText;

    [Header("Padding khung nền (trái + phải, trên + dưới)")]
    public Vector2 padding = new Vector2(40f, 20f); // (X = ngang, Y = dọc)

    private RectTransform panelRect;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (tmpText == null) return;

        // Tính kích thước text
        Vector2 preferredSize = new Vector2(
            tmpText.preferredWidth,
            tmpText.preferredHeight
        );

        // Resize Panel: preferredSize + padding
        panelRect.sizeDelta = preferredSize + padding;
    }
}

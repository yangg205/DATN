using UnityEngine;
using UnityEngine.UI;

public class SkillConnection : MonoBehaviour
{
    public RectTransform startNode;  // nút skill bắt đầu
    public RectTransform endNode;    // nút skill kết thúc
    private Image lineImage;
    public GameObject skillGameObj;

    void Awake()
    {
 
        lineImage = GetComponent<Image>();
        if (lineImage == null)
        {
            lineImage = gameObject.AddComponent<Image>();
        }
        lineImage.color = Color.white;  // Màu dây nối
    }

    void Update()
    {
        if (startNode == null || endNode == null) return;

        Vector3 startPos = startNode.position;
        Vector3 endPos = endNode.position;

        Vector3 differenceVector = endPos - startPos;

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(differenceVector.magnitude, 5f); // độ dày dây là 5

        rt.pivot = new Vector2(0, 0.5f);
        rt.position = startPos;
        float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);
    }
}

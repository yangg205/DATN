using UnityEngine;

public class CompassScroll : MonoBehaviour
{
    public RectTransform compassContainer; // GameObject BR
    public Transform player;               // Camera hoặc Player transform

    public float pixelsPerDegree = 5f;     // Tùy vào độ dài thanh chữ hướng

    private float compassWidth;

    void Start()
    {
        compassWidth = compassContainer.rect.width;
    }

    void Update()
    {
        float playerYRotation = player.eulerAngles.y;
        float xOffset = -playerYRotation * pixelsPerDegree;

        // Lặp la bàn bằng cách dùng mod (%)
        float wrappedX = xOffset % compassWidth;
        compassContainer.anchoredPosition = new Vector2(wrappedX, compassContainer.anchoredPosition.y);
    }
}

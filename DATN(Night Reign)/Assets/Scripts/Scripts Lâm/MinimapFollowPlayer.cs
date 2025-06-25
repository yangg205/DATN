using UnityEngine;

public class MinimapFollowPlayer : MonoBehaviour
{
    public Transform player;                 // Nhân vật thật
    public RectTransform playerIcon;         // Icon trên minimap
    public Camera minimapCamera;             // Camera minimap
    public Transform mainCamera;             // Gán Camera chính

    void Update()
    {
        if (player == null || playerIcon == null || minimapCamera == null || mainCamera == null)
            return;

        // Chuyển vị trí từ thế giới sang UI
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(player.position);
        RectTransform minimapRect = (RectTransform)playerIcon.parent;
        Vector2 minimapSize = minimapRect.rect.size;

        Vector2 minimapPos = new Vector2(
            viewportPos.x * minimapSize.x,
            viewportPos.y * minimapSize.y
        );

        playerIcon.anchoredPosition = minimapPos - minimapSize / 2f;

        // Xoay icon player theo hướng camera chính (đảo ngược trục Y)
        float cameraY = mainCamera.eulerAngles.y;
        playerIcon.localEulerAngles = new Vector3(0, 0, -cameraY);
    }
}

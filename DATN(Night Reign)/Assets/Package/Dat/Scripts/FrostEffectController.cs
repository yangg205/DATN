using UnityEngine;

public class FrostEffectRaycast_Tag : MonoBehaviour
{
    [Header("UI Overlay")]
    public CanvasGroup frostOverlay;

    [Header("Cold Settings")]
    public float timeToGetCold = 30f;
    public float fadeSpeed = 1.5f;
    public float raycastDistance = 2f;

    [Header("Detection")]
    public string snowTag = "Snow";  // Tag bạn đặt cho Terrain tuyết

    private float coldTimer = 0f;
    private bool onSnow = false;

    void Update()
    {
        // Raycast từ vị trí player xuống
        Vector3 origin = transform.position + Vector3.up * 0.2f;
        Ray ray = new Ray(origin, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            onSnow = hit.collider.CompareTag(snowTag);
        }
        else
        {
            onSnow = false;
        }

        // Tăng/giảm nhiệt độ lạnh
        coldTimer += (onSnow ? 1 : -2) * Time.deltaTime;
        coldTimer = Mathf.Clamp(coldTimer, 0, timeToGetCold);

        // Alpha UI theo mức độ lạnh
        float targetAlpha = Mathf.InverseLerp(0, timeToGetCold, coldTimer);
        frostOverlay.alpha = Mathf.Lerp(frostOverlay.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }

    // Hiển thị raycast khi chọn object trong Scene
    void OnDrawGizmosSelected()
    {
        Gizmos.color = onSnow ? Color.cyan : Color.gray;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.2f, transform.position + Vector3.down * raycastDistance);
    }
}

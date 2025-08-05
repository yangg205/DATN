using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    public string CheckpointID;
    public GameObject MinimapIcon;
    public GameObject VisualEffect;

    private void Start()
    {
        // Ẩn icon minimap khi bắt đầu nếu có
        if (MinimapIcon != null)
            MinimapIcon.SetActive(false);

        // Tắt hiệu ứng nếu có
        if (VisualEffect != null)
            VisualEffect.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Khi Player chạm vào checkpoint
        if (other.CompareTag("Player"))
        {
            // Hiện icon minimap
            if (MinimapIcon != null)
                MinimapIcon.SetActive(true);

            // Bật hiệu ứng
            if (VisualEffect != null)
                VisualEffect.SetActive(true);

            // Ghi log để debug nếu cần
            Debug.Log("Checkpoint reached: " + CheckpointID);

            // TODO: lưu trạng thái checkpoint nếu cần (sau này)
        }
    }
}

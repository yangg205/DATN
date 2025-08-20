using UnityEngine;

public class BossZoneController : MonoBehaviour
{
    private Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
        col.enabled = false; // mặc định tắt rào chắn
    }

    public void ActivateZone()
    {
        col.enabled = true; // bật rào chắn
        Debug.Log("Boss Zone Activated - Player không thể thoát!");
    }

    public void DeactivateZone()
    {
        col.enabled = false; // mở rào chắn
        Debug.Log("Boss Zone Deactivated - Player có thể thoát!");
    }
}

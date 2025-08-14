using UnityEngine;

public class BossZoneController : MonoBehaviour
{
    [Tooltip("Các barrier (collider) để bật khi zone activate).")]
    public GameObject[] barriers; // ví dụ tường invisible

    public void ActivateZone()
    {
        foreach (var go in barriers)
            if (go) go.SetActive(true);
    }

    public void DeactivateZone()
    {
        foreach (var go in barriers)
            if (go) go.SetActive(false);
    }
}

using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public string checkpointName;
    public Vector3 teleportPosition; // Vị trí dịch chuyển
    private bool activated;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !activated)
        {
            activated = true;
            MinimapManager.Instance.RegisterCheckpoint(this);
            Debug.Log("Checkpoint Activated: " + checkpointName);
        }
    }
}

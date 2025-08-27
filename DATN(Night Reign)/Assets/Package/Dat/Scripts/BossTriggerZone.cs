using UnityEngine;

public class BossTriggerZone : MonoBehaviour
{
    public AudioClip bossMusic;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            if (MusicManager.Instance != null && bossMusic != null)
            {
                MusicManager.Instance.PlayMusic(bossMusic, true);
            }
        }
    }
}

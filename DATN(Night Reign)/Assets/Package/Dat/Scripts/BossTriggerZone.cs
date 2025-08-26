using UnityEngine;

public class BossTriggerZone : MonoBehaviour
{
    public AudioClip bossMusic; // Kéo file nhạc vào đây trong Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // chỉ Player mới kích hoạt
        {
            MusicManager.Instance.PlayMusic(bossMusic, true);
            Debug.Log("Boss music started!");
        }
    }
}

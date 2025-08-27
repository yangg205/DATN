using UnityEngine;

public class EnemyDesQuest : MonoBehaviour
{
    private QuestManager quest;

    [Header("Boss Settings")]
    public bool isBoss = false;
    public AudioClip bossMusic;

    void Start()
    {
        quest = FindAnyObjectByType<QuestManager>();
        if (quest == null)
        {
            Debug.LogError("QuestManager not found in the scene.");
            return;
        }
    }

    private void OnDestroy()
    {
        if (quest != null)
        {
            quest.ReportKill();
        }

        // Nếu là boss thì fade out nhạc boss
        if (isBoss && MusicManager.Instance != null)
        {
            if (bossMusic == null || MusicManager.Instance.CurrentClip == bossMusic)
            {
                MusicManager.Instance.FadeOutMusic(2f);
            }
        }
    }
}

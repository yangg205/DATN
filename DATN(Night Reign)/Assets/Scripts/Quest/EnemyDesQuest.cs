using UnityEngine;

public class EnemyDesQuest : MonoBehaviour
{
    private QuestManager quest;

    [Header("Boss Settings")]
    public bool isBoss = false;
    public AudioClip bossMusic;

    // flag để phân biệt despawn
    [HideInInspector] public bool isDespawned = false;

    void Start()
    {
        quest = FindAnyObjectByType<QuestManager>();
        if (quest == null)
        {
            Debug.LogError("QuestManager not found in the scene.");
        }
    }

    private void OnDestroy()
    {
        // Nếu bị despawn thì bỏ qua, không báo quest
        if (isDespawned) return;

        if (quest != null)
            quest.ReportKill();

        if (isBoss && MusicManager.Instance != null)
        {
            if (bossMusic == null || MusicManager.Instance.CurrentClip == bossMusic)
                MusicManager.Instance.FadeOutMusic(2f);
        }
    }
}

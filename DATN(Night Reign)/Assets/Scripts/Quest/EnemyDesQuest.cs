using UnityEngine;

public class EnemyDesQuest : MonoBehaviour
{
    private QuestManager quest;
    private BattleBoss battleBoss;

    [Header("Boss Settings")]
    public bool isBoss = false;
    public AudioClip bossMusic;

    // flag để phân biệt despawn
    [HideInInspector] public bool isDespawned = false;

    [Header("BattleBoss Settings")]
    public int playercharacterId = 0;          // sẽ gán động khi spawn
    public int bossId = 0;            // id boss
    public double maxFightTime = 300; // thời gian tối đa

    void Start()
    {
        quest = FindAnyObjectByType<QuestManager>();
        battleBoss = FindAnyObjectByType<BattleBoss>();
        playercharacterId = PlayerPrefs.GetInt("PlayerCharacterId", 0);
        if (quest == null)
            Debug.LogError("QuestManager not found in the scene.");

        if (isBoss && battleBoss != null)
        {
            // Boss spawn → bắt đầu trận
            battleBoss.StartFight(playercharacterId, bossId, maxFightTime);
            Debug.LogError("Boss fight started!");
            Debug.LogError(battleBoss.isFighting);
        }
    }

    private void OnDestroy()
    {
        // Nếu bị despawn thì bỏ qua, không báo quest
        if (isDespawned) return;

        if (quest != null)
            quest.ReportKill();

        if (isBoss)
        {
            if (battleBoss != null)
            {
                battleBoss.OnBossDeath(); 
            }

            if (MusicManager.Instance != null)
            {
                if (bossMusic == null || MusicManager.Instance.CurrentClip == bossMusic)
                    MusicManager.Instance.FadeOutMusic(2f);
            }
        }
    }
}

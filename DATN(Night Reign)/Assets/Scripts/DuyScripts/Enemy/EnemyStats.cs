using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int healthLevel = 10;
    public int maxHealth;
    public int currentHealth;

    public int expReward = 25;
    public PlayerStats playerStats;

    Animator animator;

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        animator = GetComponent<Animator>();
    }
    void Start()
    {
        maxHealth = SetMaxHealthFromHealthLevel();
        currentHealth = maxHealth;
    }

    private int SetMaxHealthFromHealthLevel()
    {
        maxHealth = healthLevel * 10;
        return maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth = currentHealth - damage;

        animator.Play("DamageHit");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            animator.Play("Dead");
            //handle dead 
            if (playerStats != null)
            {
                playerStats.GainEXP(expReward);
            }
            Destroy(gameObject, 2f); // Xoá quái sau 2 giây

        }
    }
}

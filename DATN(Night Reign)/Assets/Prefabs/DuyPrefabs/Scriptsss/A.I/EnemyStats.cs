using UnityEngine;

namespace AG
{
    public class EnemyStats : CharacterStats
    {
        EnemyAnimatorManager enemyAnimatorManager;

        public UIEnemyHealthBar enemyHealthBar;

        public int soulAwardedOnDeath = 50;
        private void Awake()
        {
            enemyAnimatorManager = GetComponentInChildren<EnemyAnimatorManager>();
        }
        void Start()
        {
            maxHealth = SetMaxHealthFromHealthLevel();
            currentHealth = maxHealth;
            enemyHealthBar.SetMaxHealth(maxHealth);
        }

        private int SetMaxHealthFromHealthLevel()
        {
            maxHealth = healthLevel * 10;
            return maxHealth;
        }

        public void TakeDamageNoAnimation(int damage)
        {
            currentHealth = currentHealth - damage;
            enemyHealthBar.SetHealth(currentHealth);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDead = true;
            }
        }

        public override void TakeDamage(int damage, string damageAnimation = "Damage_Hit")
        {
            if (isDead)
                return;

            currentHealth = currentHealth - damage;
            enemyHealthBar.SetHealth(currentHealth);

            enemyAnimatorManager.PlayTargetAnimation(damageAnimation, true);

            if (currentHealth <= 0)
            {
               HandleDeath();
            }
        }

        private void HandleDeath()
        {
            currentHealth = 0;
            enemyAnimatorManager.PlayTargetAnimation("Death", true);
            isDead = true;
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();

            if(playerStats != null)
            {
                playerStats.AddSouls(soulAwardedOnDeath);
            }
        }
    }
}


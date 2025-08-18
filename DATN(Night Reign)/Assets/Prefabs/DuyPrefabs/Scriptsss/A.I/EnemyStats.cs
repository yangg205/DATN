using UnityEngine;

namespace AG
{
    public class EnemyStats : CharacterStats
    {
        Animator animator;
        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
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
            if (isDead)
                return;

            currentHealth = currentHealth - damage;

            animator.Play("Damage_Hit");

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                animator.Play("Death");
                isDead = true;
            }
        }
    }
}


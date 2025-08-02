using UnityEngine;

namespace ND
{
    public class DamageCollider : MonoBehaviour
    {
        Collider damageCollider;
        public int currentWeaponDamage = 25;

        private void Awake()
        {
            damageCollider = GetComponent<Collider>();
            damageCollider.gameObject.SetActive(true);
            damageCollider.isTrigger = true;
            damageCollider.enabled = false;
        }

        public void EnableDamageCollider()
        {
            damageCollider.enabled = true;
        }

        public void DisableDamageCollider()
        {
            damageCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider collision)
        {
            // Check nếu đối tượng có thể parry (PlayerManager)
            PlayerManager playerManager = collision.GetComponent<PlayerManager>();
            if (playerManager != null && playerManager.isParrying)
            {
                AnimatorHandler animHandler = playerManager.GetComponentInChildren<AnimatorHandler>();
                animHandler?.PlayTargetAnimation("Parry", true);
                return; // Đỡ đòn thành công, không nhận damage
            }

            // ===== PLAYER =====
            if (collision.CompareTag("Player"))
            {
                PlayerStats playerStats = collision.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(currentWeaponDamage);
                }
                return;
            }

            // ===== HITTABLE =====
            if (collision.CompareTag("Hittable"))
            {
                PlayerStats playerStats = collision.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(currentWeaponDamage);
                }
                return;
            }

            // ===== ENEMIES =====
            switch (collision.tag)
            {
                case "DragonSoulEater":
                    DragonSoulEater enemyStats = collision.GetComponent<DragonSoulEater>();
                    if (enemyStats != null)
                        enemyStats.TakeDamage(currentWeaponDamage);
                    break;

                case "DragonNightMare":
                    DragonTerrorBringer terrorBringer = collision.GetComponent<DragonTerrorBringer>();
                    if (terrorBringer != null)
                    {
                        terrorBringer.TakeDamage(currentWeaponDamage);
                    }
                    else
                    {
                        NightMare nightmare = collision.GetComponent<NightMare>();
                        if (nightmare != null)
                            nightmare.TakeDamage(currentWeaponDamage);
                    }
                    break;

                case "BossTutorial":
                    EnemyAIByYang boss = collision.GetComponent<EnemyAIByYang>();
                    if (boss != null)
                        boss.TakeDamage(currentWeaponDamage);
                    break;

                default:
                    break;
            }
        }
    }
}

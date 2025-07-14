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
            if (collision.tag == "Hittable")
            {
                PlayerStats playerStats = collision.GetComponent<PlayerStats>();

                if (playerStats != null)
                {
                    playerStats.TakeDamage(currentWeaponDamage);
                }
            }

            //Duyen
            switch (collision.tag)
            {
                case "DragonSoulEater":
                    DragonSoulEater enemyStats = collision.GetComponent<DragonSoulEater>();
                    if (enemyStats != null)
                    {
                        enemyStats.TakeDamage(currentWeaponDamage);
                    }
                    break;

                case "DragonNightMare":
                    DragonTerrorBringer anotherStats = collision.GetComponent<DragonTerrorBringer>();
                    if (anotherStats != null)
                    {
                        anotherStats.TakeDamage(currentWeaponDamage);
                    }
                    else
                    {
                        NightMare anotherStatss = collision.GetComponent<NightMare>();
                        {
                            if (anotherStatss != null)
                            {
                                anotherStatss.TakeDamage(currentWeaponDamage);
                            }
                        }
                    }

                    break;
                // Thêm các tag khác tại đây
                default:
                    // Không làm gì nếu tag không khớp
                    break;
            }
        }
    }
}

using Unity.Cinemachine;
using UnityEngine;

namespace AG
{
    public class SpellDamageCollider : DamageCollider
    {
        public GameObject impactParticles;
        public GameObject projectileParticles;
        public GameObject muzzleParticles;

        bool hasCollided = false;

        CharacterStats spellTarget;

        Vector3 impactNormal;

        private void Start()
        {
            projectileParticles = Instantiate(projectileParticles, transform.position, transform.rotation);
            projectileParticles.transform.parent = transform;

            if (muzzleParticles)
            {
                muzzleParticles = Instantiate(muzzleParticles, transform.position, transform.rotation);
                Destroy(muzzleParticles, 2f);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!hasCollided)
            {
                CharacterStats spellTarget = other.transform.GetComponent<CharacterStats>();
                DragonSoulEater dragonSoulEater = other.transform.GetComponent<DragonSoulEater>();
                DragonUsurper dragonUsurper = other.transform.GetComponent<DragonUsurper>();
                NightMare nightmare = other.transform.GetComponent<NightMare>();

                if (spellTarget != null)
                {
                    spellTarget.TakeDamage(currentWeaponDamage);
                }
                else if (dragonSoulEater != null)
                {
                    dragonSoulEater.TakeDamage(currentWeaponDamage);
                }
                else if (dragonUsurper != null)
                {
                    dragonUsurper.TakeDamage(currentWeaponDamage);
                }
                else if (nightmare != null)
                {
                    nightmare.TakeDamage(currentWeaponDamage);
                }

                hasCollided = true;
                impactParticles = Instantiate(impactParticles, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal));

                Destroy(projectileParticles);
                Destroy(impactParticles);
                Destroy(gameObject);
                Debug.Log("đã destroy");
            }
        }
    }
}






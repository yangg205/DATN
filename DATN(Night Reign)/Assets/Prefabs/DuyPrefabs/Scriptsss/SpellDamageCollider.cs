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

                if (spellTarget != null)
                {
                    spellTarget.TakeDamage(currentWeaponDamage);
                }

                hasCollided = true;
                impactParticles = Instantiate(impactParticles, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal));

                Destroy(projectileParticles);
                Destroy(impactParticles);
                Destroy(gameObject);
            }
        }
    }
}






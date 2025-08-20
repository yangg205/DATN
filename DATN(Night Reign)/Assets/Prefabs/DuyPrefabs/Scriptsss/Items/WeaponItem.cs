using UnityEngine;

namespace AG
{
    [CreateAssetMenu(menuName ="Items/Weapon Item")]
    public class WeaponItem : Item
    {
        public GameObject modelPrefab;
        public bool isUnarmed;

        [Header("Damage")]
        public int baseDamage = 25;
        public int criticalDamageMultiplier = 4;

        [Header("Absorption")]
        public float physicalDamageAbsorption;

        [Header("Idle Animations")]
        public string right_hand_idle;
        public string left_hand_idle;
        public string th_idle;

        [Header("One Hand Attack Animation")]
        public string Oh_Light_Attack_1;
        public string Oh_Light_Attack_2;
        public string Oh_Light_Attack_3;
        public string Oh_Heavy_Attack_1;
        public string Oh_Heavy_Attack_2;

        [Header("Two Hand Attack Animation")]
        public string Th_Light_Attack_1;
        public string Th_Light_Attack_2;
        public string Th_Light_Attack_3;
        public string Th_Heavy_Attack_1;

        [Header("Weapon Art")]
        public string weapon_art;

        [Header("Stamina Costs")]
        public int baseStamina;
        public float lightAttackMultiplier;
        public float heavyAttackMultiplier;

        [Header("Weapon Type")]
        public bool isSpellCaster;
        public bool isFaithCaster;
        public bool isPyroCaster;
        public bool isMeleeWeapon;
        public bool isShieldWeapon;
    }
}


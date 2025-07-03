using UnityEngine;


namespace ND
{
    [CreateAssetMenu(menuName = "Items/Weapon Item")]
    public class WeaponItem : Item
    {
        public GameObject modelPrefab;
        public bool isUnarmed;

        [Header("Idle Animations")]
        public string right_hand_idle;
        public string left_hand_idle;
        public string th_idle;

        [Header("Attack Animations")]
        public string Oh_Light_Attack_1;
        public string Oh_Light_Attack_2;
        public string Oh_Light_Attack_3;
        public string Oh_Light_Attack_4;
        public string Oh_Heavy_Attack_1;
        public string Oh_Heavy_Attack_2;

        [Header("Stamina Costs")]
        public int baseStamina;
        public float lightAttackMultiplier;
        public float heavyAttackMultiplier;

        [Header("VFX Effects")]
        public GameObject lightAttackVFX;
        public GameObject heavyAttackVFX;
        public GameObject weaponTrailVFX;

        [Header("Skill")]
        public GameObject specialSkillVFX;

        [Header("Skill Animation")]
        public string skill_Charge;
        public string skill_Attack_01;
        public string skill_Attack_02;
    }
}


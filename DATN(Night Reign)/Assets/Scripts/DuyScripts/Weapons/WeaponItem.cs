using UnityEngine;


namespace ND
{
    [CreateAssetMenu(menuName = "Items/Weapon Item")]
    public class WeaponItem : Item
    {
        [Header("Model")]
        public GameObject modelPrefab;
        public bool isUnarmed;
        public bool isBow;

        [Header("Bow and Arrow")]
        public GameObject arrowPrefab;         // Prefab mũi tên
        public Transform arrowSpawnPoint;     // Điểm spawn mũi tên (trong prefab của cung)

        [Header("Idle Animations")]
        public string right_hand_idle;
        public string left_hand_idle;
        public string th_idle;

        [Header("Attack Animations - Light")]
        public string Oh_Light_Attack_1;
        public string Oh_Light_Attack_2;
        public string Oh_Light_Attack_3;
        public string Oh_Light_Attack_4;
        public string Oh_Th_Attack_1;
        public string Oh_Th_Attack_2;

        [Header("Attack Animations - Heavy")]
        public string Oh_Heavy_Attack_1;
        public string Oh_Heavy_Attack_2;

        [Header("Skill Animations")]
        public string skill_Charge;         // Animation khi gồng (Q)
        public string skill_Attack_01;      // Animation đòn 1
        public string skill_Attack_02;      // Animation đòn 2
        public string skill_BuffCharge;     // Animation khi buff tốc đánh (nếu có skill E chẳng hạn)

        [Header("Stamina Costs")]
        public int baseStamina = 10;
        public float lightAttackMultiplier = 1.0f;
        public float heavyAttackMultiplier = 1.5f;

        [Header("Weapon Type")]
        public bool isSpellCaster;
        public bool isFaithCaster;
        public bool isPyroCaster;
        public bool isMeleeWeapon;
    }
}


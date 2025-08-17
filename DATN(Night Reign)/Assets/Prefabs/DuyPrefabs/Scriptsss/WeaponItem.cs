using UnityEngine;

namespace AG
{
    [CreateAssetMenu(menuName ="Items/Weapon Item")]
    public class WeaponItem : Item
    {
        public GameObject modelPrefab;
        public bool isUnarmed;

        [Header("Idle Animations")]
        public string right_hand_idle;
        public string left_hand_idle;

        [Header("One Hand Attack Animation")]
        public string Oh_Light_Attack_1;
        public string Oh_Light_Attack_2;
        public string Oh_Light_Attack_3;
        public string Oh_Heavy_Attack_1;
        public string Oh_Heavy_Attack_2;

        [Header("Stamina Costs")]
        public int baseStamina;
        public float lightAttackMultiplier;
        public float heavyAttackMultiplier;
    }
}


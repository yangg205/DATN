using TMPro;
using UnityEngine;

namespace AG
{
    public class CharacterStats : MonoBehaviour
    {
        public int healthLevel = 10;
        public int maxHealth;
        public int currentHealth;

        public int staminaLevel = 10;
        public float maxStamina;
        public float currentStamina;

        public int focusLevel = 10;
        public float maxFocusPoints;
        public float currentFocusPoints;

        public TextMeshProUGUI levelText;
        public int playerLevel = 1;
        public int currentEXP = 0;
        public int expToNextLevel = 100;

        public int soulCount = 0;

        public bool isDead;

        public virtual void TakeDamage(int damage, string damageAnimation = "Damage_Hit")
        {

        }

    }
}


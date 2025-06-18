using TMPro;
using UnityEngine;

namespace ND
{
    public class PlayerStats : MonoBehaviour
    {
        public int healthLevel = 10;
        public int maxHealth;
        public int currentHealth;

        public int staminaLevel = 10;
        public int maxStamina;
        public int currentStamina;

        public int playerLevel = 1;
        public int currentEXP = 0;
        public int expToNextLevel = 100;

        public HealthBar healthBar;
        public StaminaBar staminaBar;
        public ExpBar expBar;
        public TextMeshProUGUI levelText;

        AnimatorHandler animatorHandler;
        SignalRClient signalRClient;

        private void Awake()
        {
            healthBar = FindFirstObjectByType<HealthBar>();
            staminaBar = FindFirstObjectByType<StaminaBar>();
            expBar = FindFirstObjectByType<ExpBar>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();


        }
        void Start()
        {
            maxHealth = SetMaxHealthFromHealthLevel();
            currentHealth = maxHealth;
            healthBar.SetMaxHealth(maxHealth);

            maxStamina = SetMaxStaminaFromStaminaLevel();
            currentStamina = maxStamina;

            expBar.SetMaxEXP(expToNextLevel);
            UpdateLevelText();
        }

        private int SetMaxHealthFromHealthLevel()
        {
            maxHealth = healthLevel * 10;
            return maxHealth;
        }

        private int SetMaxStaminaFromStaminaLevel()
        {
            maxStamina = staminaLevel * 10;
            return maxStamina;
        }
        public void TakeDamage(int damage)
        {
            currentHealth = currentHealth - damage;

            healthBar.SetCurrenHealth(currentHealth);

            animatorHandler.PlayTargetAnimation("DamageHit", true);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                animatorHandler.PlayTargetAnimation("Dead", true);
                //handle dead 
            }
        }

        public void TakeStaminaDamage(int damage)
        {
            currentStamina = currentStamina - damage;
            staminaBar.SetCurrenStamina(currentStamina);
        }

        public void GainEXP(int amount)
        {
            currentEXP += amount;
            CheckLevelUp();
            expBar.SetCurrentEXP(currentEXP);
        }

        private void CheckLevelUp()
        {
            while (currentEXP >= expToNextLevel)
            {
                currentEXP -= expToNextLevel;
                LevelUp();
            }
        }
        private void LevelUp()
        {
            playerLevel++;
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.25f);
            expBar.SetMaxEXP(expToNextLevel);
            expBar.SetCurrentEXP(currentEXP);
            UpdateLevelText();

        }

        private void UpdateLevelText()
        {
            if (levelText != null)
            {
                levelText.text = "Level: " + playerLevel;
            }
        }
    }
}

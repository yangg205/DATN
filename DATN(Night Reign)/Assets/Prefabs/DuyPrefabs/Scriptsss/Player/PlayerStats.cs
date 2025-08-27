using TMPro;
using UnityEngine;

namespace AG
{
    public class PlayerStats : CharacterStats
    {
        PlayerManager playerManager;

        public ExpBar expBar;
        public HealthBar healthBar;
        public FocusPointBar focusPointsBar;
        public StaminaBar staminaBar;
        PlayerAnimatorManager animatorHandler;

        public float staminaRegenerationAmount = 1;
        public float staminaRegenTimer = 0;
        private void Awake()
        {
            playerManager = GetComponent<PlayerManager>();

            healthBar = FindObjectOfType<HealthBar>();
            staminaBar = FindObjectOfType<StaminaBar>();
            focusPointsBar = FindObjectOfType<FocusPointBar>();
            expBar = FindObjectOfType<ExpBar>();
            animatorHandler = GetComponentInChildren<PlayerAnimatorManager>();
        }
        void Start()
        {
            maxHealth = SetMaxHealthFromHealthLevel();
            currentHealth = maxHealth;
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetCurrentHealth(currentHealth);

            maxStamina = SetMaxStaminaFromStaminaLevel();
            currentStamina = maxStamina;
            staminaBar.SetMaxStamina(maxStamina);
            staminaBar.SetCurrentStamina(currentStamina);

            maxFocusPoints = SetMaxFocusPointsFromFocusLevel();
            currentFocusPoints = maxFocusPoints;
            focusPointsBar.SetMaxFocusPoints(maxFocusPoints);
            focusPointsBar.SetCurrentFocusPoint(currentFocusPoints);

            expBar.SetMaxEXP(expToNextLevel);
            UpdateLevelText();
        }

        private int SetMaxHealthFromHealthLevel()
        {
            maxHealth = healthLevel * 10;
            return maxHealth;
        }

        private float SetMaxFocusPointsFromFocusLevel()
        {
            maxFocusPoints = focusLevel * 10;
            return maxFocusPoints;
        }    

        private float SetMaxStaminaFromStaminaLevel()
        {
            maxStamina = staminaLevel * 10;
            return maxStamina;
        }

        public void TakeDamageNoAnimation(int damage)
        {
            currentHealth = currentHealth - damage;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDead = true;
            }
        }
        public override void TakeDamage(int damage, string damageAnimation = "Damage_Hit")
        {
            if (playerManager.isInvulnerable)
                return;

            if (isDead)
                return;

            currentHealth = currentHealth - damage;

            healthBar.SetCurrentHealth(currentHealth);

            animatorHandler.PlayTargetAnimation(damageAnimation, true);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                animatorHandler.PlayTargetAnimation("Death", true);
                isDead = true;
            }
        }

        public void TakeStaminaDamage(int damage)
        {
            currentStamina = currentStamina - damage;
            staminaBar.SetCurrentStamina(currentStamina);
        }

        public void RegenerateStamina()
        {
            if (playerManager.isInteracting)
            {
                staminaRegenTimer = 0;
            }
            else
            {
                staminaRegenTimer += Time.deltaTime;

                if (currentStamina < maxStamina && staminaRegenTimer > 1f)
                {
                    currentStamina += staminaRegenerationAmount * Time.deltaTime;
                    staminaBar.SetCurrentStamina(Mathf.RoundToInt(currentStamina));
                }
            }
        }

        public void HealPlayer(int healAmount)
        {
            currentHealth = currentHealth + healAmount;

            if(currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }    

            healthBar.SetCurrentHealth(currentHealth);
        }    

        public void DeductFocusPoints(int focusPoints)
        {
            currentFocusPoints = currentFocusPoints - focusPoints;

            if(currentFocusPoints < 0)
            {
                currentFocusPoints = 0;
            }    

            focusPointsBar.SetCurrentFocusPoint(currentFocusPoints);
        }    

        public void AddSouls(int souls)
        {
            soulCount = soulCount + souls;
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



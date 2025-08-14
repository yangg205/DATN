using System.Collections;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

namespace ND
{
    public class PlayerStats : MonoBehaviour
    {
        public float staminaRegenerationAmount = 10;
        public float staminaRegenTimer = 0;

        public float baseAttackSpeed = 1f;
        public float currentAttackSpeed = 1f;

        public int healthLevel = 10;
        public int maxHealth;
        public int currentHealth;

        public int staminaLevel = 10;
        public float maxStamina;
        public float currentStamina;

        public int playerLevel = 1;
        public int currentEXP = 0;
        public int expToNextLevel = 100;

        public int focusLevel = 10;
        public float maxFocusPoint;
        public float currentFocusPoint;

        public int soulCount = 0;

        public HealthBar healthBar;
        public StaminaBar staminaBar;
        public FocusPointBar focusPointBar;
        public ExpBar expBar;
        public TextMeshProUGUI levelText;

        PlayerManager playerManager;
        AnimatorHandler animatorHandler;

        public bool isDead;

        private void Awake()
        {
            playerManager = GetComponent<PlayerManager>();
            healthBar = FindFirstObjectByType<HealthBar>();
            staminaBar = FindFirstObjectByType<StaminaBar>();
            focusPointBar = FindFirstObjectByType<FocusPointBar>();
            expBar = FindFirstObjectByType<ExpBar>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
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

            maxFocusPoint = SetMaxFocusPointFromFocusLevel();
            currentFocusPoint = maxFocusPoint;  
            focusPointBar.SetMaxFocusPoint(maxFocusPoint);
            focusPointBar.SetCurrentFocusPoint(currentFocusPoint);

            expBar.SetMaxEXP(expToNextLevel);
            UpdateLevelText();
        }

        private int SetMaxHealthFromHealthLevel()
        {
            maxHealth = healthLevel * 10;
            return maxHealth;
        }

        private float SetMaxStaminaFromStaminaLevel()
        {
            maxStamina = staminaLevel * 10;
            return maxStamina;
        }

        private float SetMaxFocusPointFromFocusLevel()
        {
            maxFocusPoint = focusLevel * 10;
            return maxFocusPoint;
        }

        public void TakeDamage(int damage)
        {
            if (playerManager.isInvulnerable || isDead)
                return;

            currentHealth -= damage;
            healthBar.SetCurrentHealth(currentHealth);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                animatorHandler.PlayTargetAnimation("Dead", true);
                isDead = true;

                StartCoroutine(DeathCoroutine()); // Gọi coroutine destroy
            }
            else
            {
                animatorHandler.PlayTargetAnimation("DamageHit", true);
            }
        }

        public void TakeStaminaDamage(float damage)
        {
            currentStamina -= damage;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            staminaBar.SetCurrentStamina(Mathf.RoundToInt(currentStamina));
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

        public void RegenerateStamina()
        {
            if(playerManager.isInteracting || playerManager.isSprinting)
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

        public void DeductFocusPoint(int focusPoint)
        {
            currentFocusPoint = currentFocusPoint - focusPoint;
            
            if(currentFocusPoint < 0)
            {
                currentFocusPoint = 0;
            }

            focusPointBar.SetCurrentFocusPoint(currentFocusPoint);
        }

        public void AddSouls(int souls)
        {
            soulCount = soulCount + souls;
        }

        private IEnumerator DeathCoroutine()
        {
            yield return new WaitForSeconds(3f); // Chờ 3 giây cho animation "Dead" chạy

            // Gọi notify cho các hệ thống khác nếu cần (UI, GameManager, Enemy AI...)
            // Ví dụ: GameManager.Instance.OnPlayerDeath();

            Destroy(gameObject); // Xoá Player sau khi chết
        }
        public void RespawnAt(Vector3 location)
        {
            currentHealth = maxHealth;
            currentStamina = maxStamina;
            transform.position = location;
        }
        public int GetPlayerCharacterId()
        {
            return PlayerPrefs.GetInt("PlayerCharacterId", 0);
        }
    }
}

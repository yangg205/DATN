using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int healthLevel = 10;
    public int maxHealth;
    public int currentHealth;

    public int staminaLevel = 10;
    public int maxStamina;
    public int currentStamina;

    public HealthBar healthBar;
    public StaminaBar staminaBar;

    AnimatorHandler animatorHandler;

    private void Awake()
    {
        healthBar = FindFirstObjectByType<HealthBar>();
        staminaBar = FindFirstObjectByType<StaminaBar>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();

    }
    void Start()
    {
        maxHealth = SetMaxHealthFromHealthLevel();
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        maxStamina = SetMaxStaminaFromStaminaLevel();
        currentStamina = maxStamina;
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

        if(currentHealth <= 0)
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
}

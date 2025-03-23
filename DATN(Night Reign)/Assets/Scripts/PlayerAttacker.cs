using UnityEngine;

public class PlayerAttacker : MonoBehaviour
{
    AnimatorHandler animatorHandler;
    private void Awake()
    {
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
    }
    public void HandleLightAttack(WeaponItem weapon)
    {
        animatorHandler.PlayTargetAnimation(weapon.Light_Attack, true);
    }

    public void HandleHeavyAttack(WeaponItem weapon)
    {
        animatorHandler.PlayTargetAnimation(weapon.Heavy_Attack, true);

    }
}

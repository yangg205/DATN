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
        animatorHandler.PlayTargetAnimation(weapon.Oh_Light_Attack_1, true);
    }
    public void HandleHeavyAttack(WeaponItem weapon)
    {
        animatorHandler.PlayTargetAnimation(weapon.Oh_Heavy_Attack_1, true);
    }
}

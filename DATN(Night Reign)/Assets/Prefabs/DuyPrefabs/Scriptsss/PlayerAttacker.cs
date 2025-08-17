using UnityEngine;

namespace AG
{
    public class PlayerAttacker : MonoBehaviour
    {
        AnimatorHandler animationHandler;
        InputHandler inputHandler;
        WeaponSlotManager weaponSlotManager;
        public string lastAttack;

        private void Awake()
        {
            animationHandler = GetComponentInChildren<AnimatorHandler>();
            inputHandler = GetComponent<InputHandler>();
            weaponSlotManager = GetComponentInChildren<WeaponSlotManager>();
        }
        public void HandleWeaponCombo(WeaponItem weapon)
        {
            if(inputHandler.comboFlag)
            {
                animationHandler.anim.SetBool("canDoCombo", false);
                if (lastAttack == weapon.Oh_Light_Attack_1)
                {
                    animationHandler.PlayTargetAnimation(weapon.Oh_Light_Attack_2, true);
                    lastAttack = weapon.Oh_Light_Attack_2;
                }
                else if(lastAttack == weapon.Oh_Light_Attack_2)
                {
                    animationHandler.PlayTargetAnimation(weapon.Oh_Light_Attack_3, true);
                    lastAttack = weapon.Oh_Light_Attack_3;
                }
                else if (lastAttack == weapon.Oh_Heavy_Attack_1)
                {
                    animationHandler.PlayTargetAnimation(weapon.Oh_Heavy_Attack_2, true);
                    lastAttack = weapon.Oh_Heavy_Attack_2;
                }
            }
        }
        public void HandleLightAttack(WeaponItem weapon)
        {
            weaponSlotManager.attackingWeapon = weapon;
            animationHandler.PlayTargetAnimation(weapon.Oh_Light_Attack_1, true);
            lastAttack = weapon.Oh_Light_Attack_1;
        }

        public void HandleHeavyAttack(WeaponItem weapon)
        {
            weaponSlotManager.attackingWeapon = weapon;
            animationHandler.PlayTargetAnimation(weapon.Oh_Heavy_Attack_1, true);
            lastAttack = weapon.Oh_Heavy_Attack_1;
        }
    }
}


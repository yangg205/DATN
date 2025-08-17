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

                //One Hand (Light Attack)
                if (lastAttack == weapon.Oh_Light_Attack_1)
                {
                    animationHandler.PlayTargetAnimation(weapon.Oh_Light_Attack_2, true);
                    lastAttack = weapon.Oh_Light_Attack_2;
                }
                else if (lastAttack == weapon.Oh_Light_Attack_2)
                {
                    animationHandler.PlayTargetAnimation(weapon.Oh_Light_Attack_3, true);
                    lastAttack = weapon.Oh_Light_Attack_3;
                }

                //One Hand (Heavy Attack)
                else if (lastAttack == weapon.Oh_Heavy_Attack_1)
                {
                    animationHandler.PlayTargetAnimation(weapon.Oh_Heavy_Attack_2, true);
                    lastAttack = weapon.Oh_Heavy_Attack_2;
                }

                //Two Hand (Light Attack)
                else if (lastAttack == weapon.Th_Light_Attack_1)
                {
                    animationHandler.PlayTargetAnimation(weapon.Th_Light_Attack_2, true);
                    lastAttack = weapon.Th_Light_Attack_2;
                }
                else if (lastAttack == weapon.Th_Light_Attack_2)
                {
                    animationHandler.PlayTargetAnimation(weapon.Th_Light_Attack_3, true);
                    lastAttack = weapon.Th_Light_Attack_3;
                }
                // nếu sau này có heavy attack 2 tay thì bổ sung ở đây
                // else if (lastAttack == weapon.Th_Heavy_Attack_1) { ... }
            }
        }
        public void HandleLightAttack(WeaponItem weapon)
        {
            weaponSlotManager.attackingWeapon = weapon;

            if (inputHandler.twoHandFlag)
            {
                animationHandler.PlayTargetAnimation(weapon.Th_Light_Attack_1, true);
                lastAttack = weapon.Th_Light_Attack_1;
            }
            else
            {
                animationHandler.PlayTargetAnimation(weapon.Oh_Light_Attack_1, true);
                lastAttack = weapon.Oh_Light_Attack_1;
            }
        }

        public void HandleHeavyAttack(WeaponItem weapon)
        {
            weaponSlotManager.attackingWeapon = weapon;

            if (inputHandler.twoHandFlag)
            {
                animationHandler.PlayTargetAnimation(weapon.Th_Heavy_Attack_1, true);
                lastAttack = weapon.Th_Heavy_Attack_1;
                // Sau này nếu bạn có Th_Heavy_Attack_1 thì thêm logic tại đây
                // animationHandler.PlayTargetAnimation(weapon.Th_Heavy_Attack_1, true);
                // lastAttack = weapon.Th_Heavy_Attack_1;
            }
            else
            {
                animationHandler.PlayTargetAnimation(weapon.Oh_Heavy_Attack_1, true);
                lastAttack = weapon.Oh_Heavy_Attack_1;
            }
        }
    }
}


using ND;
using UnityEngine;

namespace AG
{
    public class PlayerAttacker : MonoBehaviour
    {
        AnimatorHandler animationHandler;
        PlayerManager playerManager;
        PlayerStats playerStats;
        PlayerInventory playerInventory;
        InputHandler inputHandler;
        WeaponSlotManager weaponSlotManager;
        public string lastAttack;

        private void Awake()
        {
            animationHandler = GetComponent<AnimatorHandler>();
            playerManager = GetComponentInParent<PlayerManager>();
            playerStats = GetComponentInParent<PlayerStats>();
            playerInventory = GetComponentInParent<PlayerInventory>();
            inputHandler = GetComponentInParent<InputHandler>();
            weaponSlotManager = GetComponent<WeaponSlotManager>();
        }
        public void HandleWeaponCombo(WeaponItem weapon)
        {
            if (inputHandler.comboFlag)
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

        #region Input Actions
        public void HandleRBAction()
        {
            if (playerInventory.rightWeapon.isMeleeWeapon)
            {
                PerformMeleeAction();
            }
            else if (playerInventory.rightWeapon.isSpellCaster || playerInventory.rightWeapon.isFaithCaster || playerInventory.rightWeapon.isPyroCaster)
            {
                PerformRBMagicAction(playerInventory.rightWeapon);
            }
        }
        #endregion

        #region Attack Actions
        private void PerformMeleeAction()
        {
            inputHandler.rb_input = false;

            if (playerManager.canDoCombo)
            {
                inputHandler.comboFlag = true;
                HandleWeaponCombo(playerInventory.rightWeapon);
                inputHandler.comboFlag = false;
            }
            else if (!playerManager.isInteracting)
            {
                animationHandler.anim.SetBool("isUsingRightHand", true);
                HandleLightAttack(playerInventory.rightWeapon);
            }
            else
            {
                inputHandler.queuedLightAttack = true;
            }
        }

        private void PerformRBMagicAction(WeaponItem weapon)
        {
            if(weapon.isFaithCaster)
            {
                if(playerInventory.currentSpell != null && playerInventory.currentSpell.isFaithSpell)
                {
                    playerInventory.currentSpell.AttemptToCastSpell(animationHandler, playerStats);
                }    
            }    
        }    

        private void SuccessfullyCastSpell()
        {
            playerInventory.currentSpell.SuccessfullyCastSpell(animationHandler, playerStats);
        }    
        #endregion
    }
}

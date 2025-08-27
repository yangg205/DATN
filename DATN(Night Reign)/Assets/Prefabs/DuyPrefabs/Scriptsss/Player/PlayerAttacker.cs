using ND;
using UnityEngine;

namespace AG
{
    public class PlayerAttacker : MonoBehaviour
    {
        [Header("Audio")]
        public AudioSource audioSource;

        CameraHandler cameraHandler;
        PlayerAnimatorManager animationHandler;
        PlayerEquipmentManager playerEquipmentManager;
        PlayerManager playerManager;
        PlayerStats playerStats;
        PlayerInventory playerInventory;
        InputHandler inputHandler;
        WeaponSlotManager weaponSlotManager;
        public string lastAttack;

        LayerMask backStabLayer = 1 << 23;
        LayerMask riposteLayer = 1 << 24;

        private void Awake()
        {
            cameraHandler = FindObjectOfType<CameraHandler>();
            animationHandler = GetComponent<PlayerAnimatorManager>();
            playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
            playerManager = GetComponentInParent<PlayerManager>();
            playerStats = GetComponentInParent<PlayerStats>();
            playerInventory = GetComponentInParent<PlayerInventory>();
            inputHandler = GetComponentInParent<InputHandler>();
            weaponSlotManager = GetComponent<WeaponSlotManager>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        public void HandleWeaponCombo(WeaponItem weapon)
        {
            if (playerStats.currentStamina <= 0)
                return;

            if (inputHandler.comboFlag)
            {
                animationHandler.anim.SetBool("canDoCombo", false);

                //One Hand (Light Attack)
                if (lastAttack == weapon.Oh_Light_Attack_1)
                {
                    animationHandler.PlayTargetAnimation(weapon.Oh_Light_Attack_2, true);
                    lastAttack = weapon.Oh_Light_Attack_2;
                    PlaySound(weapon.lightAttackClip);
                }
                else if (lastAttack == weapon.Oh_Light_Attack_2)
                {
                    animationHandler.PlayTargetAnimation(weapon.Oh_Light_Attack_3, true);
                    lastAttack = weapon.Oh_Light_Attack_3;
                    PlaySound(weapon.lightAttackClip);
                }

                //One Hand (Heavy Attack)
                else if (lastAttack == weapon.Oh_Heavy_Attack_1)
                {
                    animationHandler.PlayTargetAnimation(weapon.Oh_Heavy_Attack_2, true);
                    lastAttack = weapon.Oh_Heavy_Attack_2;
                    PlaySound(weapon.heavyAttackClip);
                }

                //Two Hand (Light Attack)
                else if (lastAttack == weapon.Th_Light_Attack_1)
                {
                    animationHandler.PlayTargetAnimation(weapon.Th_Light_Attack_2, true);
                    lastAttack = weapon.Th_Light_Attack_2;
                    PlaySound(weapon.lightAttackClip);
                }
                else if (lastAttack == weapon.Th_Light_Attack_2)
                {
                    animationHandler.PlayTargetAnimation(weapon.Th_Light_Attack_3, true);
                    lastAttack = weapon.Th_Light_Attack_3;
                    PlaySound(weapon.lightAttackClip);
                }
                // nếu sau này có heavy attack 2 tay thì bổ sung ở đây
                // else if (lastAttack == weapon.Th_Heavy_Attack_1) { ... }
            }
        }
        public void HandleLightAttack(WeaponItem weapon)
        {
            if (playerStats.currentStamina <= 0)
                return;

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
            PlaySound(weapon.lightAttackClip);
        }

        public void HandleHeavyAttack(WeaponItem weapon)
        {
            if (playerStats.currentStamina <= 0)
                return;

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

            PlaySound(weapon.heavyAttackClip);

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

        public void HandleLBAction()
        {
            PerformLBBlockingAction();
        }    

        public void HandleLTAction()
        {
            if(playerInventory.leftWeapon.isShieldWeapon)
            {
                PerformLTWeaponArt(inputHandler.twoHandFlag);
            }
            else
            {
                if(playerInventory.leftWeapon.isMeleeWeapon)
                {

                }
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
            if (playerManager.isInteracting)
                return;

            SpellItem spell = playerInventory.currentSpell;
            if (spell == null) return;

            if (weapon.isFaithCaster)
            {
                if (playerInventory.currentSpell != null && playerInventory.currentSpell.isFaithSpell)
                {
                    if (playerStats.currentFocusPoints >= playerInventory.currentSpell.focusPointsCost)
                    {
                        playerInventory.currentSpell.AttemptToCastSpell(animationHandler, playerStats, weaponSlotManager);
                        PlaySound(spell.castClip);
                    }
                    else
                    {
                        animationHandler.PlayTargetAnimation("Shrug", true);
                    }
                }
            }
            else if (weapon.isPyroCaster)
            {
                if (playerInventory.currentSpell != null && playerInventory.currentSpell.isPyroSpell)
                {
                    if (playerStats.currentFocusPoints >= playerInventory.currentSpell.focusPointsCost)
                    {
                        playerInventory.currentSpell.AttemptToCastSpell(animationHandler, playerStats, weaponSlotManager);
                        PlaySound(spell.castClip);
                    }
                    else
                    {
                        animationHandler.PlayTargetAnimation("Shrug", true);
                    }
                }
            }
        }    

        private void PerformLTWeaponArt(bool isTwoHanding)
        {
            if (playerManager.isInteracting)
                return;

            if(isTwoHanding)
            {

            }
            else
            {
                animationHandler.PlayTargetAnimation(playerInventory.leftWeapon.weapon_art, true);
            }
        }

        private void SuccessfullyCastSpell()
        {
            playerInventory.currentSpell.SuccessfullyCastSpell(animationHandler, playerStats, cameraHandler, weaponSlotManager);
            animationHandler.anim.SetBool("isFiringSpell", true);
        }
        #endregion

        #region Defense Actions

        private void PerformLBBlockingAction()
        {
            if (playerManager.isInteracting)
                return;

            if(playerManager.isBlocking)
                return;

            animationHandler.PlayTargetAnimation("Block Start", false, true);
            playerEquipmentManager.OpenBlockingCollider();
            playerManager.isBlocking = true;
        }    
        #endregion

        public void AttemptBackStabOrRiposte()
        {
            if (playerStats.currentStamina <= 0)
                return;

            RaycastHit hit;

            if(Physics.Raycast(inputHandler.criticalAttackRayCastStartPoint.position,
                transform.TransformDirection(Vector3.forward), out hit, 0.5f, backStabLayer))
            {
                CharacterManager enemyCharacterManager = hit.transform.gameObject.GetComponentInParent<CharacterManager>();
                DamageCollider rightWeapon = weaponSlotManager.rightHandDamageCollider;

                if(enemyCharacterManager != null)
                {
                    playerManager.transform.position = enemyCharacterManager.backStabCollider.criticalDamageStandPosition.position;
                    Vector3 rotationDirection = playerManager.transform.eulerAngles;
                    rotationDirection = hit.transform.position - playerManager.transform.position;
                    rotationDirection.y = 0;
                    rotationDirection.Normalize();
                    Quaternion tr = Quaternion.LookRotation(rotationDirection);
                    Quaternion targetRotation = Quaternion.Slerp(playerManager.transform.rotation, tr, 500 * Time.deltaTime);
                    playerManager.transform.rotation = targetRotation;

                    int criticalDamage = playerInventory.rightWeapon.criticalDamageMultiplier * rightWeapon.currentWeaponDamage;
                    enemyCharacterManager.pendingCriticalDamage = criticalDamage;

                    animationHandler.PlayTargetAnimation("Back Stab", true);
                    enemyCharacterManager.GetComponentInChildren<AnimatorManager>().PlayTargetAnimation("Back Stabbed", true);
                }    
            }
            else if(Physics.Raycast(inputHandler.criticalAttackRayCastStartPoint.position,
                transform.TransformDirection(Vector3.forward), out hit, 0.7f, riposteLayer))
            {
                CharacterManager enemyCharacterManager = hit.transform.gameObject.GetComponentInParent<CharacterManager>();
                DamageCollider rightWeapon = weaponSlotManager.rightHandDamageCollider;

                if(enemyCharacterManager != null && enemyCharacterManager.canBeRiposted)
                {
                    playerManager.transform.position = enemyCharacterManager.riposteCollider.criticalDamageStandPosition.position;

                    Vector3 rotationDirection = playerManager.transform.root.eulerAngles;
                    rotationDirection = hit.transform.position - playerManager.transform.position;
                    rotationDirection.y = 0;
                    rotationDirection.Normalize();
                    Quaternion tr = Quaternion.LookRotation(rotationDirection);
                    Quaternion targetRotation = Quaternion.Slerp(playerManager.transform.rotation, tr, 500 * Time.deltaTime);
                    playerManager.transform.rotation = targetRotation;

                    int criticalDamage = playerInventory.rightWeapon.criticalDamageMultiplier * rightWeapon.currentWeaponDamage;
                    enemyCharacterManager.pendingCriticalDamage = criticalDamage;

                    animationHandler.PlayTargetAnimation("Riposte", true);
                    enemyCharacterManager.GetComponentInChildren<AnimatorManager>().PlayTargetAnimation("Riposted", true);
                }
            }
        }
    }
}

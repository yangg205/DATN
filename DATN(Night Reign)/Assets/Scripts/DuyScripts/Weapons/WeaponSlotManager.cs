using UnityEngine;

namespace ND
{
    public class WeaponSlotManager : MonoBehaviour
    {
        public WeaponHolderSlot leftHandSlot;
        public WeaponHolderSlot rightHandSlot;
        WeaponHolderSlot backSlot;

        DamageCollider leftHandDamageCollider;
        DamageCollider rightHandDamageCollider;

        public WeaponItem attackingWeapon;

        Animator animator;

        QuickSlotsUI quickSlotsUI;

        PlayerStats playerStats;

        InputHandler inputHandler;

        PlayerEffectManager playerEffectManager;

        public GameObject arrowPrefab;
        private Transform arrowSpawnPoint;


        private void Awake()
        {
            animator = GetComponent<Animator>();
            quickSlotsUI = FindFirstObjectByType<QuickSlotsUI>();
            playerStats = GetComponentInParent<PlayerStats>();
            inputHandler = GetComponentInParent<InputHandler>();
            playerEffectManager = GetComponentInParent<PlayerEffectManager>();

            WeaponHolderSlot[] weaponHolderSlots = GetComponentsInChildren<WeaponHolderSlot>();
            foreach (WeaponHolderSlot weaponSlot in weaponHolderSlots)
            {
                if (weaponSlot.isLeftHandSlot)
                {
                    leftHandSlot = weaponSlot;
                }
                else if (weaponSlot.isRightHandSlot)
                {
                    rightHandSlot = weaponSlot;
                }
                else if (weaponSlot.isBackSlot)
                {
                    backSlot = weaponSlot;
                }
            }
        }
        public void LoadWeaponOnSlot(WeaponItem weaponItem, bool isLeft)
        {
            if (isLeft)
            {
                leftHandSlot.currentWeapon = weaponItem;
                leftHandSlot.LoadWeaponModel(weaponItem);

                if (weaponItem != null && !weaponItem.isBow)
                {
                    LoadLeftWeaponDamageCollider();
                }

                quickSlotsUI.UpdateWeaponQuickSlotsUI(true, weaponItem);

                #region Handle Left Weapon Idle Animations
                if (weaponItem != null)
                {
                    animator.CrossFade(weaponItem.left_hand_idle, 0.2f);
                }
                else
                {
                    animator.CrossFade("Left Arm Empty", 0.2f);
                }
                #endregion
            }
            else
            {
                if (inputHandler.twoHandFlag)
                {
                    backSlot.LoadWeaponModel(leftHandSlot.currentWeapon);
                    leftHandSlot.UnloadWeaponAndDestroy();
                    animator.CrossFade(weaponItem.th_idle, 0.2f);
                }
                else
                {
                    #region Handle Right Weapon Idle Animations
                    animator.CrossFade("Both Arms Empty", 0.2f);

                    backSlot.UnloadWeaponAndDestroy();

                    if (weaponItem != null)
                    {
                        animator.CrossFade(weaponItem.right_hand_idle, 0.2f);
                    }
                    else
                    {
                        animator.CrossFade("Right Arm Empty", 0.2f);
                    }
                    #endregion
                }

                rightHandSlot.currentWeapon = weaponItem;
                rightHandSlot.LoadWeaponModel(weaponItem);

                if (weaponItem.isBow)
                {
                    arrowSpawnPoint = rightHandSlot.currentWeaponModel.transform.Find("ArrowSpawn");
                    if (arrowSpawnPoint == null)
                    {
                        Debug.LogWarning("ArrowSpawn point not found in bow model!");
                    }
                }
                else
                {
                    LoadRightWeaponDamageCollider(); // Chỉ gọi khi không phải cung
                }

                quickSlotsUI.UpdateWeaponQuickSlotsUI(false, weaponItem);
            }
        }

        #region Handle Weapon Collider

        private void LoadLeftWeaponDamageCollider()
        {
            leftHandDamageCollider = leftHandSlot.currentWeaponModel.GetComponentInChildren<DamageCollider>();
            playerEffectManager.leftWeaponFX = leftHandSlot.currentWeaponModel.GetComponentInChildren<WeaponFX>();
        }

        private void LoadRightWeaponDamageCollider()
        {
            rightHandDamageCollider = rightHandSlot.currentWeaponModel.GetComponentInChildren<DamageCollider>();
            playerEffectManager.rightWeaponFX = rightHandSlot.currentWeaponModel.GetComponentInChildren<WeaponFX>();
        }

        public void OpenRightDamageCollider()
        {
            if (attackingWeapon == null || attackingWeapon.isUnarmed || rightHandDamageCollider == null)
                return;
            rightHandDamageCollider.EnableDamageCollider();
             playerEffectManager?.PlayWeaponFX(false); // ← Bật trail
        }

        public void OpenLeftDamageCollider()
        {
            if (attackingWeapon == null || attackingWeapon.isUnarmed || leftHandDamageCollider == null)
                return;
            leftHandDamageCollider.EnableDamageCollider();
             playerEffectManager?.PlayWeaponFX(true);
        }

        public void CloseRightHandDamageCollider()
        {
             rightHandDamageCollider.DisableDamageCollider();
             playerEffectManager?.StopWeaponFX(false);
        }

        public void CloseLeftHandDamageCollider()
        {
              leftHandDamageCollider.DisableDamageCollider();
              playerEffectManager?.StopWeaponFX(true);
        }
        #endregion

        #region Handle Weapon Stamina Drain
        public void DrainStaminaLightAttack()
        {
            if (attackingWeapon == null || attackingWeapon.isUnarmed)
                return;
            playerStats.TakeStaminaDamage(Mathf.RoundToInt(attackingWeapon.baseStamina * attackingWeapon.lightAttackMultiplier));
        }

        public void DrainStaminaHeavyAttack()
        {
            if (attackingWeapon == null || attackingWeapon.isUnarmed)
                return;
            playerStats.TakeStaminaDamage(Mathf.RoundToInt(attackingWeapon.baseStamina * attackingWeapon.heavyAttackMultiplier));
        }
        #endregion


    }
}

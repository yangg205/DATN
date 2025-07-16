using System.Collections;
using UnityEngine;

namespace ND
{
    public class PlayerAttacker : MonoBehaviour
    {
        PlayerStats playerStats;
        PlayerInventory playerInventory;
        PlayerManager playerManager;
        AnimatorHandler animatorHandler;
        InputHandler inputHandler;
        public WeaponSlotManager weaponSlotManager;
        PlayerEffectManager playerEffectManager;
        public string lastAttack;

        private SkillFXController skillFX;

        public int CurrentLightComboStep => lightComboStep;

        private int lightComboStep = 0;
        private int heavyComboStep = 0;
        private float comboResetTimer = 0f;
        private float comboResetDelay = 1.5f;

        private float lastSkillTime = -Mathf.Infinity;
        [SerializeField] private float skillCooldown = 5f;

        private float boostDuration = 6f;
        private float boostCooldown = 20f;
        private float lastBoostTime = -Mathf.Infinity;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            weaponSlotManager = GetComponentInChildren<WeaponSlotManager>();
            inputHandler = GetComponent<InputHandler>();
            playerInventory = GetComponent<PlayerInventory>();
            playerManager = GetComponent<PlayerManager>();
            playerEffectManager = GetComponentInChildren<PlayerEffectManager>();
            skillFX = GetComponentInChildren<SkillFXController>();
        }

        private void Update()
        {
            if (lightComboStep > 0 || heavyComboStep > 0)
            {
                comboResetTimer += Time.deltaTime;
                if (comboResetTimer >= comboResetDelay)
                    ResetCombos();
            }
        }

        public void HandleWeaponCombo(WeaponItem weapon)
        {
            if (inputHandler.isInputDisabled) return;
            if (animatorHandler.anim.GetBool("canDoCombo") == false) return;

            animatorHandler.anim.SetBool("canDoCombo", false);

            if (inputHandler.twoHandFlag)
            {
                if (lastAttack == weapon.Oh_Th_Attack_1)
                {
                    animatorHandler.PlayTargetAnimation(weapon.Oh_Th_Attack_2, true);
                    lastAttack = weapon.Oh_Th_Attack_2;
                }
            }
            else
            {
                if (lastAttack == weapon.Oh_Light_Attack_1)
                {
                    animatorHandler.PlayTargetAnimation(weapon.Oh_Light_Attack_2, true);
                    lastAttack = weapon.Oh_Light_Attack_2;
                }
                else if (lastAttack == weapon.Oh_Light_Attack_2)
                {
                    animatorHandler.PlayTargetAnimation(weapon.Oh_Light_Attack_3, true);
                    lastAttack = weapon.Oh_Light_Attack_3;
                }
                else if (lastAttack == weapon.Oh_Light_Attack_3)
                {
                    animatorHandler.PlayTargetAnimation(weapon.Oh_Light_Attack_4, true);
                    lastAttack = weapon.Oh_Light_Attack_4;
                }
            }

        }

        #region Combo Attacks
        public void HandleLightAttack(WeaponItem weapon)
        {
            if (inputHandler.isInputDisabled) return;
            if (weapon == null || animatorHandler.anim.GetBool("isInteracting")) return;

            comboResetTimer = 0f;

            string animName;

            if (inputHandler.twoHandFlag)
            {
                if (lightComboStep == 2 && !animatorHandler.anim.GetBool("canDoCombo")) return;

                lightComboStep = (lightComboStep % 2) + 1;

                animName = lightComboStep switch
                {
                    1 => weapon.Oh_Th_Attack_1,
                    2 => weapon.Oh_Th_Attack_2,
                    _ => weapon.Oh_Th_Attack_1
                };

                animatorHandler.anim.SetBool("canDoCombo", false);
            }
            else
            {
                lightComboStep++;

                animName = lightComboStep switch
                {
                    1 => weapon.Oh_Light_Attack_1,
                    2 => weapon.Oh_Light_Attack_2,
                    3 => !string.IsNullOrEmpty(weapon.Oh_Light_Attack_3) ? weapon.Oh_Light_Attack_3 : weapon.Oh_Light_Attack_1,
                    4 => !string.IsNullOrEmpty(weapon.Oh_Light_Attack_4) ? weapon.Oh_Light_Attack_4 : weapon.Oh_Light_Attack_1,
                    _ => weapon.Oh_Light_Attack_1
                };

                if (lightComboStep > 4)
                    lightComboStep = 1;
            }

            animatorHandler.PlayTargetAnimation(animName, true);
            lastAttack = animName;
            weaponSlotManager.attackingWeapon = weapon;

            playerEffectManager.PlayWeaponFX(false);


        }

        public void HandleHeavyAttack(WeaponItem weapon)
        {
            if (inputHandler.isInputDisabled) return;
            if (weapon == null || animatorHandler.anim.GetBool("isInteracting")) return;
            if (inputHandler.twoHandFlag) return;

            comboResetTimer = 0f;
            weaponSlotManager.attackingWeapon = weapon;

            string animName;

            heavyComboStep = (heavyComboStep % 2) + 1;
            animName = heavyComboStep == 1 ? weapon.Oh_Heavy_Attack_1 : weapon.Oh_Heavy_Attack_2;

            animatorHandler.PlayTargetAnimation(animName, true);
            lastAttack = animName;
        }

        public void ResetCombos()
        {
            lightComboStep = 0;
            heavyComboStep = 0;
            comboResetTimer = 0;
        }
        #endregion

        #region Skill (Q)
        public void TryUseSkill()
        {
            if (inputHandler.isInputDisabled) return;

            if (Time.time >= lastSkillTime + skillCooldown)
            {
                lastSkillTime = Time.time;
                StartCoroutine(ExecuteWeaponSkill());
            }
            else
            {
                Debug.Log("Skill đang hồi!");
            }
        }

        private IEnumerator ExecuteWeaponSkill()
        {
            WeaponItem weapon = playerInventory.rightWeapon;
            if (weapon == null || playerManager.isInteracting) yield break;

            playerManager.isInteracting = true;

            skillFX?.PlayChargeVFX(); // 🔥 Bật VFX gồng
            animatorHandler.PlayTargetAnimation(weapon.skill_Charge, true);
            yield return new WaitForSeconds(1f);
            skillFX?.StopCurrentVFX(); // ❌ Tắt VFX gồng

            animatorHandler.PlayTargetAnimation(weapon.skill_Attack_01, true);
            weaponSlotManager.OpenRightDamageCollider();
            yield return new WaitForSeconds(0.6f);
            weaponSlotManager.CloseRightHandDamageCollider();

            yield return new WaitForSeconds(0.4f);

            animatorHandler.PlayTargetAnimation(weapon.skill_Attack_02, true);
            weaponSlotManager.OpenRightDamageCollider();
            yield return new WaitForSeconds(0.6f);
            weaponSlotManager.CloseRightHandDamageCollider();

            playerManager.isInteracting = false;
        }
        #endregion

        #region Buff R
        public void TryUseAttackSpeedBoost()
        {
            if (Time.time < lastBoostTime + boostCooldown)
            {
                Debug.Log("Buff chưa hồi!");
                return;
            }

            lastBoostTime = Time.time;
            StartCoroutine(ActivateAttackSpeedBoost());
        }

        private IEnumerator ActivateAttackSpeedBoost()
        {
            WeaponItem weapon = playerInventory.rightWeapon;
            if (weapon == null) yield break;

            playerManager.isInteracting = true;

            skillFX?.PlayBuffVFX(); // 🔥 Bật VFX buff
            animatorHandler.PlayTargetAnimation(weapon.skill_BuffCharge, true);
            yield return new WaitForSeconds(1.5f);
            skillFX?.StopCurrentVFX(); // ❌ Tắt VFX buff

            playerStats.currentAttackSpeed = 5f;
            Debug.Log("Tăng tốc đánh!");

            yield return new WaitForSeconds(boostDuration);

            playerStats.currentAttackSpeed = 1f;
            Debug.Log("Hết buff!");

            playerManager.isInteracting = false;
        }
        #endregion
    }
}

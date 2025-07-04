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
        public string lastAttack;

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

        #region Combo Attacks
        public void HandleLightAttack(WeaponItem weapon)
        {
            if (weapon == null || animatorHandler.anim.GetBool("isInteracting"))
                return;

            comboResetTimer = 0f;
            lightComboStep++;

            string animName = lightComboStep switch
            {
                1 => weapon.Oh_Light_Attack_1,
                2 => weapon.Oh_Light_Attack_2,
                3 => !string.IsNullOrEmpty(weapon.Oh_Light_Attack_3) ? weapon.Oh_Light_Attack_3 : weapon.Oh_Light_Attack_1,
                4 => !string.IsNullOrEmpty(weapon.Oh_Light_Attack_4) ? weapon.Oh_Light_Attack_4 : weapon.Oh_Light_Attack_1,
                _ => weapon.Oh_Light_Attack_1
            };

            if (lightComboStep > 4) lightComboStep = 1;

            animatorHandler.PlayTargetAnimation(animName, true);
            lastAttack = animName;
            weaponSlotManager.attackingWeapon = weapon;

            PlayAttackVFX(weapon.lightAttackVFX);
        }

        public void HandleHeavyAttack(WeaponItem weapon)
        {
            if (weapon == null || animatorHandler.anim.GetBool("isInteracting"))
                return;

            comboResetTimer = 0f;
            weaponSlotManager.attackingWeapon = weapon;

            if (heavyComboStep == 0)
            {
                animatorHandler.PlayTargetAnimation(weapon.Oh_Heavy_Attack_1, true);
                lastAttack = weapon.Oh_Heavy_Attack_1;
                heavyComboStep = 1;
            }
            else
            {
                animatorHandler.PlayTargetAnimation(weapon.Oh_Heavy_Attack_2, true);
                lastAttack = weapon.Oh_Heavy_Attack_2;
                heavyComboStep = 0;
            }

            PlayAttackVFX(weapon.heavyAttackVFX);
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

            animatorHandler.PlayTargetAnimation(weapon.skill_Charge, true);
            PlaySkillChargeVFX(weapon.skillChargeVFX);

            yield return new WaitForSeconds(1f); // thời gian gồng

            // Đòn 1
            animatorHandler.PlayTargetAnimation(weapon.skill_Attack_01, true);
            weaponSlotManager.OpenRightDamageCollider();
            PlaySkillVFX(weapon.specialSkillVFX);
            yield return new WaitForSeconds(0.6f);
            weaponSlotManager.CloseRightHandDamageCollider();

            yield return new WaitForSeconds(0.4f);

            // Đòn 2
            animatorHandler.PlayTargetAnimation(weapon.skill_Attack_02, true);
            weaponSlotManager.OpenRightDamageCollider();
            PlaySkillVFX(weapon.specialSkillVFX);
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
            animatorHandler.PlayTargetAnimation(weapon.skill_BuffCharge, true);

            PlaySkillChargeVFX(weapon.attackSpeedVFX); // dùng chung hiệu ứng spawn tại chân

            yield return new WaitForSeconds(1f); // animation kết thúc

            playerStats.currentAttackSpeed = 1.5f;
            Debug.Log("Tăng tốc đánh!");

            yield return new WaitForSeconds(boostDuration);

            playerStats.currentAttackSpeed = 1f;
            Debug.Log("Hết buff!");

            playerManager.isInteracting = false;
        }
        #endregion

        #region VFX
        public void PlayAttackVFX(GameObject vfxPrefab)
        {
            SpawnWeaponVFX(vfxPrefab);
        }

        public void PlaySkillVFX(GameObject vfxPrefab)
        {
            SpawnWeaponVFX(vfxPrefab);
        }

        public void PlaySkillChargeVFX(GameObject vfxPrefab)
        {
            if (vfxPrefab == null) return;

            Vector3 spawnPos = transform.position + Vector3.up * 1f;
            GameObject vfxInstance = Instantiate(vfxPrefab, spawnPos, Quaternion.identity, transform);
            Destroy(vfxInstance, 2f);
        }

        private void SpawnWeaponVFX(GameObject vfxPrefab)
        {
            if (vfxPrefab == null || weaponSlotManager == null)
                return;

            GameObject weaponModel = weaponSlotManager.GetRightHandWeaponModel();
            if (weaponModel == null)
            {
                Debug.LogError("Weapon model not found.");
                return;
            }

            // Tìm đúng đường dẫn "Weapon Pivot/VFX_SpawnPoint"
            Transform vfxSpawnPoint = weaponModel.transform.Find("Weapon Pivot/VFX_SpawnPoint");

            if (vfxSpawnPoint == null)
            {
                Debug.LogError("Không tìm thấy VFX_SpawnPoint trong: " + weaponModel.name);
                foreach (Transform t in weaponModel.GetComponentsInChildren<Transform>(true))
                {
                    Debug.Log("Transform found: " + t.name);
                }
                return;
            }

            GameObject vfxInstance = Instantiate(vfxPrefab, vfxSpawnPoint.position, vfxSpawnPoint.rotation);
            Destroy(vfxInstance, 2f);
        }
        #endregion
    }
}

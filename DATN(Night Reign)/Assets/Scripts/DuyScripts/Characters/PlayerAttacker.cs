using UnityEngine;

namespace ND
{
    public class PlayerAttacker : MonoBehaviour
    {
        AnimatorHandler animatorHandler;
        InputHandler inputHandler;
        public WeaponSlotManager weaponSlotManager;
        public string lastAttack;

        private int lightComboStep = 0;
        private int heavyComboStep = 0;
        private float comboResetTimer = 0f;
        private float comboResetDelay = 1.5f;

        private void Awake()
        {
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            weaponSlotManager = GetComponentInChildren<WeaponSlotManager>();
            inputHandler = GetComponent<InputHandler>();
        }

        private void Update()
        {
            if (lightComboStep > 0 || heavyComboStep > 0)
            {
                comboResetTimer += Time.deltaTime;
                if (comboResetTimer >= comboResetDelay)
                {
                    ResetCombos();
                }
            }
        }

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

        public void HandleSkill(WeaponItem weapon)
        {
            if (weapon == null)
                return;

            animatorHandler.PlayTargetAnimation("Skill_Attack", true);

            if (weapon.specialSkillVFX != null)
                PlayAttackVFX(weapon.specialSkillVFX);
        }

        public void ResetCombos()
        {
            lightComboStep = 0;
            heavyComboStep = 0;
            comboResetTimer = 0;
        }

        public void PlayAttackVFX(GameObject vfxPrefab)
        {
            if (vfxPrefab == null || weaponSlotManager == null)
                return;

            GameObject weaponModel = weaponSlotManager.GetRightHandWeaponModel();
            if (weaponModel == null)
            {
                Debug.LogWarning("Weapon model not found.");
                return;
            }

            // Tìm VFX_SpawnPoint trong model
            Transform vfxSpawnPoint = null;
            foreach (Transform t in weaponModel.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "VFX_SpawnPoint")
                {
                    vfxSpawnPoint = t;
                    break;
                }
            }

            if (vfxSpawnPoint == null)
            {
                Debug.LogError("Không tìm thấy VFX_SpawnPoint trong: " + weaponModel.name);
                return;
            }

            GameObject vfxInstance = Instantiate(vfxPrefab, vfxSpawnPoint.position, vfxSpawnPoint.rotation);
            Destroy(vfxInstance, 2f);
        }
    }
}

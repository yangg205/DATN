using System.Collections;
using UnityEngine;

namespace ND
{
    public class AnimatorHandler : MonoBehaviour
    {
        PlayerManager playerManager;
        public Animator anim;
        InputHandler inputHandler;
        PlayerLocomotion playerLocomotion;
        int vertical;
        int horizontal;
        public bool canRotate;

        public void Initialize()
        {
            playerManager = GetComponentInParent<PlayerManager>();
            anim = GetComponent<Animator>();
            inputHandler = GetComponentInParent<InputHandler>();
            playerLocomotion = GetComponentInParent<PlayerLocomotion>();
            vertical = Animator.StringToHash("Vertical");
            horizontal = Animator.StringToHash("Horizontal");
        }

        public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement, bool isSprinting)
        {
            float v = 0;
            if (verticalMovement > 0.55f) v = 1;
            else if (verticalMovement > 0) v = 0.5f;
            else if (verticalMovement < -0.55f) v = -1;
            else if (verticalMovement < 0) v = -0.5f;

            float h = 0;
            if (horizontalMovement > 0.55f) h = 1;
            else if (horizontalMovement > 0) h = 0.5f;
            else if (horizontalMovement < -0.55f) h = -1;
            else if (horizontalMovement < 0) h = -0.5f;

            if (isSprinting)
            {
                v = 2;
                h = horizontalMovement;
            }

            anim.SetFloat(vertical, v, 0.1f, Time.deltaTime);
            anim.SetFloat(horizontal, h, 0.1f, Time.deltaTime);
        }

        public void PlayTargetAnimation(string targetAnim, bool isInteracting)
        {
            anim.applyRootMotion = isInteracting;
            anim.SetBool("isInteracting", isInteracting);

            // Gán tốc độ animation theo buff (R)
            var stats = playerManager.GetComponent<PlayerStats>();
            anim.speed = stats != null ? stats.currentAttackSpeed : 1f;

            anim.CrossFade(targetAnim, 0.2f);
        }

        public void CanRotate() => canRotate = true;
        public void StopRotation() => canRotate = false;

        public void EnableCombo() => anim.SetBool("canDoCombo", true);
        public void DisableCombo() => anim.SetBool("canDoCombo", false);

        private void OnAnimatorMove()
        {
            if (!playerManager.isInteracting) return;

            float delta = Time.deltaTime;
            Vector3 deltaPosition = anim.deltaPosition;
            deltaPosition.y = 0;

            Vector3 velocity = deltaPosition / delta;
            playerLocomotion.rigidbody.linearVelocity = velocity;
        }

        public void TriggerAttackVFX()
        {
            var attacker = GetComponentInParent<PlayerAttacker>();
            if (attacker == null) return;

            var weapon = attacker.weaponSlotManager?.attackingWeapon;
            if (weapon == null) return;

            switch (attacker.CurrentLightComboStep)
            {
                case 1: attacker.PlayAttackVFX(weapon.lightAttackVFX_1); break;
                case 2: attacker.PlayAttackVFX(weapon.lightAttackVFX_2); break;
                case 3: attacker.PlayAttackVFX(weapon.lightAttackVFX_3); break;
                case 4: attacker.PlayAttackVFX(weapon.lightAttackVFX_4); break;
            }
        }
    }
}

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

            anim.SetFloat("Vertical", v, 0.1f, Time.deltaTime);
            anim.SetFloat("Horizontal", h, 0.1f, Time.deltaTime);
            anim.SetBool("isSprinting", isSprinting);

            Debug.Log($"[AnimatorHandler] Set Vertical: {v}, Horizontal: {h}, Sprinting: {isSprinting}");
        }

        public void PlayTargetAnimation(string targetAnim, bool isInteracting)
        {
            anim.applyRootMotion = isInteracting;
            anim.SetBool("canRotate", false);
            anim.SetBool("isInteracting", isInteracting);
            anim.CrossFade(targetAnim, 0.2f);
        }

        public void CanRotate() => anim.SetBool("canRotate", true);
        public void StopRotation() => anim.SetBool("canRotate", false);

        public void EnableCombo() => anim.SetBool("canDoCombo", true);
        public void DisableCombo() => anim.SetBool("canDoCombo", false);

        public void EnableIsInvulnerable()
        {
            anim.SetBool("isInvulnerable", true);
        }

        public void DisableIsInvulnerable()
        {
            anim.SetBool("isInvulnerable", false);
        }
        public void EnableParry()
        {
            PlayerManager playerManager = GetComponentInParent<PlayerManager>();
            if (playerManager != null)
            {
                Debug.Log("Parry ENABLED");
                playerManager.isParrying = true;
            }
        }

        public void DisableParry()
        {
            Debug.Log("Parry DISABLED");

            PlayerManager playerManager = GetComponentInParent<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.isParrying = false;
            }
        }

        private void OnAnimatorMove()
        {
            if (!playerManager.isInteracting) return;

            float delta = Time.deltaTime;
            Vector3 deltaPosition = anim.deltaPosition;
            deltaPosition.y = 0;

            Vector3 velocity = deltaPosition / delta;
            playerLocomotion.rigidbody.linearVelocity = velocity;
        }
    }
}

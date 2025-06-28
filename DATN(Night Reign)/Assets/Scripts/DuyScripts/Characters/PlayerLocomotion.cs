using UnityEngine;

namespace ND
{
    public class PlayerLocomotion : MonoBehaviour
    {
        PlayerManager playerManager;
        Transform cameraObject;
        InputHandler inputHandler;
        public Vector3 moveDirection;

        [HideInInspector] public Transform myTransform;
        [HideInInspector] public AnimatorHandler animatorHandler;

        public new Rigidbody rigidbody;
        public GameObject normalCamera;

        [Header("Ground and Air Detection Stats")]
        [SerializeField] float groundDetectionRayStartPoint = 0.5f;
        [SerializeField] float minimumDistanceNeededToBeginFall = 1f;
        [SerializeField] float groundDirectionRayDistance = 0.2f;
        LayerMask ignoreForGroundCheck;
        public float inAirTimer;

        [Header("Movement Stats")]
        [SerializeField] float movementSpeed = 5;
        [SerializeField] float walkingSpeed = 1;
        [SerializeField] float sprintSpeed = 7;
        [SerializeField] float rotationSpeed = 10;
        [SerializeField] float fallingSpeed = 80;

        PlayerStats playerStats;

        [Header("Stamina Stats")]
        [SerializeField] float staminaRegenRate = 20f;

        void Start()
        {
            playerManager = GetComponent<PlayerManager>();
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animatorHandler.Initialize();

            playerStats = GetComponent<PlayerStats>();

            playerManager.isGrounded = true;
            ignoreForGroundCheck = ~(1 << 8 | 1 << 11);
        }

        void Update()
        {
            RegenerateStamina();
        }

        Vector3 normalVector;
        Vector3 targetPosition;

        private void HandleRotation(float delta)
        {
            Vector3 targetDir = cameraObject.forward * inputHandler.vertical;
            targetDir += cameraObject.right * inputHandler.horizontal;
            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
                targetDir = myTransform.forward;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rotationSpeed * delta);
            myTransform.rotation = targetRotation;
        }

        public void HandleMovement(float delta)
        {
            if (inputHandler.rollFlag || playerManager.isInteracting)
                return;

            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;
            moveDirection.Normalize();
            moveDirection.y = 0;

            float speed = movementSpeed;

            if (inputHandler.sprintFlag && inputHandler.moveAmount > 0.5f)
            {
                if (UseStamina(1))
                {
                    speed = sprintSpeed;
                    playerManager.isSprinting = true;
                    moveDirection *= speed;
                }
                else
                {
                    playerManager.isSprinting = false;
                    moveDirection *= movementSpeed;
                }
            }
            else
            {
                moveDirection *= inputHandler.moveAmount < 0.5f ? walkingSpeed : speed;
                playerManager.isSprinting = false;
            }

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
            rigidbody.linearVelocity = projectedVelocity;

            animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, playerManager.isSprinting);

            if (animatorHandler.canRotate)
                HandleRotation(delta);
        }

        public void HandleRollingAndSprinting(float delta)
        {
            if (animatorHandler.anim.GetBool("isInteracting"))
                return;

            if (inputHandler.rollFlag && UseStamina(10))
            {
                moveDirection = cameraObject.forward * inputHandler.vertical;
                moveDirection += cameraObject.right * inputHandler.horizontal;

                if (inputHandler.moveAmount > 0)
                {
                    animatorHandler.PlayTargetAnimation("Rolling", true);
                    moveDirection.y = 0;
                    myTransform.rotation = Quaternion.LookRotation(moveDirection);
                }
                else
                {
                    animatorHandler.PlayTargetAnimation("Backstep", true);
                }
            }
        }

        public void HandleFalling(float delta, Vector3 moveDirection)
        {
            playerManager.isGrounded = false;
            RaycastHit hit;
            Vector3 origin = myTransform.position;
            origin.y += groundDetectionRayStartPoint;

            if (Physics.Raycast(origin, myTransform.forward, out hit, 0.4f))
                moveDirection = Vector3.zero;

            if (playerManager.isInAir)
            {
                rigidbody.AddForce(-Vector3.up * fallingSpeed);
                rigidbody.AddForce(moveDirection * fallingSpeed / 10f);
            }

            Vector3 dir = moveDirection.normalized;
            origin += dir * groundDirectionRayDistance;

            targetPosition = myTransform.position;

            if (Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, ignoreForGroundCheck))
            {
                normalVector = hit.normal;
                Vector3 tp = hit.point;
                playerManager.isGrounded = true;
                targetPosition.y = tp.y;

                if (playerManager.isInAir)
                {
                    animatorHandler.PlayTargetAnimation(inAirTimer > 0.5f ? "Land" : "Empty", true);
                    inAirTimer = 0;
                    playerManager.isInAir = false;
                }
            }
            else
            {
                if (playerManager.isGrounded)
                    playerManager.isGrounded = false;

                if (!playerManager.isInAir && !playerManager.isInteracting)
                {
                    animatorHandler.PlayTargetAnimation("Falling", true);
                    rigidbody.linearVelocity = rigidbody.linearVelocity.normalized * (movementSpeed / 2);
                    playerManager.isInAir = true;
                }
            }

            myTransform.position = Vector3.Lerp(myTransform.position, targetPosition,
                (playerManager.isInteracting || inputHandler.moveAmount > 0) ? Time.deltaTime / 0.1f : 1f);
        }

        public void HandleJumping()
        {
            if (playerManager.isInteracting)
                return;

            if (inputHandler.jump_input && UseStamina(10))
            {
                if (inputHandler.moveAmount > 0)
                {
                    moveDirection = cameraObject.forward * inputHandler.vertical;
                    moveDirection += cameraObject.right * inputHandler.horizontal;
                    animatorHandler.PlayTargetAnimation("Jump", true);
                    moveDirection.y = 0;
                    myTransform.rotation = Quaternion.LookRotation(moveDirection);
                }
                else
                {
                    // Nhảy tại chỗ
                    animatorHandler.PlayTargetAnimation("JumpOnPlace", true);
                }
            }
        }

        private bool UseStamina(int amount)
        {
            if (playerStats.currentStamina < amount)
                return false;

            playerStats.TakeStaminaDamage(amount);
            return true;
        }

        private void RegenerateStamina()
        {
            if (playerStats.currentStamina >= playerStats.maxStamina)
                return;

            bool isConsuming = (inputHandler.sprintFlag && inputHandler.moveAmount > 0.5f)
                                || inputHandler.rollFlag
                                || inputHandler.jump_input
                                || playerManager.isInteracting;

            if (!isConsuming)
            {
                float regenMultiplier = inputHandler.moveAmount == 0 ? 1.5f : 1f;
                float staminaGain = staminaRegenRate * regenMultiplier * Time.deltaTime;

                playerStats.currentStamina += staminaGain;
                playerStats.currentStamina = Mathf.Clamp(playerStats.currentStamina, 0, playerStats.maxStamina);
                playerStats.staminaBar.SetCurrenStamina(Mathf.RoundToInt(playerStats.currentStamina));
            }
        }
    }
}

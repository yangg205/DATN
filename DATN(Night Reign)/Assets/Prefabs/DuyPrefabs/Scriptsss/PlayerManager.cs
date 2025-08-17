using BehaviorDesigner.Runtime.Tasks.Unity.Math;
using ND;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AG
{
    public class PlayerManager : CharacterManager
    {
        InputHandler inputHandler;
        Animator anim;
        CameraHandler cameraHandler;
        PlayerLocomotion playerLocomotion;

        InteractableUI interactableUI;
        public GameObject interactableUIGameObject;
        public GameObject itemInteractableGameObject;

        [Header("Flags")]
        public bool isInteracting;
        public bool isSprinting;
        public bool isInAir;
        public bool isGrounded;
        public bool canDoCombo;

        private void Awake()
        {
            cameraHandler = FindObjectOfType<CameraHandler>();
        }

        void Start()
        {
            inputHandler = GetComponent<InputHandler>();
            anim = GetComponentInChildren<Animator>();
            playerLocomotion = GetComponent<PlayerLocomotion>();
            interactableUI = FindObjectOfType<InteractableUI>();
        }

        void Update()
        {
            float delta = Time.deltaTime;
            isInteracting = anim.GetBool("isInteracting");
            canDoCombo = anim.GetBool("canDoCombo");
            anim.SetBool("isInAir", isInAir);

            inputHandler.TickInput(delta);
            playerLocomotion.HandleRollingAndSprinting(delta);
            playerLocomotion.HandleJumping();

            CheckForInteractableObject();
        }

        private void FixedUpdate()
        {
            float delta = Time.fixedDeltaTime;
            playerLocomotion.HandleMovement(delta);
            playerLocomotion.HandleFalling(delta, playerLocomotion.moveDirection);
        }
        private void LateUpdate()
        {
            inputHandler.rollFlag = false;
            inputHandler.rb_input = false;
            inputHandler.rt_input = false;
            inputHandler.d_pad_Down = false;
            inputHandler.d_pad_Up = false;
            inputHandler.d_pad_Left = false;
            inputHandler.d_pad_Right = false;
            inputHandler.x_input = false;
            inputHandler.jump_input = false;
            inputHandler.inventory_input = false;

            float delta = Time.deltaTime;

            if (cameraHandler != null)
            {
                cameraHandler.FollowTarget(delta);
                cameraHandler.HandleCameraRotation(delta, inputHandler.mouseX, inputHandler.mouseY);
            }

            if (isInAir)
            {
                playerLocomotion.inAirTimer = playerLocomotion.inAirTimer + Time.deltaTime;
            }    
        }

        public void CheckForInteractableObject()
        {
            RaycastHit hit;

            if (Physics.SphereCast(transform.position, 0.1f, transform.forward, out hit, 1f))
            {
                if (hit.collider.tag == "Interactable")
                {
                    Interactable interactableObject = hit.collider.GetComponent<Interactable>();

                    if (interactableObject != null)
                    {
                        string interactableText = interactableObject.interactableText;
                        interactableUI.interactableText.text = interactableText;
                        interactableUIGameObject.SetActive(true);

                        if (inputHandler.x_input)
                        {
                            hit.collider.GetComponent<Interactable>().Interact(this);
                        }
                    }
                }
            }
            else
            {
                if (interactableUIGameObject != null)
                {
                    interactableUIGameObject.SetActive(false);
                }

                if (itemInteractableGameObject != null && inputHandler.x_input)
                {
                    itemInteractableGameObject.SetActive(false);
                }
            }
        }
    }
}


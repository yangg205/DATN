using UnityEngine;

namespace AG
{
    public class InputHandler : MonoBehaviour
    {
        public float horizontal;
        public float vertical;
        public float moveAmount;
        public float mouseX;
        public float mouseY;

        public bool b_input; //roll
        public bool x_input; //interact
        public bool y_input; //two hand
        public bool rb_input;//light attack
        public bool rt_input;//heavy attack
        public bool critical_Attack_input;//critical attack

        public bool jump_input;//jump
        public bool inventory_input;//inventory
        public bool lockOn_input; //lockon
        public bool right_Stick_Right_input;//locon chuyen sang phai
        public bool right_Stick_Left_input;//lockon chuyen sang trai

        public bool d_pad_Up;
        public bool d_pad_Down;
        public bool d_pad_Left;
        public bool d_pad_Right;

        public bool rollFlag;
        public bool twoHandFlag;
        public bool sprintFlag;
        public bool comboFlag;
        public bool lockOnFlag;
        public bool inventoryFlag;
        public float rollInputTimer;

        public Transform criticalAttackRayCastStartPoint;

        //xử lý input k spam
        public bool queuedLightAttack;
        public bool queuedHeavyAttack;

        PlayerControls inputActions;
        PlayerAttacker playerAttacker;
        PlayerInventory playerInventory;
        PlayerManager playerManager;
        UIManager uiManager;
        CameraHandler cameraHandler;
        AnimatorHandler animatorHandler;
        WeaponSlotManager weaponSlotManager;

        Vector2 movementInput;
        Vector2 cameraInput;

        private void Awake()
        {
            playerAttacker = GetComponentInChildren<PlayerAttacker>();
            playerInventory = GetComponent<PlayerInventory>();
            playerManager = GetComponent<PlayerManager>();
            uiManager = FindObjectOfType<UIManager>();
            cameraHandler = FindObjectOfType<CameraHandler>();
            weaponSlotManager = GetComponentInChildren<WeaponSlotManager>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
        }
        public void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerControls();
                inputActions.PlayerMovement.Movement.performed += inputActions => movementInput = inputActions.ReadValue<Vector2>();
                inputActions.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
                inputActions.PlayerActions.RB.performed += i => rb_input = true;
                inputActions.PlayerActions.RT.performed += i => rt_input = true;
                inputActions.PlayerQuickSlots.DPadRight.performed += i => d_pad_Right = true;
                inputActions.PlayerQuickSlots.DPadLeft.performed += i => d_pad_Left = true;
                inputActions.PlayerActions.X.performed += i => x_input = true;
                inputActions.PlayerActions.Jump.performed += i => jump_input = true;
                inputActions.PlayerActions.Inventory.performed += i => inventory_input = true;
                inputActions.PlayerActions.LockOn.performed += i => lockOn_input = true;
                inputActions.PlayerMovement.LockOnTargetRight.performed += i => right_Stick_Right_input = true;
                inputActions.PlayerMovement.LockOnTargetLeft.performed += i => right_Stick_Left_input = true;
                inputActions.PlayerActions.Y.performed += i => y_input = true;
                inputActions.PlayerActions.CriticalAttack.performed += i => critical_Attack_input = true;


            }

            inputActions.Enable();
        }

        public void OnDisable()
        {
            inputActions.Disable();
        }

        public void TickInput(float delta)
        {
            HandleMoveInput(delta);
            HandleRollInput(delta);
            HandleAttackInput(delta);
            HandleQuickSlotsInput();
            HandleInventoryInput();
            HandleLockOnInput();
            HandleTwoHandInput();
            HandleCriticalAttackInput();
        }

        public void HandleMoveInput(float delta)
        {
            horizontal = movementInput.x;
            vertical = movementInput.y;
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
            mouseX = cameraInput.x;
            mouseY = cameraInput.y;
        }

        private void HandleRollInput(float delta)
        {
            b_input = inputActions.PlayerActions.Roll.phase == UnityEngine.InputSystem.InputActionPhase.Performed;
            sprintFlag = b_input;

            if (b_input)
            {
                rollInputTimer += delta;
            }
            else
            {
                if (rollInputTimer > 0 && rollInputTimer < 0.5f)
                {
                    sprintFlag = false;
                    rollFlag = true;
                }

                rollInputTimer = 0;
            }
        }

        private void HandleAttackInput(float delta)
        {
            if (rb_input)
            {
                playerAttacker.HandleRBAction();
            }

            if (rt_input)
            {
                if (playerManager.canDoCombo)
                {
                    comboFlag = true;
                    playerAttacker.HandleWeaponCombo(playerInventory.rightWeapon);
                    comboFlag = false;
                }
                else if (!playerManager.isInteracting)
                {
                    playerAttacker.HandleHeavyAttack(playerInventory.rightWeapon);
                }
                else
                {
                    queuedHeavyAttack = true;
                }
            }

            if (playerManager.canDoCombo)
            {
                if (queuedLightAttack)
                {
                    comboFlag = true;
                    playerAttacker.HandleWeaponCombo(playerInventory.rightWeapon);
                    comboFlag = false;
                    queuedLightAttack = false;
                }
                else if (queuedHeavyAttack)
                {
                    comboFlag = true;
                    playerAttacker.HandleWeaponCombo(playerInventory.rightWeapon);
                    comboFlag = false;
                    queuedHeavyAttack = false;
                }
            }
        }

        public void HandleQuickSlotsInput()
        {
            if(d_pad_Right)
            {
                playerInventory.ChangeRightWeapon();
            }    
            else if(d_pad_Left)
            {
                playerInventory.ChangeLeftWeapon();
            }    
        } 

        public void HandleInventoryInput()
        {
            if(inventory_input)
            {
                inventoryFlag = !inventoryFlag;
                
                if(inventoryFlag)
                {
                    uiManager.OpenSelectWindow();
                    uiManager.UpdateUI();
                    uiManager.hudWindow.SetActive(false);
                }
                else
                {
                    uiManager.CloseSelectWindow();
                    uiManager.CloseAllInventoryWindows();
                    uiManager.hudWindow.SetActive(true);
                }
            }
        }

        public void HandleLockOnInput()
        {
            if(lockOn_input && lockOnFlag == false)
            {
                cameraHandler.ClearLockOnTargets();
                lockOn_input = false;
                lockOnFlag = true;
                cameraHandler.HandleLockOn();
                if(cameraHandler.nearestLockOnTarget != null)
                {
                    cameraHandler.currentLockOnTarget = cameraHandler.nearestLockOnTarget;
                    lockOnFlag = true; 
                }
            }

            else if(lockOn_input && lockOnFlag)
            {
                lockOn_input = false;
                lockOnFlag = false;
                cameraHandler.ClearLockOnTargets();
            }

            if(lockOnFlag && right_Stick_Left_input)
            {
                right_Stick_Left_input = false;
                cameraHandler.HandleLockOn();
                if(cameraHandler.leftLockOnTarget != null)
                {
                    cameraHandler.currentLockOnTarget = cameraHandler.leftLockOnTarget;
                }
            }

            if(lockOnFlag && right_Stick_Right_input)
            {
                right_Stick_Right_input = false;
                cameraHandler.HandleLockOn();
                if (cameraHandler.rightLockOnTarget != null)
                {
                    cameraHandler.currentLockOnTarget = cameraHandler.rightLockOnTarget;
                }
            }

            cameraHandler.SetCameraHeight();
        }

        private void HandleTwoHandInput()
        {
            if(y_input)
            {
                y_input = false;

                twoHandFlag = !twoHandFlag;

                if(twoHandFlag)
                {
                    weaponSlotManager.LoadWeaponOnSlot(playerInventory.rightWeapon, false);
                }
                else
                {
                    weaponSlotManager.LoadWeaponOnSlot(playerInventory.rightWeapon, false);
                    weaponSlotManager.LoadWeaponOnSlot(playerInventory.leftWeapon, true);

                }
            }
        }

        private void HandleCriticalAttackInput()
        {
            if (critical_Attack_input)
            {
                critical_Attack_input = false;
                playerAttacker.AttemptBackStabOrRiposte();
            }
        }    
    }
}


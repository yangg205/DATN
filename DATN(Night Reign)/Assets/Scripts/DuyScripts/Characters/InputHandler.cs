 using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

namespace ND
{
    public class InputHandler : MonoBehaviour
    {
        public float horizontal;
        public float vertical;
        public float moveAmount;
        public float mouseX;
        public float mouseY;

        public bool roll_input;
        public bool interact_input;
        public bool twoHand_input;
        public bool lightAttack_input;
        public bool heavyAttack_input;
        public bool jump_input;
        public bool inventory_input;
        public bool lockOn_input;
        public bool parry_input;
        public bool right_Stick_Right_Input;
        public bool right_Stick_Left_Input;
        public bool skill_input;
        public bool attackSpeedBoost_input;

        public bool d_Pad_Up;
        public bool d_Pad_Down;
        public bool d_Pad_Left;
        public bool d_Pad_Right;

        public bool rollFlag;
        public bool twoHandFlag;
        public bool sprintFlag;
        public bool lockOnFlag;
        public bool inventoryFlag;
        public float rollInputTimer;

        PlayerControls inputActions;
        PlayerAttacker playerAttacker;
        PlayerInventory playerInventory;
        PlayerManager playerManager;
        PlayerStats playerStats;
        WeaponSlotManager weaponSlotManager;
        UIManager uiManager;
        CameraHandler cameraHandler;
        AnimatorHandler animatorHandler;

        Vector2 movementInput;
        Vector2 cameraInput;

        private void Awake()
        {
            playerAttacker = GetComponentInChildren<PlayerAttacker>();
            playerInventory = GetComponent<PlayerInventory>();
            playerManager = GetComponent<PlayerManager>();
            playerStats = GetComponent<PlayerStats>();
            weaponSlotManager = GetComponentInChildren<WeaponSlotManager>();
            uiManager = FindFirstObjectByType<UIManager>();
            cameraHandler = FindFirstObjectByType<CameraHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
        }

        public void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerControls();

                inputActions.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
                inputActions.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
                inputActions.PlayerActions.LightAttack.performed += i => lightAttack_input = true;
                inputActions.PlayerActions.HeavyAttack.performed += i => heavyAttack_input = true;
                inputActions.PlayerActions.Interact.performed += i => interact_input = true;
                inputActions.PlayerActions.Roll.performed += i => roll_input = true;
                inputActions.PlayerActions.Roll.canceled += i => roll_input = false;
                inputActions.PlayerActions.Jump.performed += i => jump_input = true;
                inputActions.PlayerActions.LockOn.performed += i => lockOn_input = true;
                inputActions.PlayerActions.Parry.performed += i => parry_input = true;
                inputActions.PlayerQuickSlots.DPadRight.performed += i => d_Pad_Right = true;
                inputActions.PlayerQuickSlots.DPadLeft.performed += i => d_Pad_Left = true;

                inputActions.PlayerActions.Inventory.performed += i => HandleInventoryInput();
                inputActions.PlayerMovement.LockOnTargetRight.performed += i => right_Stick_Right_Input = true;
                inputActions.PlayerMovement.LockOnTargetLeft.performed += i => right_Stick_Left_Input = true;
                inputActions.PlayerActions.TwoHand.performed += i => twoHand_input = true;
                inputActions.PlayerActions.Skill.performed += i => skill_input = true;
                inputActions.PlayerActions.BoostAttackSpeed.performed += i => attackSpeedBoost_input = true;
            }

            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        public void TickInput(float delta)
        {
            HandleMoveInput(delta);
            if (inventoryFlag)
                return; // Bỏ qua các input khác nếu inventory đang mở
            HandleRollInput(delta);
            HandleAttackInput(delta);
            HandleQuickSlotsInput();
            HandleLockOnInput();
            HandleTwoHandInput();
            HandleSkillInput();
            HandleAttackSpeedBoostInput();
            HandleParryInput();
        }
        private void HandleInventoryInput()
        {
            // Nếu đang mở panel (inventoryFlag == true) → tắt hết
            if (inventoryFlag)
            {
                inventoryFlag = false;
                uiManager.CloseSelectWindow();
                uiManager.CloseAllInventoryWindows();
                uiManager.hudWindow.SetActive(true);

                if (CameraHandler.singleton != null)
                    CameraHandler.singleton.canRotate = true;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                inventoryFlag = true;
                uiManager.OpenSelectWindow();
                uiManager.UpdateUI();
                uiManager.hudWindow.SetActive(false);

                if (CameraHandler.singleton != null)
                    CameraHandler.singleton.canRotate = false;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
            private void HandleMoveInput(float delta)
        {
            horizontal = movementInput.x;
            vertical = movementInput.y;
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
            mouseX = cameraInput.x;
            mouseY = cameraInput.y;
        }

        private void HandleRollInput(float delta)
        {
            roll_input = inputActions.PlayerActions.Roll.phase == UnityEngine.InputSystem.InputActionPhase.Performed;

            if (roll_input)
            {
                rollInputTimer += delta;

                if(playerStats.currentStamina <= 0)
                {
                    roll_input = false;
                    sprintFlag = false;
                }   
                
                if(moveAmount > 0.5f && playerStats.currentStamina > 0)
                {
                    sprintFlag = true;
                }    
            }
            else
            {
                sprintFlag = false;
                if (rollInputTimer > 0 && rollInputTimer < 0.5f)
                {
                    rollFlag = true;
                }

                rollInputTimer = 0;
            }
        }

        private void HandleAttackInput(float delta)
        {

            if (lightAttack_input)
            {
                playerAttacker.HandleLightAction();
                lightAttack_input = false;
            }

            if (heavyAttack_input)
            {
                playerAttacker.HandleHeavyAttack(playerInventory.rightWeapon);
                heavyAttack_input = false;
            }  
        }

        private void HandleQuickSlotsInput()
        {
            if (d_Pad_Right)
                playerInventory.ChangeRightWeapon();
            else if (d_Pad_Left)
                playerInventory.ChangeLeftWeapon();
        }

        private void HandleLockOnInput()
        {
            if (lockOn_input && lockOnFlag == false)
            {
                lockOn_input = false;
                cameraHandler.HandleLockOn();
                if (cameraHandler.nearestLockOnTarget != null)
                {
                    cameraHandler.currentLockOnTarget = cameraHandler.nearestLockOnTarget;
                    lockOnFlag = true;
                }
            }
            else if (lockOn_input && lockOnFlag)
            {
                lockOn_input = false;
                lockOnFlag = false;
                cameraHandler.ClearLockOnTargets();
            }

            if (lockOnFlag && right_Stick_Left_Input)
            {
                right_Stick_Left_Input = false;
                cameraHandler.HandleLockOn();
                if (cameraHandler.leftLockTarget != null)
                {
                    cameraHandler.currentLockOnTarget = cameraHandler.leftLockTarget;
                }
            }

            if (lockOnFlag && right_Stick_Right_Input)
            {
                right_Stick_Right_Input = false;
                cameraHandler.HandleLockOn();
                if (cameraHandler.rightLockTarget != null)
                {
                    cameraHandler.currentLockOnTarget = cameraHandler.rightLockTarget;
                }
            }

            cameraHandler.SetCameraHeight();
        }

        private void HandleTwoHandInput()
        {
            if (twoHand_input)
            {
                twoHand_input = false;
                twoHandFlag = !twoHandFlag;

                if (twoHandFlag)
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
        private void HandleSkillInput()
        {
            if (skill_input)
            {
                skill_input = false;
                playerAttacker.TryUseSkill();
            }
        }

        private void HandleAttackSpeedBoostInput()
        {
            if (attackSpeedBoost_input)
            {
                attackSpeedBoost_input = false;
                playerAttacker.TryUseAttackSpeedBoost();
            }
        }

        private void HandleParryInput()
        {
            if (parry_input)
            {
                parry_input = false;
                playerAttacker.HandleParry();
            }
        }
    }
}

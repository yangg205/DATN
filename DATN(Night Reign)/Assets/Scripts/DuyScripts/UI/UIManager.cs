using UnityEngine;

namespace ND
{
    public class UIManager : MonoBehaviour
    {
        private MouseManager mouseManager;
        public PlayerInventory playerInventory;

        [Header("HUD Windows")]
        public GameObject hudWindow;
        public GameObject selectWindow;
        public GameObject weaponInventoryWindow;

        [Header("Weapon Inventory")]
        public GameObject weaponInventorySlotPrefab;
        public Transform weaponInventorySlotsParent;

        WeaponInventorySlot[] weaponInventorySlots;

        private void Awake()
        {
            mouseManager = MouseManager.Instance;
        }
        private void Start()
        {
            weaponInventorySlots = weaponInventorySlotsParent.GetComponentsInChildren<WeaponInventorySlot>();
        }
        public void UpdateUI()
        {
            #region Weapon Inventory Slot
            for(int i = 0; i < weaponInventorySlots.Length; i++)
            {
                if(i < playerInventory.weaponsInventory.Count)
                {
                    if(weaponInventorySlots.Length < playerInventory.weaponsInventory.Count)
                    {
                        Instantiate(weaponInventorySlotPrefab, weaponInventorySlotsParent);
                        weaponInventorySlots = weaponInventorySlotsParent.GetComponentsInChildren<WeaponInventorySlot>();
                    }
                    weaponInventorySlots[i].AddItem(playerInventory.weaponsInventory[i]);
                }    
                else
                {
                    weaponInventorySlots[i].ClearInventory();
                }    
            }    
            #endregion
        }

        public void OpenSelectWindow()
        {
            mouseManager.UnlockCursor();
            selectWindow.SetActive(true);
        }

        public void CloseSelectWindow()
        {
            mouseManager.LockCursor();
            selectWindow.SetActive(false);
        }

        public void CloseAllInventoryWindows()
        {
            weaponInventoryWindow.SetActive(false);
        }    
    }
}

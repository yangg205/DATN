using UnityEngine;
using UnityEngine.UI;

namespace ND
{
    public class WeaponInventorySlot : MonoBehaviour
    {
        PlayerInventory playerInventory;
        WeaponSlotManager weaponSlotManager;
        UIManager uiManager;

        public Image icon;
        WeaponItem item;

        private void Awake()
        {
            playerInventory = FindObjectOfType<PlayerInventory>();
            weaponSlotManager = FindObjectOfType<WeaponSlotManager>();
            uiManager = FindObjectOfType<UIManager>();
        }
        public void AddItem( WeaponItem newItem )
        {
            item = newItem;
            icon.sprite = item.itemIcon;
            icon.enabled = true;
            gameObject.SetActive(true);
        }    

        public void ClearInventory()
        {
            item = null;
            icon.sprite = null;
            icon.enabled = false;
            gameObject.SetActive(false);
        }    

        public void EquipThisItem()
        {
            if(uiManager.rightHandSlot01Selected)
            {
                playerInventory.weaponsInventory.Add(playerInventory.weaponInRightHandSlots[0]);
                playerInventory.weaponInRightHandSlots[0] = item;
                playerInventory.weaponsInventory.Remove(item);
                playerInventory.rightWeapon = playerInventory.weaponInRightHandSlots[playerInventory.currentRightWeaponIndex];
            }
            else if(uiManager.rightHandSlot02Selected)
            {
                playerInventory.weaponsInventory.Add(playerInventory.weaponInRightHandSlots[1]);
                playerInventory.weaponInRightHandSlots[1] = item;
                playerInventory.weaponsInventory.Remove(item);
                playerInventory.rightWeapon = playerInventory.weaponInRightHandSlots[playerInventory.currentRightWeaponIndex];

            }
            else if(uiManager.leftHandSlot01Selected)
            {
                playerInventory.weaponsInventory.Add(playerInventory.weaponInLeftHandSlots[0]);
                playerInventory.weaponInLeftHandSlots[0] = item;
                playerInventory.weaponsInventory.Remove(item);

            }
            else if(uiManager.rightHandSlot02Selected)
            {
                playerInventory.weaponsInventory.Add(playerInventory.weaponInLeftHandSlots[1]);
                playerInventory.weaponInLeftHandSlots[1] = item;
                playerInventory.weaponsInventory.Remove(item);

            }
            else
            {
                return;
            }
            playerInventory.rightWeapon = playerInventory.weaponInRightHandSlots[playerInventory.currentRightWeaponIndex];
            playerInventory.leftWeapon = playerInventory.weaponInLeftHandSlots[playerInventory.currentLeftWeaponIndex];

            weaponSlotManager.LoadWeaponOnSlot(playerInventory.rightWeapon, false);
            weaponSlotManager.LoadWeaponOnSlot(playerInventory.leftWeapon, true);

            uiManager.equipmentWindowUI.LoadWeaponOnEquipmentScreen(playerInventory);
            uiManager.ResetAllSelectedSlots();
        }
    }
}


using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    WeaponsSlotManager weaponsSlotManager;

    public WeaponItem rightWeapon;
    public WeaponItem leftWeapon;

    private void Awake()
    {
        weaponsSlotManager = GetComponentInChildren<WeaponsSlotManager>();

    }

    private void Start()
    {
        weaponsSlotManager.LoadWeaponOnSlot(rightWeapon, false);
        weaponsSlotManager.LoadWeaponOnSlot(leftWeapon, true);
    }
}

using TMPro;
using UnityEngine;

namespace ND
{
    public class WeaponPickUp : Interactable
    {
        public WeaponItem weapons;

        public override void Interact(PlayerManager playerManager)
        {
            base.Interact(playerManager);
            PickUpItem(playerManager);
        }

        public void PickUpItem(PlayerManager playerManager)
        {
            PlayerInventory playerInventory;
            PlayerLocomotion playerLocomotion;
            AnimatorHandler animatorHandler;

            playerInventory = playerManager.GetComponent<PlayerInventory>();
            playerLocomotion = playerManager.GetComponent<PlayerLocomotion>();
            animatorHandler = playerManager.GetComponentInChildren<AnimatorHandler>();

            playerLocomotion.rigidbody.linearVelocity = Vector3.zero; //stop player from moveving whilist pickup item
            animatorHandler.PlayTargetAnimation("Pick Up Item", true); //play animation pick up 
            playerInventory.weaponsInventory.Add(weapons);

            playerManager.itemInteractableGameObject.GetComponentInChildren<TextMeshProUGUI>().text = weapons.itemName;
            playerManager.itemInteractableGameObject.SetActive(true);
            Destroy(gameObject);
        }
    }
}

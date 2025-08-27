using UnityEngine;

namespace AG
{
    [CreateAssetMenu(menuName = "Items/Consumables/Flask")]
    public class FlaskItem : ConsumableItem
    {
        [Header("Flask Type")]
        public bool eatusFlask;
        public bool ashenFlask;

        [Header("Recovery Amount")]
        public int healRecoverAmount;
        public int focusPointRecoverAmount;

        [Header("Recover FX")]
        public GameObject recoverFX;

        public override void AttempToConsumeItem(PlayerAnimatorManager playerAnimatorManager, WeaponSlotManager weaponSlotManager, PlayerEffectManager playerEffectManager)
        {
            base.AttempToConsumeItem(playerAnimatorManager, weaponSlotManager, playerEffectManager);
            GameObject flask = Instantiate(itemModel, weaponSlotManager.rightHandSlot.transform);
            playerEffectManager.currentParticleFX = recoverFX;
            playerEffectManager.amountToBeHealed = healRecoverAmount;
            playerEffectManager.instantiatedFXModel = flask;
            weaponSlotManager.rightHandSlot.UnloadWeapon();
        }    
    }
}
    


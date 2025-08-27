using UnityEngine;

namespace AG
{
    public class ConsumableItem : Item
    {
        [Header("Item Quality")]
        public int maxItemAmount;
        public int currentItemAmount;

        [Header("Item Model")]
        public GameObject itemModel;

        [Header("Animations")]
        public string consumeAnimation;
        public bool isInteracting;

        public virtual void AttempToConsumeItem(PlayerAnimatorManager playerAnimatorManager, WeaponSlotManager weaponSlotManager, PlayerEffectManager playerEffectManager)
        {
            if(currentItemAmount > 0)
            {
                playerAnimatorManager.PlayTargetAnimation(consumeAnimation, isInteracting, true);
            }    
            else
            {
                playerAnimatorManager.PlayTargetAnimation("Shrug", true);
            }    
        }    
    }
}


using ND;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickUp : Interactable
{
    public GameObject itemPrefabToStore; // Trỏ tới chính prefab này (hoặc clone lại nếu cần)

    [Header("Hiển thị")]
    public string itemName;
    public Texture2D itemIcon;

    public override void Interact(PlayerManager playerManager)
    {
        base.Interact(playerManager);
        PickUpItem(playerManager);
    }

    public void PickUpItem(PlayerManager playerManager)
    {
        var playerLocomotion = playerManager.GetComponent<PlayerLocomotion>();
        var animatorHandler = playerManager.GetComponentInChildren<AnimatorHandler>();
        var playerInventory = playerManager.GetComponent<PlayerInventory>();

        playerLocomotion.rigidbody.linearVelocity = Vector3.zero;
        animatorHandler.PlayTargetAnimation("Pick Up Item", true);

        // Lưu prefab hoặc chính object này
        if (itemPrefabToStore != null)
        {
            GameObject prefabCopy = Instantiate(itemPrefabToStore); // nếu muốn clone
            prefabCopy.SetActive(false); // tạm ẩn trong inventory
            playerInventory.miscInventory.Add(prefabCopy);
        }

        // Hiển thị UI thông báo
        if (playerManager.itemInteractableGameObject != null)
        {
            var text = playerManager.itemInteractableGameObject.GetComponentInChildren<TextMeshProUGUI>();
            var image = playerManager.itemInteractableGameObject.GetComponentInChildren<RawImage>();

            if (text != null) text.text = itemName;
            if (image != null && itemIcon != null)
                image.texture = itemIcon;

            playerManager.itemInteractableGameObject.SetActive(true);
        }

        Destroy(gameObject); // huỷ object trên map
    }
}

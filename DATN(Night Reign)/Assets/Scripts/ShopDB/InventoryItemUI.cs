using UnityEngine;
using UnityEngine.UI;
using TMPro;
using server.model;

public class InventoryItemUI : MonoBehaviour
{
    public Button itemButton;
    public Image itemIcon;

    private server.model.Item currentItemData;
    private ShopManager shopManager;

    public void Initialize(server.model.Item itemData, ShopManager manager)
    {
        currentItemData = itemData;
        shopManager = manager;

        itemIcon.sprite = null;
        // Debug.LogWarning($"Item {itemData.Name} (ID: {itemData.Item_id}) has no IconPath specified.");

        itemButton.onClick.AddListener(OnItemClicked);
    }

    private void OnItemClicked()
    {
        shopManager.DisplayItemDetails(currentItemData);
    }

    private void OnDestroy()
    {
        if (itemButton != null)
        {
            itemButton.onClick.RemoveListener(OnItemClicked);
        }
    }
}
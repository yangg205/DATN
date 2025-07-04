using UnityEngine;
using UnityEngine.UI;
using TMPro;
using server.model;

public class InventoryItemUI : MonoBehaviour
{
    public Button itemButton;
    [SerializeField] public Image itemIcon; // Added SerializeField to ensure it's assignable in Inspector

    private server.model.Item currentItemData;
    private ShopManager shopManager;

    public void Initialize(server.model.Item itemData, ShopManager manager)
    {
        currentItemData = itemData;
        shopManager = manager;

        // --- IMPROVEMENT HERE: Load Icon from Resources ---
        if (!string.IsNullOrEmpty(itemData.IconPath))
        {
            // Remove "Assets/" if present and ensure path is relative to a Resources folder
            string resourcePath = itemData.IconPath.Replace("Assets/", "").Replace(".png", "");
            Sprite iconSprite = Resources.Load<Sprite>(resourcePath);
            if (iconSprite != null)
            {
                itemIcon.sprite = iconSprite;
                itemIcon.color = Color.white; // Ensure visibility if sprite is loaded
            }
            else
            {
                itemIcon.sprite = null; // Clear if not found
                itemIcon.color = new Color(1, 1, 1, 0); // Make transparent if no sprite
                Debug.LogWarning($"Could not load sprite for item {itemData.Name} in list at path: {itemData.IconPath}. Make sure it's in a Resources folder and the path is correct.");
            }
        }
        else
        {
            itemIcon.sprite = null; // Clear if no path
            itemIcon.color = new Color(1, 1, 1, 0); // Make transparent if no sprite
            Debug.LogWarning($"Item {itemData.Name} (ID: {itemData.Item_id}) has no IconPath specified for display in the list.");
        }
        // --- END IMPROVEMENT ---

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
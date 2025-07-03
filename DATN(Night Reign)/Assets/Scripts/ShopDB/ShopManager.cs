using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Linq;
using System;
using server.model;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Display")]
    public Transform showItemParent;
    public GameObject itemPrefab;

    [Header("Item Description Panel")]
    public GameObject itemDesPanel;
    public Image itemDesIcon;
    public TextMeshProUGUI itemDesName;
    public TextMeshProUGUI itemDesDescription;
    public TextMeshProUGUI itemDesPrice;
    public TMP_InputField quantityInputField;
    public Button buyButton;

    // New: Buttons for quantity adjustment
    [Header("Quantity Adjustment Buttons")]
    public Button plusButton;
    public Button minusButton;

    [Header("Category Buttons")]
    public Button weaponButton;
    public Button armorButton;
    public Button potionButton;
    public Button currencyButton;

    private SignalRClient _signalRClientService;

    private List<server.model.Item> allShopItems;
    private server.model.Item selectedItem;
    public int currentPlayerCharacterId;

    void Awake()
    {
        _signalRClientService = FindObjectOfType<SignalRClient>();
        if (_signalRClientService == null)
        {
            Debug.LogError("SignalRClient not found in the scene! Please ensure it's on a GameObject.");
            enabled = false;
            return;
        }
    }

    void Start()
    {
        currentPlayerCharacterId = PlayerPrefs.GetInt("PlayerCharacterId", 0);
        InitializeCategoryButtons();

        itemDesPanel.SetActive(false);

        quantityInputField.text = "1";
        quantityInputField.onValueChanged.AddListener(OnQuantityChanged);

        // Assigning handlers for plus and minus buttons
        plusButton.onClick.AddListener(OnPlusButtonClicked);
        minusButton.onClick.AddListener(OnMinusButtonClicked);

        buyButton.onClick.AddListener(OnBuyButtonClicked);
        buyButton.interactable = false;

        LoadAllShopItems();
    }

    private void InitializeCategoryButtons()
    {
        weaponButton.onClick.AddListener(() => FilterItemsByType("weapon"));
        armorButton.onClick.AddListener(() => FilterItemsByType("armor"));
        potionButton.onClick.AddListener(() => FilterItemsByType("potion"));
        currencyButton.onClick.AddListener(() => FilterItemsByType("currency"));
    }

    public async void LoadAllShopItems()
    {
        try
        {
            allShopItems = await _signalRClientService.GetAllItems();

            if (allShopItems != null && allShopItems.Count > 0)
            {
                FilterItemsByType("weapon");
            }
            else
            {
                ClearInventoryDisplay();
                Debug.Log("Shop is empty or failed to retrieve items from API.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load all shop items from API: {ex.Message}");
        }
    }

    private void ClearInventoryDisplay()
    {
        foreach (Transform child in showItemParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void PopulateInventoryUI(List<server.model.Item> itemsToDisplay)
    {
        ClearInventoryDisplay();

        if (itemsToDisplay == null || itemsToDisplay.Count == 0)
        {
            Debug.Log("No items to display for this category.");
            return;
        }

        foreach (server.model.Item itemData in itemsToDisplay)
        {
            GameObject itemUIObject = Instantiate(itemPrefab, showItemParent);
            InventoryItemUI itemUI = itemUIObject.GetComponent<InventoryItemUI>();

            if (itemUI != null)
            {
                itemUI.Initialize(itemData, this);
                itemUIObject.name = $"ShopItem_{itemData.Name}";
            }
            else
            {
                Debug.LogError("ItemPrefab is missing the InventoryItemUI script!");
            }
        }
    }

    public void FilterItemsByType(string type)
    {
        if (allShopItems == null)
        {
            Debug.LogWarning("No shop items loaded yet to filter.");
            return;
        }

        List<server.model.Item> filteredItems = allShopItems.Where(item => item.Type == type).ToList();

        PopulateInventoryUI(filteredItems);
        itemDesPanel.SetActive(false);
        selectedItem = null;
        buyButton.interactable = false;
        // Optionally disable plus/minus buttons if no item is selected
        plusButton.interactable = false;
        minusButton.interactable = false;
    }

    public void DisplayItemDetails(server.model.Item itemData)
    {
        selectedItem = itemData;
        if (itemData == null)
        {
            itemDesPanel.SetActive(false);
            buyButton.interactable = false;
            plusButton.interactable = false;
            minusButton.interactable = false;
            return;
        }

        itemDesPanel.SetActive(true);

        itemDesIcon.sprite = null;
        // Debug.LogWarning($"Item {itemData.Name} (ID: {itemData.Item_id}) has no icon path for description. Please ensure IconPath is provided or mapped.");

        itemDesName.text = itemData.Name;
        itemDesDescription.text = itemData.Description;
        itemDesPrice.text = $"{itemData.Price} Gold";

        // Reset quantity to 1 when a new item is selected
        quantityInputField.text = "1";
        CheckBuyButtonInteractability();
    }

    private void OnQuantityChanged(string value)
    {
        CheckBuyButtonInteractability();
    }

    private void OnPlusButtonClicked()
    {
        int currentQuantity;
        if (int.TryParse(quantityInputField.text, out currentQuantity))
        {
            currentQuantity++;
            quantityInputField.text = currentQuantity.ToString();
            itemDesPrice.text = $"{selectedItem.Price * currentQuantity} Gold"; 
        }
        else
        {
            quantityInputField.text = "1"; // Default to 1 if input is invalid
        }
        CheckBuyButtonInteractability();
    }

    private void OnMinusButtonClicked()
    {
        int currentQuantity;
        if (int.TryParse(quantityInputField.text, out currentQuantity))
        {
            currentQuantity--;
            if (currentQuantity < 1) // Ensure quantity doesn't go below 1
            {
                currentQuantity = 1;
            }
            itemDesPrice.text = $"{selectedItem.Price * currentQuantity} Gold";
            quantityInputField.text = currentQuantity.ToString();
        }
        else
        {
            quantityInputField.text = "1"; // Default to 1 if input is invalid
        }
        CheckBuyButtonInteractability();
    }

    private void CheckBuyButtonInteractability()
    {
        if (selectedItem == null)
        {
            buyButton.interactable = false;
            plusButton.interactable = false;
            minusButton.interactable = false;
            return;
        }

        int quantity;
        if (int.TryParse(quantityInputField.text, out quantity) && quantity > 0)
        {
            // Optional: Add logic to check if player has enough gold
            // decimal totalPrice = selectedItem.Price * quantity;
            // if (PlayerManager.Instance.CurrentGold >= totalPrice) { ... }
            buyButton.interactable = true;
            plusButton.interactable = true; // Enable plus/minus if item is selected and quantity is valid
            minusButton.interactable = (quantity > 1); // Disable minus if quantity is 1
        }
        else
        {
            buyButton.interactable = false;
            plusButton.interactable = false;
            minusButton.interactable = false;
        }
    }

    private async void OnBuyButtonClicked()
    {
        if (selectedItem == null)
        {
            Debug.LogWarning("No item selected to buy.");
            return;
        }

        int quantity;
        if (!int.TryParse(quantityInputField.text, out quantity) || quantity <= 0)
        {
            Debug.LogWarning("Invalid quantity for purchase.");
            return;
        }

        BuyItemRequestDTO request = new BuyItemRequestDTO
        {
            PlayerCharacterId = currentPlayerCharacterId,
            ItemId = selectedItem.Item_id,
            Quantity = quantity
        };

        Debug.Log($"Attempting to buy {request.Quantity} of {selectedItem.Name} (ID: {request.ItemId}) for Player ID: {request.PlayerCharacterId}.");

        try
        {
            BuyItemResponseDTO response = await _signalRClientService.BuyItem(request);

            if (response.Success)
            {
                Debug.Log($"Successfully bought {quantity} of {selectedItem.Name}! Message: {response.Message}");
                Debug.Log($"New Player Total Coin: {response.NewPlayerTotalCoin}");
            }
            else
            {
                Debug.LogWarning($"Failed to buy {selectedItem.Name}: {response.Message}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during purchase of {selectedItem.Name}: {ex.Message}");
        }
    }
}
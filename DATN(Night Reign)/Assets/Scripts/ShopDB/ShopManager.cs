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

    [Header("Quantity Adjustment Buttons")]
    public Button plusButton;
    public Button minusButton;

    [Header("Category Buttons")]
    public Button weaponButton;
    public Button armorButton;
    public Button potionButton;
    public Button currencyButton;

    [Header("Currency Exchange Panel")]
    public GameObject currencyExchangePanel;
    public TMP_InputField moneyInputField;
    public Button changeButton;
    // Nếu bạn có TextMeshProUGUI riêng hiển thị tổng số tiền/coin của người chơi ở đâu đó trên UI chính, giữ lại biến này:
    // public TextMeshProUGUI currentMoneyText;
    // public TextMeshProUGUI currentCoinText;

    // Đây là TextMeshProUGUI để hiển thị Coin được quy đổi
    public TextMeshProUGUI convertedCoinDisplay;

    // ĐÃ LOẠI BỎ: public TextMeshProUGUI exchangeResultText; // Không cần biến này nếu chỉ muốn log

    private SignalRClient _signalRClientService;

    private List<server.model.Item> allShopItems;
    private server.model.Item selectedItem;
    public int currentPlayerCharacterId;
    private int currentPlayerId;

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
        currentPlayerId = PlayerPrefs.GetInt("PlayerId", 0);

        InitializeCategoryButtons();

        itemDesPanel.SetActive(false);
        currencyExchangePanel.SetActive(false);

        quantityInputField.text = "1";
        quantityInputField.onValueChanged.AddListener(OnQuantityChanged);

        plusButton.onClick.AddListener(OnPlusButtonClicked);
        minusButton.onClick.AddListener(OnMinusButtonClicked);

        buyButton.onClick.AddListener(OnBuyButtonClicked);
        buyButton.interactable = false;

        changeButton.onClick.AddListener(OnExchangeCurrencyClicked);
        moneyInputField.onValueChanged.AddListener(OnMoneyInputValueChanged);

        LoadAllShopItems();
        // RefreshPlayerCurrencyDisplays();
    }

    private void InitializeCategoryButtons()
    {
        weaponButton.onClick.AddListener(() => FilterItemsByType("weapon"));
        armorButton.onClick.AddListener(() => FilterItemsByType("armor"));
        potionButton.onClick.AddListener(() => FilterItemsByType("potion"));
        currencyButton.onClick.AddListener(OnCurrencyButtonClicked);
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

            itemUI.Initialize(itemData, this);
            itemUIObject.name = $"ShopItem_{itemData.Name}";
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
        currencyExchangePanel.SetActive(false);
        selectedItem = null;
        buyButton.interactable = false;
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
        currencyExchangePanel.SetActive(false);

        if (!string.IsNullOrEmpty(itemData.IconPath))
        {
            string resourcePath = itemData.IconPath
                                    .Replace("Assets/", "")
                                    .Replace(".png", "");

            Sprite iconSprite = Resources.Load<Sprite>(resourcePath);
            if (iconSprite != null)
            {
                itemDesIcon.sprite = iconSprite;
                itemDesIcon.color = Color.white;
            }
            else
            {
                itemDesIcon.sprite = null;
                itemDesIcon.color = new Color(1, 1, 1, 0);
                Debug.LogWarning($"Could not load sprite for item {itemData.Name} at path: {itemData.IconPath}. Expected resource path: '{resourcePath}'. Make sure it's in a Resources folder and the path is correct (e.g., Assets/Resources/{resourcePath}.png).");
            }
        }
        else
        {
            itemDesIcon.sprite = null;
            itemDesIcon.color = new Color(1, 1, 1, 0);
            Debug.LogWarning($"Item {itemData.Name} (ID: {itemData.Item_id}) has no icon path for description. Please ensure IconPath is provided.");
        }

        itemDesName.text = itemData.Name;
        itemDesDescription.text = itemData.Description;
        itemDesPrice.text = $"{itemData.Price} Gold";

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
        if (selectedItem != null && int.TryParse(quantityInputField.text, out currentQuantity))
        {
            currentQuantity++;
            quantityInputField.text = currentQuantity.ToString();
            itemDesPrice.text = $"{selectedItem.Price * currentQuantity} Gold";
        }
        else
        {
            quantityInputField.text = "1";
        }
        CheckBuyButtonInteractability();
    }

    private void OnMinusButtonClicked()
    {
        int currentQuantity;
        if (selectedItem != null && int.TryParse(quantityInputField.text, out currentQuantity))
        {
            currentQuantity--;
            if (currentQuantity < 1)
            {
                currentQuantity = 1;
            }
            itemDesPrice.text = $"{selectedItem.Price * currentQuantity} Gold";
            quantityInputField.text = currentQuantity.ToString();
        }
        else
        {
            quantityInputField.text = "1";
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
            buyButton.interactable = true;
            plusButton.interactable = true;
            minusButton.interactable = (quantity > 1);
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

    // --- Currency Exchange Logic ---

    private void OnCurrencyButtonClicked()
    {
        ClearInventoryDisplay();
        itemDesPanel.SetActive(false);

        currencyExchangePanel.SetActive(true);
        moneyInputField.text = "";
        convertedCoinDisplay.text = "";
        // Debug.Log("Currency exchange panel opened. Input fields cleared."); // Log khi mở panel
    }

    private void OnMoneyInputValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            convertedCoinDisplay.text = "";
            Debug.Log("Money input is empty. Converted Coin display cleared."); // Log khi input rỗng
            return;
        }

        if (decimal.TryParse(value, out decimal amountToExchange))
        {
            decimal convertedCoin = amountToExchange * 10000m;
            convertedCoinDisplay.text = $"{convertedCoin:N0}";
            Debug.Log($"Money input changed to '{value}'. Converted Coin: {convertedCoin:N0}"); // Log khi nhập hợp lệ
        }
        else
        {
            convertedCoinDisplay.text = "";
            Debug.LogWarning($"Invalid input '{value}'. Please enter a valid number for Money."); // Log lỗi khi nhập không hợp lệ
        }
    }

    private async void OnExchangeCurrencyClicked()
    {
        if (!decimal.TryParse(moneyInputField.text, out decimal amountToExchange) || amountToExchange <= 0)
        {
            Debug.LogWarning("Exchange failed: Please enter a valid amount to exchange."); // Log lỗi
            return;
        }

        ExchangeCurrencyRequestDTO request = new ExchangeCurrencyRequestDTO
        {
            PlayerId = currentPlayerId,
            PlayerCharacterId = currentPlayerCharacterId,
            AmountToExchange = amountToExchange,
            SourceCurrencyItemName = "Money",
            TargetCurrencyItemName = "Coin"
        };

        Debug.Log($"Attempting to exchange {request.AmountToExchange} Money for Coin for Player ID: {request.PlayerId}, Character ID: {request.PlayerCharacterId}.");

        try
        {
            // Đảm bảo tên phương thức đúng: ExchangeCurrencyAsync
            ExchangeCurrencyResponseDTO response = await _signalRClientService.ExchangeCurrency(request);

            if (response.Success)
            {
                Debug.Log($"Exchange successful! You now have {response.NewPlayerTotalMoney:N0} Money and {response.NewPlayerTotalCoin:N0} Coin."); // Log thành công

                moneyInputField.text = "";
                convertedCoinDisplay.text = "";
            }
            else
            {
                Debug.LogWarning($"Exchange failed: {response.Message}"); // Log lỗi từ server
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exchange error: {ex.Message}"); // Log lỗi Exception
        }
    }

    public async void RefreshPlayerCurrencyDisplays()
    {
        // Hàm này chỉ cần nếu bạn có TextMeshProUGUI riêng để hiển thị tổng tiền/coin ở đâu đó trên UI
    }
}

// ... (Các DTO class và SignalRClient.cs vẫn giữ nguyên như cũ) ...
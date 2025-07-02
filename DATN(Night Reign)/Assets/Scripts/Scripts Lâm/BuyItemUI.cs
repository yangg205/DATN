using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuyItemUI : MonoBehaviour
{
    public Button plusButton;
    public Button minusButton;
    public TMP_InputField quantityInput;
    public TMP_Text totalPriceText;

    public int itemPrice = 100; // Ví dụ giá cố định, bạn có thể thay đổi tuỳ item
    private int quantity = 1;

    void Start()
    {
        quantityInput.text = quantity.ToString();
        UpdateTotal();

        plusButton.onClick.AddListener(() => ChangeQuantity(1));
        minusButton.onClick.AddListener(() => ChangeQuantity(-1));
        quantityInput.onValueChanged.AddListener(OnQuantityChanged);
    }

    void ChangeQuantity(int delta)
    {
        quantity += delta;
        quantity = Mathf.Clamp(quantity, 1, 999); // Giới hạn số lượng
        quantityInput.text = quantity.ToString();
        UpdateTotal();
    }

    void OnQuantityChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            quantity = Mathf.Clamp(result, 1, 999);
            UpdateTotal();
        }
    }

    void UpdateTotal()
    {
        int total = itemPrice * quantity;
        totalPriceText.text = $"Total: {total} Gold";
    }
}

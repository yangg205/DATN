using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIController : MonoBehaviour
{
    [Header("References")]
    public GameObject inventoryPanel; // Kéo Inventory Panel vào đây
    public TextMeshProUGUI buttonText; // Kéo TMP text trong button vào đây

    private bool isInventoryOpen = false;

    void Start()
    {
        inventoryPanel.SetActive(false); // Tắt Inventory lúc đầu
        UpdateButtonText();
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
        UpdateButtonText();
    }

    void UpdateButtonText()
    {
        if (isInventoryOpen)
            buttonText.text = "Đóng túi đồ";
        else
            buttonText.text = "Mở túi đồ";
    }
}

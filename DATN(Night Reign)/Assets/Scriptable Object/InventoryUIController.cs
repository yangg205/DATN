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

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }
    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
    }

}

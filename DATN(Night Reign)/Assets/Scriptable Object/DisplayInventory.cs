using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayInventory : MonoBehaviour
{
    public int X_START;
    public int Y_START;
    public InventoryObject inventory;
    public int X_SPACE_BETWEEN_ITEM;
    public int NUMBER_OF_COLUMN;
    public int Y_SPACE_BETWEEN_ITEMS;

    Dictionary<InventorySlot, GameObject> itemsDisplayed = new Dictionary<InventorySlot, GameObject>();

    void Start()
    {
        CreateDisplay();
    }

    void Update()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        for (int i = 0; i < inventory.Container.Count; i++)
        {
            var slot = inventory.Container[i];

            if (itemsDisplayed.ContainsKey(slot))
            {
                itemsDisplayed[slot].GetComponentInChildren<TextMeshProUGUI>().text = slot.amount.ToString("n0");
            }
            else
            {
                if (slot.item != null && slot.item.prefab != null)
                {
                    var obj = Instantiate(slot.item.prefab, Vector3.zero, Quaternion.identity, transform);

                    RectTransform rt = obj.GetComponent<RectTransform>();
                    if (rt != null)
                        rt.localPosition = GetPosition(i);

                    var textComponent = obj.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComponent != null)
                        textComponent.text = slot.amount.ToString("n0");

                    itemsDisplayed.Add(slot, obj);
                }
            }
        }
    }

    public void CreateDisplay()
    {
        if (inventory == null || inventory.Container == null)
        {
            Debug.LogError("Inventory hoặc Container chưa được gán!");
            return;
        }

        for (int i = 0; i < inventory.Container.Count; i++)
        {
            var slot = inventory.Container[i];

            if (slot == null || slot.item == null || slot.item.prefab == null)
            {
                Debug.LogWarning($"Slot {i} null hoặc item/prefab null.");
                continue;
            }

            var obj = Instantiate(slot.item.prefab, Vector3.zero, Quaternion.identity, transform);

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt != null)
                rt.localPosition = GetPosition(i);

            var textComponent = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
                textComponent.text = slot.amount.ToString("n0");

            itemsDisplayed.Add(slot, obj);
        }
    }

    public Vector3 GetPosition(int i)
    {
        return new Vector3(
            X_START + (X_SPACE_BETWEEN_ITEM * (i % NUMBER_OF_COLUMN)),
            -Y_SPACE_BETWEEN_ITEMS * (i / NUMBER_OF_COLUMN),
            0f
        );
    }
}

using System.Collections.Generic;
using UnityEngine;

public class SimpleInventory : MonoBehaviour
{
    public static SimpleInventory Instance;

    private Dictionary<string, int> items = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddItem(string itemID, int amount)
    {
        if (!items.ContainsKey(itemID))
            items[itemID] = 0;

        items[itemID] += amount;
        Debug.Log($"Đã thêm {amount}x {itemID} vào túi đồ.");
    }

    public int GetItemCount(string itemID)
    {
        return items.TryGetValue(itemID, out int count) ? count : 0;
    }
}

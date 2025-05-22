using UnityEngine;
[CreateAssetMenu(fileName = "New Food Object ", menuName = "Inventory System/Items/Food")]
public class FoodObject :ItemObject
{
    public int restoreHeathValue;
    public void Awake()
    {
        type = ItemType.Food;
    }
}

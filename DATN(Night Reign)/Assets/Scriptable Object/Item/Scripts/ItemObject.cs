using UnityEngine;
// Không cần using System.Collections; hay System.Collections.Generic; ở đây nếu không dùng đến

// Các Enum định nghĩa ở đây để dễ truy cập
public enum ItemType
{
    Food,
    Equipment,
    Default
}

public enum Attributes
{
    Agility,
    Intellect,
    Stamina,
    Strength
}

// Định nghĩa ItemObject
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory System/Items/ItemObject")] // Sửa lại menuName nếu muốn
public abstract class ItemObject : ScriptableObject
{
    public int Id;
    public Sprite uiDisplay;
    public ItemType type;
    [TextArea(15, 20)]
    public string description;
    public ItemBuff[] buffs;

    // Các trường mới đã thêm ở lần trước
    [Header("World Display Options")]
    public GameObject worldItemPrefab;
    public Sprite worldItemSprite;

    public Item CreateItem()
    {
        Item newItem = new Item(this); // Chỗ này cần class Item được định nghĩa
        return newItem;
    }
}

// ĐỊNH NGHĨA CLASS ITEM NGAY TẠI ĐÂY (TRONG CÙNG FILE HOẶC FILE RIÊNG NẾU MUỐN, NHƯNG PHẢI TỒN TẠI)
[System.Serializable]
public class Item
{
    public string Name; // Thường là itemObject.name
    public int Id;      // Thường là itemObject.Id
    public ItemBuff[] buffs; // Buffs cụ thể cho instance này, có thể có giá trị random

    // Constructor của Item
    public Item(ItemObject itemObject) // Đây là constructor mà lỗi 'Progress.Item' đang ám chỉ
    {
        Name = itemObject.name; // Lấy tên từ ScriptableObject
        Id = itemObject.Id;
        buffs = new ItemBuff[itemObject.buffs.Length];
        for (int i = 0; i < buffs.Length; i++)
        {
            // Tạo một ItemBuff mới với giá trị min/max từ ItemObject
            // và để nó tự generate value
            buffs[i] = new ItemBuff(itemObject.buffs[i].min, itemObject.buffs[i].max)
            {
                attribute = itemObject.buffs[i].attribute // Gán attribute
            };
            // Value đã được generate trong constructor của ItemBuff
        }
    }
}

// ĐỊNH NGHĨA CLASS ITEMBUFF NGAY TẠI ĐÂY
[System.Serializable]
public class ItemBuff
{
    public Attributes attribute;
    public int value; // Giá trị cuối cùng của buff sau khi random
    public int min;   // Giá trị nhỏ nhất có thể
    public int max;   // Giá trị lớn nhất có thể

    public ItemBuff(int _min, int _max)
    {
        min = _min;
        max = _max;
        GenerateValue();
    }

    public void GenerateValue()
    {
        // Đảm bảo max lớn hơn hoặc bằng min để Random.Range hoạt động đúng
        if (max >= min)
        {
            value = UnityEngine.Random.Range(min, max + 1); // max + 1 nếu bạn muốn max được bao gồm
        }
        else
        {
            value = min; // Hoặc một giá trị mặc định nào đó nếu min > max
        }
    }
}
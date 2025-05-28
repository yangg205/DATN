using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CharacterButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Các tham chiếu UI
    public Image characterImage;  // Hình ảnh của nhân vật
    public Sprite hoverSprite;    // Hình ảnh khi hover (di chuột vào)
    public Sprite defaultSprite;  // Hình ảnh mặc định khi không hover

    public TextMeshProUGUI characterNameText;  // Text hiển thị tên nhân vật
    public TextMeshProUGUI characterDescriptionText;  // Text hiển thị mô tả
    public TextMeshProUGUI characterStatsText; // Text hiển thị chỉ số nhân vật

    public Button button;  // Nút chọn nhân vật

    // Đối tượng Character để lưu trữ thông tin nhân vật
    public Character character;

    // Hàm gọi khi chuột di vào nút
    public void OnPointerEnter(PointerEventData eventData)
    {
        characterStatsText.color = Color.white;

        // Thay đổi hình ảnh khi hover vào
        characterImage.sprite = hoverSprite;

        // Ẩn tên nhân vật khi hover
        characterNameText.text = "";

        // Cập nhật căn chỉ số qua bên trái
        characterStatsText.alignment = TextAlignmentOptions.Left;

        // Hiển thị thông tin chỉ số nhân vật (bao gồm cả chỉ số Magic, Critical Chance và Critical Damage)
        characterDescriptionText.text = character.description;
        characterStatsText.text = "<color=red>Health: </color>" + character.health +
                                  "<color=orange> \nAttack: </color> " + character.attack +
                                  "<color=blue> \nMana: </color> " + character.mana +
                                  "<color=green> \nDefense: </color> " + character.defense +
                                  "<color=purple> \nMagic: </color> " + character.magic +
                                  "<color=yellow>\nCritical Chance: </color> " + character.criticalChance + "%" +
                                  "<color=#663300>\nCritical Damage: </color> " + character.criticalDamage + "%";  // Thêm chỉ số chí mạng

        // Thêm hiệu ứng cho nút
        button.transform.localScale = new Vector3(1.2f, 1.2f, 1f);  // Phóng to nút
        button.GetComponent<Image>().color = new Color(1f, 0.5f, 0f);  // Đổi màu nút thành cam
    }

    // Hàm gọi khi chuột rời khỏi nút
    public void OnPointerExit(PointerEventData eventData)
    {
        // Khôi phục lại hình ảnh mặc định
        characterImage.sprite = defaultSprite;

        // Hiển thị lại tên nhân vật khi rời chuột
        characterNameText.text = character.name;

        // Xóa mô tả và chỉ số nhân vật khi rời chuột
        characterDescriptionText.text = "";
        characterStatsText.text = "";

        // Khôi phục lại hiệu ứng cho nút
        button.transform.localScale = new Vector3(1f, 1f, 1f);  // Quay lại kích thước ban đầu
        button.GetComponent<Image>().color = new Color(1f, 1f, 1f);  // Khôi phục lại màu trắng
    }

    // Lớp Character để lưu trữ thông tin nhân vật
    [System.Serializable]
    public class Character
    {
        public string name;        // Tên nhân vật
        public string description; // Mô tả nhân vật
        public int health;         // Chỉ số sức khỏe
        public int mana;           // Chỉ số mana
        public int attack;         // Chỉ số tấn công
        public float criticalChance; // Tỉ lệ chí mạng (đơn vị phần trăm)
        public float criticalDamage; // Dame chí mạng (đơn vị phần trăm)
        public int magic;          // Chỉ số magic
        public int defense;        // Chỉ số thủ (defense)

    }
}

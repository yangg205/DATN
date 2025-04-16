using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;


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
        if (character == null || characterImage == null || button == null || characterStatsText == null)
        {
            Debug.LogWarning("Thiếu tham chiếu trong CharacterButtonHover");
            return;
        }

        // Đổi sprite và hiển thị thông tin
        characterImage.sprite = hoverSprite;
        characterStatsText.color = Color.white;
        characterStatsText.alignment = TextAlignmentOptions.Left;

        characterDescriptionText.text = character.description ?? "No description";
        characterStatsText.text = $"<color=red>Health: </color>{character.health}" +
                                  $"\n<color=orange>Attack: </color>{character.attack}" +
                                  $"\n<color=blue>Mana: </color>{character.mana}" +
                                  $"\n<color=green>Defense: </color>{character.defense}" +
                                  $"\n<color=purple>Magic: </color>{character.magic}" +
                                  $"\n<color=yellow>Critical Chance: </color>{character.criticalChance}%" +
                                  $"\n<color=#663300>Critical Damage: </color>{character.criticalDamage}%";

        button.GetComponent<Image>().color = new Color(1f, 0.5f, 0f); // Đổi màu nút
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (character == null || characterImage == null || button == null || characterStatsText == null)
        {
            return;
        }

        characterImage.sprite = defaultSprite;
        characterNameText.text = character.name;
        characterDescriptionText.text = "";
        characterStatsText.text = "";

        button.GetComponent<Image>().color = Color.white; // Khôi phục màu gốc
    }



    // Lớp Character để lưu trữ thông tin nhân vật
    [System.Serializable]
    public class Character
    {
        public int id;           // ID nhân vật
        public string name;        // Tên nhân vật
        public string description; // Mô tả nhân vật
        public int health;         // Chỉ số sức khỏe
        public int mana;           // Chỉ số mana
        public int attack;         // Chỉ số tấn công
        public int attackspeed;    // chỉ số tốc đánh 
        public float criticalChance; // Tỉ lệ chí mạng (đơn vị phần trăm)
        public float criticalDamage; // Dame chí mạng (đơn vị phần trăm)
        public int magic;          // Chỉ số magic
        public int defense;        // Chỉ số thủ (defense)

    }
}









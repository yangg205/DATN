using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;


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
        public int attackspeed;    // chỉ số tốc đánh 
        public float criticalChance; // Tỉ lệ chí mạng (đơn vị phần trăm)
        public float criticalDamage; // Dame chí mạng (đơn vị phần trăm)
        public int magic;          // Chỉ số magic
        public int defense;        // Chỉ số thủ (defense)

    }
}





//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using TMPro;
//using Fusion;  // Đảm bảo rằng bạn đã thêm Fusion vào đầu script
//using UnityEngine.SceneManagement;  // Để sử dụng SceneManager cho chuyển scene

//public class CharacterButtonHover : SimulationBehaviour, IPointerEnterHandler, IPointerExitHandler
//{
//    // Các tham chiếu UI
//    public Image characterImage;  // Hình ảnh của nhân vật
//    public Sprite hoverSprite;    // Hình ảnh khi hover (di chuột vào)
//    public Sprite defaultSprite;  // Hình ảnh mặc định khi không hover

//    public TextMeshProUGUI characterNameText;  // Text hiển thị tên nhân vật
//    public TextMeshProUGUI characterDescriptionText;  // Text hiển thị mô tả
//    public TextMeshProUGUI characterStatsText; // Text hiển thị chỉ số nhân vật

//    public Button button;  // Nút chọn nhân vật

//    // Đối tượng Character để lưu trữ thông tin nhân vật
//    public Character character;

//    // Tham chiếu NetworkRunner để chuyển scene
//    public NetworkRunner networkRunner;

//    // Tên scene sẽ chuyển đến
//    public string sceneToLoad = "Duy1"; // Thay tên scene nếu cần

//    // Hàm gọi khi nhấn chọn nhân vật
//    public void OnCharacterSelected()
//    {
//        Debug.Log(Runner);
//        //Debug.Log(SceneRef);
//        //Debug.Log(LoadSceneMode);

//        Runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Single); // Scene 2   




//        // Kiểm tra nếu có NetworkRunner đã thiết lập
//        if (networkRunner != null)
//        {
//            //networkRunner.LoadScene(SceneRef.FromIndex(0), LoadSceneMode.Additive); // Scene 1



//            //SwitchScene();
//            // Kiểm tra nếu đang trong một session mạng hợp lệ
//            if (networkRunner.IsRunning)
//            {
//                Debug.Log(">>>");
//                //// Thực hiện chuyển scene với NetworkRunner trong Fusion
//                //SwitchScene();
//            }
//            else
//            {
//                Debug.LogError("NetworkRunner không đang chạy! Đảm bảo đã kết nối và khởi tạo session mạng.");
//            }
//        }
//        else
//        {
//            Debug.LogError("NetworkRunner chưa được thiết lập!");
//        }
//    }

//    // Hàm gọi khi chuột di vào nút
//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        characterStatsText.color = Color.white;

//        // Thay đổi hình ảnh khi hover vào
//        characterImage.sprite = hoverSprite;

//        // Ẩn tên nhân vật khi hover
//        characterNameText.text = "";

//        // Cập nhật căn chỉ số qua bên trái
//        characterStatsText.alignment = TextAlignmentOptions.Left;

//        // Hiển thị thông tin chỉ số nhân vật (bao gồm cả chỉ số Magic, Critical Chance và Critical Damage)
//        characterDescriptionText.text = character.description;
//        characterStatsText.text = "<color=red>Health: </color>" + character.health +
//                                  "<color=orange> \nAttack: </color> " + character.attack +
//                                  "<color=blue> \nMana: </color> " + character.mana +
//                                  "<color=green> \nDefense: </color> " + character.defense +
//                                  "<color=purple> \nMagic: </color> " + character.magic +
//                                  "<color=yellow>\nCritical Chance: </color> " + character.criticalChance + "%" +
//                                  "<color=#663300>\nCritical Damage: </color> " + character.criticalDamage + "%";  // Thêm chỉ số chí mạng

//        // Thêm hiệu ứng cho nút
//        button.transform.localScale = new Vector3(1.2f, 1.2f, 1f);  // Phóng to nút
//        button.GetComponent<Image>().color = new Color(1f, 0.5f, 0f);  // Đổi màu nút thành cam
//    }

//    // Hàm gọi khi chuột rời khỏi nút
//    public void OnPointerExit(PointerEventData eventData)
//    {
//        // Khôi phục lại hình ảnh mặc định
//        characterImage.sprite = defaultSprite;

//        // Hiển thị lại tên nhân vật khi rời chuột
//        characterNameText.text = character.name;

//        // Xóa mô tả và chỉ số nhân vật khi rời chuột
//        characterDescriptionText.text = "";
//        characterStatsText.text = "";

//        // Khôi phục lại hiệu ứng cho nút
//        button.transform.localScale = new Vector3(1f, 1f, 1f);  // Quay lại kích thước ban đầu
//        button.GetComponent<Image>().color = new Color(1f, 1f, 1f);  // Khôi phục lại màu trắng
//    }

//    // Hàm chuyển scene
//    private void SwitchScene()
//    {
//        try
//        {
//            // Kiểm tra nếu NetworkRunner đang chạy
//            if (networkRunner != null && networkRunner.IsRunning)
//            {
//                // Đảm bảo rằng bạn có quyền tải scene
//                if (networkRunner.IsSceneAuthority)
//                {
//                    // Tải các scene theo chế độ Additive
//                    networkRunner.LoadScene(SceneRef.FromIndex(0), LoadSceneMode.Additive); // Scene 1
//                    networkRunner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Additive); // Scene 2
//                    networkRunner.LoadScene(SceneRef.FromIndex(2), LoadSceneMode.Additive); // Scene 3

//                    Debug.Log("Đang tải 3 scene trong chế độ Additive...");
//                }
//                else
//                {
//                    Debug.LogError("Không có quyền tải scene trong mạng.");
//                }
//            }
//            else
//            {
//                Debug.LogError("NetworkRunner không đang chạy hoặc không được thiết lập!");
//            }
//        }
//        catch (System.Exception ex)
//        {
//            // Xử lý lỗi khi chuyển scene không thành công
//            Debug.LogError($"Lỗi khi chuyển scene: {ex.Message}");
//        }
//    }

//    // Lớp Character để lưu trữ thông tin nhân vật
//    [System.Serializable]
//    public class Character
//    {
//        public string name;        // Tên nhân vật
//        public string description; // Mô tả nhân vật
//        public int health;         // Chỉ số sức khỏe
//        public int mana;           // Chỉ số mana
//        public int attack;         // Chỉ số tấn công
//        public float criticalChance; // Tỉ lệ chí mạng (đơn vị phần trăm)
//        public float criticalDamage; // Dame chí mạng (đơn vị phần trăm)
//        public int magic;          // Chỉ số magic
//        public int defense;        // Chỉ số thủ (defense)
//    }
//}








//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using TMPro;
//using UnityEngine.SceneManagement;
//using Fusion;  // Thêm Fusion để sử dụng networkRunner

//public class CharacterButtonHover : SimulationBehaviour, IPointerEnterHandler, IPointerExitHandler
//{
//    // Các tham chiếu UI
//    public Image characterImage;  // Hình ảnh của nhân vật
//    public Sprite hoverSprite;    // Hình ảnh khi hover (di chuột vào)
//    public Sprite defaultSprite;  // Hình ảnh mặc định khi không hover

//    public TextMeshProUGUI characterNameText;  // Text hiển thị tên nhân vật
//    public TextMeshProUGUI characterDescriptionText;  // Text hiển thị mô tả
//    public TextMeshProUGUI characterStatsText; // Text hiển thị chỉ số nhân vật

//    public Button button;  // Nút chọn nhân vật
//    // Tham chiếu đến nút (Button) UI
//    public Button switchSceneButton;

//    // Đối tượng Character để lưu trữ thông tin nhân vật
//    public Character character;

//    // Tham chiếu tới networkRunner (Fusion)
//    public NetworkRunner networkRunner;  // Thêm tham chiếu tới NetworkRunner

//    // Tên scene sẽ chuyển đến
//    public string sceneToLoad = "Duy1"; // Thay tên scene nếu cần

//    void Start()
//    {
//        // Kiểm tra nếu button được gắn đúng cách
//        if (switchSceneButton != null)
//        {
//            // Đăng ký sự kiện OnClick cho button
//            switchSceneButton.onClick.AddListener(OnButtonClick);
//        }
//        else
//        {
//            Debug.LogError("Button chưa được gắn vào trong Inspector!");
//        }
//    }

//    // Hàm gọi khi chuột di vào nút
//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        characterStatsText.color = Color.white;

//        // Thay đổi hình ảnh khi hover vào
//        characterImage.sprite = hoverSprite;

//        // Ẩn tên nhân vật khi hover
//        characterNameText.text = "";

//        // Cập nhật căn chỉ số qua bên trái
//        characterStatsText.alignment = TextAlignmentOptions.Left;

//        // Hiển thị thông tin chỉ số nhân vật (bao gồm cả chỉ số Magic, Critical Chance và Critical Damage)
//        characterDescriptionText.text = character.description;
//        characterStatsText.text = "<color=red>Health: </color>" + character.health +
//                                  "<color=orange> \nAttack: </color> " + character.attack +
//                                  "<color=blue> \nMana: </color> " + character.mana +
//                                  "<color=green> \nDefense: </color> " + character.defense +
//                                  "<color=purple> \nMagic: </color> " + character.magic +
//                                  "<color=yellow>\nCritical Chance: </color> " + character.criticalChance + "%" +
//                                  "<color=#663300>\nCritical Damage: </color> " + character.criticalDamage + "%";  // Thêm chỉ số chí mạng

//        // Thêm hiệu ứng cho nút
//        button.transform.localScale = new Vector3(1.2f, 1.2f, 1f);  // Phóng to nút
//        button.GetComponent<Image>().color = new Color(1f, 0.5f, 0f);  // Đổi màu nút thành cam
//    }

//    // Hàm gọi khi chuột rời khỏi nút
//    public void OnPointerExit(PointerEventData eventData)
//    {
//        // Khôi phục lại hình ảnh mặc định
//        characterImage.sprite = defaultSprite;

//        // Hiển thị lại tên nhân vật khi rời chuột
//        characterNameText.text = character.name;

//        // Xóa mô tả và chỉ số nhân vật khi rời chuột
//        characterDescriptionText.text = "";
//        characterStatsText.text = "";

//        // Khôi phục lại hiệu ứng cho nút
//        button.transform.localScale = new Vector3(1f, 1f, 1f);  // Quay lại kích thước ban đầu
//        button.GetComponent<Image>().color = new Color(1f, 1f, 1f);  // Khôi phục lại màu trắng
//    }

//    //Hàm gọi khi nhấn nút để tải scene
//    void OnButtonClick()
//    {
//        // Kiểm tra quyền quản lý scene
//        if (networkRunner.IsSceneAuthority)
//        {
//            // Log quyền scene authority để debug
//            Debug.Log("Có quyền để tải scene!");

//            // Tải các scene theo chế độ Additive
//            //networkRunner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Additive); // Scene 1
//            networkRunner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Single); // Scene 2
//            //networkRunner.LoadScene(SceneRef.FromIndex(3), LoadSceneMode.Additive); // Scene 3

//            Debug.Log("Đang tải 3 scene trong chế độ Additive...");
//        }
//        else
//        {
//            // Log thông tin khi không có quyền
//            Debug.LogError("Không có quyền để tải scene. Kiểm tra quyền scene authority.");
//            Debug.Log("IsSceneAuthority: " + networkRunner.IsSceneAuthority);
//        }
//    }

//    // Lớp Character để lưu trữ thông tin nhân vật
//    [System.Serializable]
//    public class Character
//    {
//        public string name;        // Tên nhân vật
//        public string description; // Mô tả nhân vật
//        public int health;         // Chỉ số sức khỏe
//        public int mana;           // Chỉ số mana
//        public int attack;         // Chỉ số tấn công
//        public float criticalChance; // Tỉ lệ chí mạng (đơn vị phần trăm)
//        public float criticalDamage; // Dame chí mạng (đơn vị phần trăm)
//        public int magic;          // Chỉ số magic
//        public int defense;        // Chỉ số thủ (defense)
//    }
//}



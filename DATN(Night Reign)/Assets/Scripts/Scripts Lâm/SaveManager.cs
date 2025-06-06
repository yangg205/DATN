//using UnityEngine;
//using System.IO;

//public class SaveManager : MonoBehaviour
//{
//    private string savePath;
//    public SaveManager saveManager;

//    public void SaveGameFromPause()
//    {
//        if (saveManager != null)
//            saveManager.SaveGame();
//    }
//    private void Awake()
//    {
//        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
//    }

//    public void SaveGame()
//    {
//        // Dữ liệu giả để demo. Bạn có thể thay bằng dữ liệu thật như vị trí nhân vật, chỉ số,...
//        GameData data = new GameData
//        {
//            playerName = "Hero",
//            level = 3,
//            health = 100
//        };

//        string json = JsonUtility.ToJson(data, true);
//        File.WriteAllText(savePath, json);
//        Debug.Log("Game Saved to: " + savePath);
//    }
//}

//[System.Serializable]
//public class GameData
//{
//    public string playerName;

//    public int level;
//    public int health;
//    public int hp;
//    public int mp;
//    public int stamina;
//    public float staminaRegenRate;

//    public int damage;
//    public float attackSpeed;

//    public float critChance;
//    public float critDamage;

//    public int magic;
//    public int defense;
//}




using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SaveManager : MonoBehaviour
{
    private string savePath;

    [Header("UI Confirmation Panel")]
    public GameObject saveConfirmPanel; // Gán panel từ Canvas
    public Button yesButton;
    public Button noButton;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        // Gán sự kiện khi nút được bấm
        if (yesButton != null)
            yesButton.onClick.AddListener(OnYesClicked);

        if (noButton != null)
            noButton.onClick.AddListener(OnNoClicked);

        if (saveConfirmPanel != null)
            saveConfirmPanel.SetActive(false); // Tắt panel khi bắt đầu
    }

    public void SaveGameFromPause()
    {
        Debug.Log("SaveGameFromPause() được gọi");

        if (saveConfirmPanel != null)
        {
            saveConfirmPanel.SetActive(true); // Hiện panel xác nhận
        }
        else
        {
            Debug.LogWarning("saveConfirmPanel chưa được gán!");
        }
    }

    private void OnYesClicked()
    {
        Debug.Log("Đã Save Game");
        SaveGame();
        if (saveConfirmPanel != null)
            saveConfirmPanel.SetActive(false);
    }

    private void OnNoClicked()
    {
        Debug.Log("Không Save Game");

        if (saveConfirmPanel != null)
            saveConfirmPanel.SetActive(false);
    }

    public void SaveGame()
    {
        //GameData data = new GameData
        //{
        //    playerName = "Hero",
        //    level = 3,
        //    health = 100
        //};

        //string json = JsonUtility.ToJson(data, true);
        //File.WriteAllText(savePath, json);
        //Debug.Log("Game Saved to: " + savePath);
    }
}

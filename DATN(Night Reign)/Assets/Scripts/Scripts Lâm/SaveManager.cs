using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    private string savePath;
    public SaveManager saveManager;

    public void SaveGameFromPause()
    {
        if (saveManager != null)
            saveManager.SaveGame();
    }
    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    public void SaveGame()
    {
        // Dữ liệu giả để demo. Bạn có thể thay bằng dữ liệu thật như vị trí nhân vật, chỉ số,...
        GameData data = new GameData
        {
            playerName = "Hero",
            level = 3,
            health = 100
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved to: " + savePath);
    }
}

[System.Serializable]
public class GameData
{
    public string playerName;

    public int level;
    public int health;
    public int hp;
    public int mp;
    public int stamina;
    public float staminaRegenRate;

    public int damage;
    public float attackSpeed;

    public float critChance;
    public float critDamage;

    public int magic;
    public int defense;
}

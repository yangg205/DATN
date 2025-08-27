
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using AG;
using server.model;

public class SaveManager : MonoBehaviour
{
    private string savePath;

    [Header("UI Confirmation Panel")]
    public GameObject saveConfirmPanel; // Gán panel từ Canvas
    public Button yesButton;
    public Button noButton;
    SignalRClient SignalRClient;
    NotificationManager NotificationManager;
    PlayerStats playerStats;
    //GameObject ButtonYes;
    //SkillTreeManager SkillTreeManager;
    GameObject player;
    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        //ButtonYes = GameObject.Find("ButtonYes");
        SignalRClient = FindAnyObjectByType<SignalRClient>();
        NotificationManager = FindAnyObjectByType<NotificationManager>();
        playerStats = FindAnyObjectByType<PlayerStats>();
        //var skilltreeManager = FindAnyObjectByType<SkillTreeManager>();
        //var player = GameObject.FindGameObjectWithTag("Player").GetComponent<GameObject>();
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Cannot find GameObject with tag 'Player' in the scene!");
            }
        }
        if (SignalRClient is null)
        {
            Debug.Log("server null");
        }
        if (NotificationManager is null)
        {
            Debug.Log("notificate null");
        }
        if (playerStats is null)
        {
            Debug.Log("player state null");
        }
        //if (skilltreeManager is null)
        //{
        //    Debug.Log("skill tree null");
        //}
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

    public async void SaveGame()
    {
        if (player == null)
        {
            Debug.LogError("Cannot save game: player");
            return;
        }
        if (playerStats == null)
        {
            Debug.LogError("Cannot save game: playerStats");
            return;
        }
        if (SignalRClient == null)
        {
            Debug.LogError("Cannot save game: SignalRClient is missing!");
            return;
        }
        Vector3 playerPosition = player.transform.localPosition;
        Debug.Log($"Saving player position: ({playerPosition.x}, {playerPosition.y}, {playerPosition.z})");

        var playerCharacters = new Player_Characters
        {
            Player_Character_id = PlayerPrefs.GetInt("PlayerCharacterId", 0),
            Player_id = PlayerPrefs.GetInt("PlayerId", 0),
            Characters_id = PlayerPrefs.GetInt("Character", 0),
            Ownershipdate = DateTime.Now,
            Current_hp = playerStats.currentHealth,
            Total_point = 0,
            Total_coin = 0,
            Skill_Point = 0,
            Position_x = playerPosition.x,
            Position_y = playerPosition.y,
            Position_z = playerPosition.z,
            Datesave = DateTime.Now,
        };

        var result = await SignalRClient.SaveGame(playerCharacters);
        if (result.status)
        {
            NotificationManager.ShowNotification(result.message, 1);
        }
        else
        {
            Debug.LogError($"Failed to save game: {result.message}");
            NotificationManager.ShowNotification($"Failed to save game: {result.message}", 1);
        }
    }
}

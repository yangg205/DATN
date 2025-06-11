//using UnityEditor.SceneManagement;
//using UnityEngine;
//using UnityEngine.UI;

//public class SaveConfirm : MonoBehaviour
//{
//    SignalRClient SignalRClient;
//    NotificationManager NotificationManager;
//    PlayerStats playerStats;
//    GameObject ButtonYes;
//    //SkillTreeManager SkillTreeManager;
//    GameObject player;
//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        ButtonYes = GameObject.Find("ButtonYes");
//        SignalRClient = FindAnyObjectByType<SignalRClient>();
//        NotificationManager = FindAnyObjectByType<NotificationManager>();
//        playerStats = FindAnyObjectByType<PlayerStats>();
//        //var skilltreeManager = FindAnyObjectByType<SkillTreeManager>();
//        var player = GameObject.FindGameObjectWithTag("Player").GetComponent<GameObject>();
//        if(SignalRClient is null)
//        {
//            Debug.Log("server null");
//        }
//        if (NotificationManager is null)
//        {
//            Debug.Log("notificate null");
//        }
//        if (playerStats is null)
//        {
//            Debug.Log("player state null");
//        }
//        //if (skilltreeManager is null)
//        //{
//        //    Debug.Log("skill tree null");
//        //}
//        if(player is null)
//        {
//            Debug.Log("player null");
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        ButtonYes.GetComponent<Button>().onClick.AddListener(SaveOnclick);
//    }
//    public async void SaveOnclick()
//    {
//        double local_x = player.transform.position.x;
//        double local_y = player.transform.position.y;
//        double local_z = player.transform.position.z;
//        Debug.Log($"{local_x},{local_y},{local_z}");
//        var playerCharacters = new Player_Characters
//        {
//            Player_Character_id = PlayerPrefs.GetInt("PlayerCharacterId", 0),
//            Current_hp = playerStats.currentHealth,
//            Current_exp = playerStats.currentEXP,
//            level = playerStats.playerLevel,
//            Total_point = 0,
//            Total_coin = 0,
//            Skill_Point = 0,
//            Position_x = local_x,
//            Position_y = local_y,
//            Position_z = local_z
//        };
//        var result = await SignalRClient.SaveGame(playerCharacters);
//        if (result.status)
//        {
//            NotificationManager.ShowNotification(result.message, 4);
//        }
//    }
//}

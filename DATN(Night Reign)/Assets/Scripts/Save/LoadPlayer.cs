using UnityEngine;
using AG; 

public class LoadPlayer : MonoBehaviour
{
    [SerializeField] GameObject playerObject;

   public AG.PlayerStats playerStatsComponent; 
    void Start()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
            {
                Debug.LogError("LoadPlayer: Không tìm thấy GameObject có tag 'Player'. Không thể tải dữ liệu vị trí.");
                return;
            }
        }

        playerStatsComponent = playerObject.GetComponent<AG.PlayerStats>();

        if (playerStatsComponent == null)
        {
            Debug.LogError("LoadPlayer: GameObject 'Player' không có component PlayerStats. Không thể tải dữ liệu chỉ số.");
            return;
        }

        if (GameDataHolder.LoadedPlayerCharactersData != null)
        {
            Debug.Log("LoadPlayer: Phát hiện dữ liệu game đã tải. Đang áp dụng...");

            playerStatsComponent.currentHealth = GameDataHolder.LoadedPlayerCharactersData.Current_hp;
            playerStatsComponent.currentEXP = GameDataHolder.LoadedPlayerCharactersData.Current_exp;
            playerStatsComponent.playerLevel = GameDataHolder.LoadedPlayerCharactersData.level;
            playerStatsComponent.soulCount = GameDataHolder.LoadedPlayerCharactersData.Total_coin;
            Vector3 loadedPosition = new Vector3(
                (float)GameDataHolder.LoadedPlayerCharactersData.Position_x,
                (float)GameDataHolder.LoadedPlayerCharactersData.Position_y,
                (float)GameDataHolder.LoadedPlayerCharactersData.Position_z
            );
            playerObject.transform.localPosition = loadedPosition;

            Debug.LogWarning($"LoadPlayer: Đã áp dụng HP: {playerStatsComponent.currentHealth}, EXP: {playerStatsComponent.currentEXP}, Level: {playerStatsComponent.playerLevel}");
            Debug.LogWarning($"LoadPlayer: Đã đặt Player tại vị trí: ({loadedPosition.x}, {loadedPosition.y}, {loadedPosition.z})");

            GameDataHolder.ClearLoadedData();
        }
        else
        {
            Debug.Log("LoadPlayer: Không có dữ liệu game đã tải. Khởi tạo player với giá trị mặc định của scene.");

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
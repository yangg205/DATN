using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using ND;
using UnityEngine.SceneManagement;

public class AutoOutline : MonoBehaviour
{
    [SerializeField] int characterId;
    private Outline outline;
    private bool isSelected = false;
    [SerializeField] bool status= false;
    SignalRClient signalRClient;
    [SerializeField] NotificationManager NotificationManager; // Thêm tham chiếu đến NotificationManager
    int playerId;
    private void Start()
    {
        NotificationManager = FindAnyObjectByType<NotificationManager>();
        playerId = PlayerPrefs.GetInt("PlayerId", 0);
        signalRClient = FindAnyObjectByType<SignalRClient>();
        outline = GetComponent<Outline>();
        SelectedCharacter();

    }

    private void Update()
    {
        //if (status == true)
        //{
        //    if (outline != null)
        //    {
        //        outline.enabled = true; // Hiển thị sẵn Outline
        //        outline.effectColor = Color.yellow; // Có thể đổi màu nếu muốn khác lúc hover
        //    }
        //}
    }
    private async void SelectedCharacter()
    {
        var result = await signalRClient.SelectedCharacter(playerId, characterId);
        if (result.status)
        {
            if (outline != null)
            {
                outline.enabled = true; // Hiển thị sẵn Outline
                outline.effectColor = Color.yellow; // Có thể đổi màu nếu muốn khác lúc hover
            }
        }
        //else
        //{
        //    outline.enabled = false; // Hiển thị sẵn Outline
        //    outline.effectColor = Color.yellow; // Có thể đổi màu nếu muốn khác lúc hover
        //}
        
    }
    public void Deselect()
    {
        isSelected = false;
        if (outline != null)
        {
            outline.effectColor = Color.yellow; // Quay về màu hover ban đầu
        }
    }
    public async void Continue()
    {

        Debug.Log($"Đang cố gắng tải dữ liệu game cho PlayerID: {playerId}, CharacterID: {characterId}");
        var result = await signalRClient.ContinueGame(playerId, characterId);

        if (result.status && result.player_Characters != null)
        {
            PlayerPrefs.SetInt("PlayerCharacterId", result.player_Characters.Player_Character_id);
            GameDataHolder.LoadedPlayerCharactersData = result.player_Characters;
            PlayerPrefs.SetString("SceneToLoad", "VucLavareach");
            if (NotificationManager != null) NotificationManager.ShowNotification(result.message, 2);
            Debug.Log("Dữ liệu game đã tải thành công và được lưu trữ tạm thời.");
            Debug.Log($"HP tải về: {GameDataHolder.LoadedPlayerCharactersData.Current_hp}");
            SceneManager.LoadScene("VucLavareach"); // Chuyển sang scene VucLavareach

        }
        else
        {
            // Nếu result.status là false, nghĩa là có lỗi đã xảy ra và được xử lý ở tầng SignalRClient.
            GameDataHolder.ClearLoadedData(); // Xóa dữ liệu cũ nếu tải thất bại
            Debug.LogError($"Tải game thất bại: {result.message}");
            if (NotificationManager != null) NotificationManager.ShowNotification($"Tải game thất bại: {result.message}", 3);
        }
    }
}


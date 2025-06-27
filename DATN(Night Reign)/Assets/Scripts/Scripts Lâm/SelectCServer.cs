using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectCServer : MonoBehaviour
{
    SignalRClient SignalRClient;
    private int Titanos = 5;
    private int Sylas = 1005;
    private int Zarathos = 1006;
    private void Awake()
    {
        SignalRClient = FindAnyObjectByType<SignalRClient>();
        if (SignalRClient == null)
        {
            Debug.LogError("❌ Không tìm thấy SignalRClient trong scene!");
        }
    }
    public async void SelectCharacter1()
    {
        var playerId = PlayerPrefs.GetInt("PlayerId", 0);
        if (SignalRClient == null)
        {
            Debug.LogError("❌ SignalRClient không được khởi tạo!");
            return;
        }
        var result = await SignalRClient.SelectCharacter(playerId, Titanos);
        Debug.Log($"✅ Chọn nhân vật thành công: {result}");
        if (result.status)
        {
            PlayerPrefs.SetInt("PlayerCharacterId", result.player_Characters.Player_Character_id);
            PlayerPrefs.SetInt("Character", Titanos);
            SceneManager.LoadScene("Name Scene");
        }
        else
        {
            Debug.LogError($"❌ Lỗi khi chọn nhân vật: {result.message}");
        }
    }
    public async void SelectCharacter2()
    {
        var playerId = PlayerPrefs.GetInt("PlayerId", 0);
        if (SignalRClient == null)
        {
            Debug.LogError("❌ SignalRClient không được khởi tạo!");
            return;
        }
        var result = await SignalRClient.SelectCharacter(playerId, Sylas);
        Debug.Log($"✅ Chọn nhân vật thành công: {result}");
        if (result.status)
        {
            PlayerPrefs.SetInt("PlayerCharacterId", result.player_Characters.Player_Character_id);
            PlayerPrefs.SetInt("Character", Sylas);
            SceneManager.LoadScene("Name Scene");
        }
        else
        {
            Debug.LogError($"❌ Lỗi khi chọn nhân vật: {result.message}");
        }
    }
    public async void SelectCharacter3()
    {
        var playerId = PlayerPrefs.GetInt("PlayerId", 0);
        
        if (SignalRClient == null)
        {
            Debug.LogError("❌ SignalRClient không được khởi tạo!");
            return;
        }
        var result = await SignalRClient.SelectCharacter(playerId, Zarathos);
        Debug.Log($"✅ Chọn nhân vật thành công: {result}");
        if (result.status)
        {
            PlayerPrefs.SetInt("PlayerCharacterId", result.player_Characters.Player_Character_id);
            PlayerPrefs.SetInt("Character", Zarathos);
            SceneManager.LoadScene("Name Scene");
        }
        else
        {
            Debug.LogError($"❌ Lỗi khi chọn nhân vật: {result.message}");
        }
    }
}

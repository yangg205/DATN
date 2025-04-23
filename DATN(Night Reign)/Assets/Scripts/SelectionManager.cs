using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static AuthManager;

public class SelectionManager : MonoBehaviour
{
    //còn thiếu show lỗi
    public TMP_InputField nameInputField;
    private CharacterButtonHover characterButtonHover;
    public Button ButtonTank;
    public Button ButtonDame;
    public Button ButtonMagic;
    public int playerid; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
        ButtonTank.onClick.AddListener(() => OnButtonClick("Tank",5));
        ButtonDame.onClick.AddListener(() => OnButtonClick("Dame", 1));
        ButtonMagic.onClick.AddListener(() => OnButtonClick("Magic", 0));
        
    }

    // Update is called once per frame
    void OnButtonClick(string playerClass, int characterid)
    {
        // Đọc tên người chơi từ input field
        var playerName = nameInputField.text;
        int id = PlayerPrefs.GetInt("PlayerId", -1); // Lấy playerid từ PlayerPrefs
        // Lấy playerid từ PlayerPrefs
        playerid = PlayerPrefs.GetInt("PlayerId", -1); // Nếu không tìm thấy, mặc định là -1

        if (playerid == -1)
        {
            Debug.LogError("Player ID không hợp lệ!");
            return;
        }

        // Thực hiện gọi API chọn nhân vật và chuyển sang scene
        StartCoroutine(SelectCharacter(id, characterid));

        // Chuyển sang scene "Duy"
        SceneManager.LoadScene("Duy");
        playerid = id;
        Debug.Log("Player ID: " + playerid);

    }

    public IEnumerator SelectCharacter(int playerid, int characterid)
    {
        var requestData = new PlayerCharacter
        {
            player_id = playerid,
            character_id = characterid
        };

        string json = JsonUtility.ToJson(requestData);

        UnityWebRequest request = new UnityWebRequest("http://yang2005-001-site1.ntempurl.com/SelecteCharacter", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 👉 Thêm Header Authorization kiểu Basic Auth
        string auth = "11239336:60-dayfreetrial";
        string authBase64 = System.Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
        request.SetRequestHeader("Authorization", "Basic " + authBase64);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Chọn nhân vật response: {request.downloadHandler.text}");
            var response = JsonUtility.FromJson<ReturnPlayerResponse>(request.downloadHandler.text);

            if (response.data.status)
            {
                Debug.Log("✅ Chọn nhân vật thành công");
            }
            else
            {
                Debug.LogWarning("❌ Chọn nhân vật thất bại: " + response.data.message);
            }
        }
        else
        {
            Debug.LogError($"❌ Lỗi kết nối: {request.error}");
        }
    }


    [System.Serializable]
    public class PlayerCharacter
    {
        public int player_id;
        public int character_id;
    }
}

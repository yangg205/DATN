using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class NameInputHandler : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInputField; // Ô nhập tên
    public TextMeshProUGUI warningText;   // Text thông báo "Hãy nhập tên!"

    [Header("Scene Settings")]
    public string sceneToLoad = "GameScene"; // Tên scene cần load

    void Start()
    {
        if (warningText != null)
            warningText.gameObject.SetActive(false); // Ẩn cảnh báo khi mới bắt đầu
    }

    public void OnConfirmButtonClick()
    {
        string playerName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            // Hiện thông báo yêu cầu nhập tên
            if (warningText != null)
            {
                warningText.text = "Hãy nhập tên nhân vật trước khi chơi !!";
                warningText.gameObject.SetActive(true);
            }
        }
        else
        {
            // Ẩn thông báo và lưu tên người chơi
            if (warningText != null)
                warningText.gameObject.SetActive(false);

            PlayerPrefs.SetString("PlayerName", playerName); // Lưu tên

            // Chuyển scene
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}

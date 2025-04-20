using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    public TMP_InputField nameInputField;

    public Button ButtonTank;
    public Button ButtonDame;
    public Button ButtonMagic;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ButtonTank.onClick.AddListener(() => OnButtonClick("Tank"));
        ButtonDame.onClick.AddListener(() => OnButtonClick("Dame"));
        ButtonMagic.onClick.AddListener(() => OnButtonClick("Magic"));
    }

    // Update is called once per frame
    void OnButtonClick(string playerClass)
    {
        // đọc tên người chơi từ input field
        var playerName = nameInputField.text;
        // lưu thông tin người chơi 
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetString("PlayerClass", playerClass);
        SceneManager.LoadScene("Duy");
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    // Panels
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject otpPanel;
    public GameObject thongBaoPanel;

    // Toggle Buttons
    public Button showLoginButton;
    public Button showRegisterButton;

    // Login UI Elements
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Button loginButton;

    // Register UI Elements
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerRepasswordInput;
    public Button registerButton;

    // OTP UI Elements
    public TMP_InputField otpInput;
    public Button otpSubmitButton;

    // Notification UI Elements
    public TMP_Text thongBaoText;

    // Backend API Base URL
    private string baseUrl = "http://localhost:7102/api/player";

    private string currentEmail; // To store email for OTP verification

    void Start()
    {
        // Assign button listeners for toggle buttons
        showLoginButton.onClick.AddListener(ShowLoginPanel);
        showRegisterButton.onClick.AddListener(ShowRegisterPanel);

        // Assign button listeners for actions
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        otpSubmitButton.onClick.AddListener(OnOtpSubmitButtonClicked);

        // Hide all panels at start
        HideAllPanels();
    }

    // Hide all panels
    void HideAllPanels()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        otpPanel.SetActive(false);
        thongBaoPanel.SetActive(false);
    }

    // Show/Hide Panels
    void ShowLoginPanel()
    {
        HideAllPanels();
        loginPanel.SetActive(true);
    }

    void ShowRegisterPanel()
    {
        HideAllPanels();
        registerPanel.SetActive(true);
    }

    void ShowOtpPanel()
    {
        HideAllPanels();
        otpPanel.SetActive(true);
    }

    void ShowThongBao(string message)
    {
        HideAllPanels();
        thongBaoText.text = message;
        thongBaoPanel.SetActive(true);
        StartCoroutine(HideThongBaoAfterDelay(3f)); // Ẩn sau 3 giây
    }

    // Coroutine để ẩn thông báo sau một khoảng thời gian
    IEnumerator HideThongBaoAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        thongBaoPanel.SetActive(false);
    }

    // Button Click Handlers
    void OnLoginButtonClicked()
    {
        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text.Trim();

        // Validate inputs
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowThongBao("Vui lòng nhập đầy đủ email và mật khẩu!");
            return;
        }

        StartCoroutine(Login(email, password));
    }

    void OnRegisterButtonClicked()
    {
        string email = registerEmailInput.text.Trim();
        string password = registerPasswordInput.text.Trim();
        string repassword = registerRepasswordInput.text.Trim();

        // Validate inputs
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(repassword))
        {
            ShowThongBao("Vui lòng nhập đầy đủ email, mật khẩu và xác nhận mật khẩu!");
            return;
        }

        if (password != repassword)
        {
            ShowThongBao("Mật khẩu xác nhận không khớp!");
            return;
        }

        currentEmail = email; // Store email for OTP verification
        StartCoroutine(Register(email, password, repassword));
    }

    void OnOtpSubmitButtonClicked()
    {
        string otp = otpInput.text.Trim();

        // Validate inputs
        if (string.IsNullOrEmpty(currentEmail) || string.IsNullOrEmpty(otp))
        {
            ShowThongBao("Vui lòng nhập mã OTP!");
            return;
        }

        StartCoroutine(VerifyOtp(currentEmail, otp));
    }

    // HTTP Requests
    IEnumerator Register(string email, string password, string repassword)
    {
        var requestData = new PlayerRegisterModel
        {
            Email = email,
            Password = password,
            Repassword = repassword
        };
        string json = JsonUtility.ToJson(requestData);
        Debug.Log($"Register JSON: {json}");

        UnityWebRequest request = new UnityWebRequest($"{baseUrl}/register", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Register response: {request.downloadHandler.text}");
            HandleResponse(request.downloadHandler.text, ShowOtpPanel);
        }
        else
        {
            Debug.LogError($"Register failed: {request.error}");
            ShowThongBao("Lỗi kết nối: " + request.error);
        }
    }

    IEnumerator VerifyOtp(string email, string otp)
    {
        var requestData = new VerifyOtpModel
        {
            Email = email,
            OTP = otp
        };
        string json = JsonUtility.ToJson(requestData);
        Debug.Log($"VerifyOtp JSON: {json}");

        UnityWebRequest request = new UnityWebRequest($"{baseUrl}/verifyOtp", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"VerifyOtp response: {request.downloadHandler.text}");
            HandleResponse(request.downloadHandler.text, ShowLoginPanel);
        }
        else
        {
            Debug.LogError($"VerifyOtp failed: {request.error}");
            ShowThongBao("Lỗi kết nối: " + request.error);
        }
    }

    IEnumerator Login(string email, string password)
    {
        var requestData = new PlayerLoginModel
        {
            Email = email,
            Password = password
        };
        string json = JsonUtility.ToJson(requestData);
        Debug.Log($"Login JSON: {json}");

        UnityWebRequest request = new UnityWebRequest($"{baseUrl}/login", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Login response: {request.downloadHandler.text}");
            HandleResponse(request.downloadHandler.text, null);
            SceneManager.LoadScene("Menu");
        }
        else
        {
            Debug.LogError($"Login failed: {request.error}");
            ShowThongBao("Lỗi kết nối: " + request.error);
        }
    }

    // Handle response JSON
    void HandleResponse(string responseJson, System.Action onSuccess)
    {
        var response = JsonUtility.FromJson<ReturnPlayerResponse>(responseJson);
        ShowThongBao(response.data.message);
        if (response.data.status && onSuccess != null)
        {
            onSuccess();
        }
    }
}

// Request Models
[System.Serializable]
public class PlayerRegisterModel
{
    public string Email;
    public string Password;
    public string Repassword;
}

[System.Serializable]
public class VerifyOtpModel
{
    public string Email;
    public string OTP;
}

[System.Serializable]
public class PlayerLoginModel
{
    public string Email;
    public string Password;
}

// Response Models
[System.Serializable]
public class ReturnPlayerResponse
{
    public ReturnPlayer data;
}

[System.Serializable]
public class ReturnPlayer
{
    public bool status;
    public string message;
    public Player Player;
}

[System.Serializable]
public class Player
{
    public string Email;
    public string Password;
    public string Name;
    public decimal Total_Money;
}

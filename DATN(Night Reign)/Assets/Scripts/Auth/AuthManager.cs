using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    [SerializeField] GameObject loginButton;
    [SerializeField] GameObject registerButton;

    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject registerPanel;
    [SerializeField] GameObject otpPanel;
    NotificationManager notificationManager;

    //input fields login
    [SerializeField] TMP_InputField emailLogin;
    [SerializeField] TMP_InputField passwordLogin;
    //input fields login
    [SerializeField] TMP_InputField emailRegister;
    [SerializeField] TMP_InputField passwordRegister;
    [SerializeField] TMP_InputField confirmPasswordRegister;
    //input fields otp
    [SerializeField] TMP_InputField otpCode;

    private bool isEmailLoaded = false;
    //server
    private SignalRClient signalRClient;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loginButton.SetActive(true);
        registerButton.SetActive(true);
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        otpPanel.SetActive(false);
        signalRClient = FindAnyObjectByType<SignalRClient>();
        if (signalRClient == null)
        {
            Debug.LogError("SignalRClient khong co trong scene.");
        }
        notificationManager = FindAnyObjectByType<NotificationManager>();


        string playerMail = PlayerPrefs.GetString("email", ""); // Lấy email từ PlayerPrefs
        if (!string.IsNullOrEmpty(playerMail))
        {
            emailLogin.text = playerMail;
            isEmailLoaded = true;
        }
    }
    // Update is called once per frame
    void Update()
    {


        // Gán sự kiện khi bắt đầu chỉnh sửa
        emailLogin.onValueChanged.AddListener(OnEmailChanged);
    }
    public void OnClickShowLoginInner()
    {
        loginPanel.SetActive(true);
        otpPanel.SetActive(false);
        registerPanel.SetActive(false);
        SetButtonsVisible(false);


    }
    public void OnClickShowRegisterInner()
    {
        registerPanel.SetActive(true);
        otpPanel.SetActive(false);
        loginPanel.SetActive(false);
        SetButtonsVisible(false);

    }
    public void OnClickShowOtpInner()
    {
        otpPanel.SetActive(true);
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        SetButtonsVisible(false);
    }
    public void OnClickExit()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        otpPanel.SetActive(false);
        SetButtonsVisible(true);

    }
    public async void OnClickSubmitLogin()
    {
        string email = emailLogin.text.Trim(); 
        string password = passwordLogin.text.Trim();
        PlayerPrefs.SetString("email", email); 
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            
            notificationManager.ShowNotification("Vui lòng nhập đầy đủ email và mật khẩu!", 4);
            return;
        }
        var result = await signalRClient.SendLogin(email, password);
        if (result.status)
        {
            PlayerPrefs.SetInt("PlayerId", result.Player.Player_id);
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            notificationManager.ShowNotification(result.message, 4);
        }
    }
    public async void OnClickSubmiRegister()
    {

        string email = emailRegister.text.Trim();
        string password = passwordRegister.text.Trim();
        string confirmPassword = confirmPasswordRegister.text.Trim();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            notificationManager.ShowNotification("Vui lòng nhập đầy đủ email và mật khẩu!", 4);
            return;
        }
        var result = await signalRClient.SendRegister(email, password, confirmPassword);
        if (result.status)
        {
            notificationManager.ShowNotification("Kiểm tra email để lấy OTP", 4);
            PlayerPrefs.SetString("email", email);
            otpPanel.SetActive(true);
            registerPanel.SetActive(false);
        }
        else
        {
            notificationManager.ShowNotification(result.message, 2);
        }
    }
    public async void OnClickSubmitOtp()
    {
        string email = PlayerPrefs.GetString("email");
        string otp = otpCode.text.Trim();
        if (string.IsNullOrEmpty(email))
        {
            notificationManager.ShowNotification("Vui lòng đăng ký trước!", 4);
            return;
        }
        if (string.IsNullOrEmpty(otp))
        {
            notificationManager.ShowNotification("Vui lòng nhập đầy đủ OTP!", 4);
            return;
        }
        var result = await signalRClient.SendOTP(email, otp);
        if (result.status)
        {
            notificationManager.ShowNotification("Đăng ký thành công!", 4);
            otpPanel.SetActive(false);
            loginPanel.SetActive(true);
        }
        else
        {
            notificationManager.ShowNotification(result.message, 2);
        }
        string playerMail = PlayerPrefs.GetString("email", ""); 
        if (!string.IsNullOrEmpty(playerMail))
        {
            emailLogin.text = playerMail;
            isEmailLoaded = true;
        }
    }
    void OnEmailChanged(string input)
    {
        if (isEmailLoaded && string.IsNullOrEmpty(input))
        {
            isEmailLoaded = false;
        }
    }

    void SetButtonsVisible(bool visible)
    {
        loginButton.SetActive(visible);
        registerButton.SetActive(visible);
    }

}


using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
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

    //server
    private SignalRClient signalRClient;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        otpPanel.SetActive(false);
        signalRClient = FindAnyObjectByType<SignalRClient>();
        if (signalRClient == null)
        {
            Debug.LogError("SignalRClient khong co trong scene.");
        }
        notificationManager = FindAnyObjectByType<NotificationManager>();
    }
    // Update is called once per frame
    void Update()
    {
        string playerMail = PlayerPrefs.GetString("email");
        if (playerMail.Contains("@"))
        {
            emailLogin.text = $"{playerMail}";
        }
    }
    public void OnClickShowLoginInner()
    {
        loginPanel.SetActive(true);
        otpPanel.SetActive(false);
        registerPanel.SetActive(false);

    }
    public void OnClickShowRegisterInner()
    {
        registerPanel.SetActive(true);
        otpPanel.SetActive(false);
        loginPanel.SetActive(false);
    }
    public void OnClickShowOtpInner()
    {
        otpPanel.SetActive(true);
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
    }
    public void OnClickExit()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        otpPanel.SetActive(false);
    }
    public async void OnClickSubmitLogin()
    {
        string email = emailLogin.text.Trim(); 
        string password = passwordLogin.text.Trim();
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
    }
}


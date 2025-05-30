using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;

public class SignalRClient : MonoBehaviour
{
    private static SignalRClient instance; // Đảm bảo chỉ có 1 SignalRClient duy nhất
    private HubConnection _connection;
    private const string HubUrl = "http://localhost:7102/gamehub";
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private async void Start()
    {
        // Khởi tạo kết nối SignalR
        _connection = new HubConnectionBuilder()
            .WithUrl($"{HubUrl}", options =>
            {
                options.SkipNegotiation = true;
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
            })
            .WithAutomaticReconnect()
            .Build();

        // Đăng ký sự kiện nhận tin nhắn từ server
        _connection.On<string>("ReceiveMessage", (message) =>
        {
            Debug.Log($"Tin nhắn từ server: {message}");
        });

        // Kết nối tới server
        try
        {
            await _connection.StartAsync();
            Debug.Log("✅ Kết nối thành công!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Kết nối thất bại: {ex.Message}");
        }
    }
    public async 

    public async Task<ReturnPlayerCharacterSkill> SendUpdateSkill(int playerCharacterId, int skillTreeId)
    {
        try
        {
            var result = await _connection.InvokeAsync<ReturnPlayerCharacterSkill>(
                "UpdateSkill", playerCharacterId, skillTreeId);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

            return result; // Trả về kết quả cập nhật kỹ năng
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Gửi yêu cầu thất bại: {ex.Message}");
            throw; // Ném lại ngoại lệ để xử lý ở nơi gọi
        }
    }
    public async Task<ReturnPlayer> SendLogin(string email, string password)
    {
        try
        {
            //gui ket qua tu server
            var result = await _connection.InvokeAsync<ReturnPlayer>("Login", email, password);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result; // Trả về kết quả đăng nhập
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Gửi yêu cầu đăng nhập thất bại: {ex.Message}");
            throw;
        }
    }
    public async Task<ReturnPlayer> SendRegister(string email, string password, string repassword)
    {
        try
        {
            //gui ket qua tu server
            var result = await _connection.InvokeAsync<ReturnPlayer>("RegisterPendingPlayer", email, password, repassword);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result; // Trả về kết quả đăng ký
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Gửi yêu cầu đăng nhập thất bại: {ex.Message}");
            throw;
        }
    }
    public async Task<ReturnPlayer> SendOTP(string email, string otp)
    {
        try
        {
            //gui ket qua tu server
            var result = await _connection.InvokeAsync<ReturnPlayer>("VerifyOtpAndRegister", email, otp);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result; // Trả về kết quả otp
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Gửi yêu cầu đăng nhập thất bại: {ex.Message}");
            throw;
        }
    }
    private async void OnDestroy()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            Debug.Log("Ngắt kết nối SignalR.");
        }
    }
}

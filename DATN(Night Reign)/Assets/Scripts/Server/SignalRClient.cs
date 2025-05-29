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

    private async void Start()
    {
        // Kiểm tra nếu đã có một instance tồn tại
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Hủy đối tượng thừa
            return;
        }

        // Đặt instance và giữ nó không bị phá hủy
        instance = this;
        DontDestroyOnLoad(gameObject);

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

    public async Task SendUpdateSkill(int playerCharacterId, int skillTreeId)
    {
        try
        {
            var result = await _connection.InvokeAsync<ReturnPlayerCharacterSkill>(
                "UpdateSkill", playerCharacterId, skillTreeId);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Debug.Log($"Phản hồi từ server:\n{jsonResult}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Gửi yêu cầu thất bại: {ex.Message}");
        }
    }
    public async Task SendLogin(string email, string password)
    {
        try
        {
            var result = await _connection.InvokeAsync<ReturnPlayer>("Login", email, password);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Debug.Log($"Phản hồi từ server:\n{jsonResult}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Gửi yêu cầu đăng nhập thất bại: {ex.Message}");
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

using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text.Json; // Để serialize JSON
using System.Threading.Tasks;
using UnityEngine;

// Class giống như ReturnPlayerCharacterSkill bên backend
public class ReturnPlayerCharacterSkill
{
    public bool status { get; set; }
    public string message { get; set; }
    public object Player_Character_Skill { get; set; }
}

public class SignalRClient : MonoBehaviour
{
    private HubConnection _connection;

    private async void Start()
    {
        // Khởi tạo kết nối SignalR
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:7102/gamehub", options =>
            {
                options.SkipNegotiation = true; // Bỏ qua negotiation cho HTTP
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
            })
            .WithAutomaticReconnect() // Tự động kết nối lại nếu mất kết nối
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
            // Gọi phương thức UpdateSkill và nhận kết quả kiểu ReturnPlayerCharacterSkill
            var result = await _connection.InvokeAsync<ReturnPlayerCharacterSkill>(
                "UpdateSkill", playerCharacterId, skillTreeId);

            // Serialize kết quả thành JSON để dễ đọc
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Debug.Log($"Phản hồi từ server:\n{jsonResult}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Gửi yêu cầu thất bại: {ex.Message}");
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
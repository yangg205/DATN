using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
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
    public async Task<ReturnPlayerCharacter> SelectCharacter(int playerId, int characterId)
    {
        try
        {
            var result = await _connection.InvokeAsync<ReturnPlayerCharacter>("SelectCharacter", playerId, characterId);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result; // Trả về kết quả chọn nhân vật
        }
        catch (Exception ex)
        {
            Debug.Log("Gui yeu cau chon nhan vat that bai"+ex.Message);
            throw;
        }
    }
    public async Task<ReturnPlayerCharacter> SaveGame(Player_Characters playerCharacters)
    {
        try
        {
            var result = await _connection.InvokeAsync<ReturnPlayerCharacter>("SaveGame", playerCharacters);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result; // Trả về kết quả chọn nhân vật
        }
        catch(Exception ex)
        {
            Debug.Log("Gui yeu cau that bai" + ex.Message);
            throw;
        }
    }

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
            Debug.Log($"❌ Gửi yêu cầu thất bại: {ex.Message}");
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
    public async Task<ReturnPlayerCharacter> ContinueGame(int playerId, int characterId)
    {
        try
        {
            var result = await _connection.InvokeAsync<ReturnPlayerCharacter>("ContinueGame", playerId, characterId);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result; // Trả về kết quả tiếp tục trò chơi
        }
        catch(Exception ex)
        {
            Debug.Log("gui yeu cau continue that bai "+ex.Message);
            throw;
        }
    }
    public async Task<List<Ranking>> RankingPlayerCharacter(int characterId)
    {
        try
        {
            var result = await _connection.InvokeAsync<List<Ranking>>("RankingPlayerCharacter", characterId);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result; // Trả về danh sách xếp hạng nhân vật
        }
        catch(Exception ex)
        {
            Debug.Log("gui yeu cau ranking that bai" + ex.Message);
            throw;
        }
    }
    public async Task<PlayerCharacterSkillPoint> GetSkillPoint(int playerCharacterId)
    {
        try
        {
            var result = await _connection.InvokeAsync<PlayerCharacterSkillPoint>("GetSkillPoint", playerCharacterId);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result; // Trả về điểm kỹ năng của nhân vật
        }
        catch(Exception ex)
        {
            Debug.Log("gui yeu cau lay diem ky nang that bai"+ex.Message);
            throw;
        }
    }
    public async Task<bool> DeleteSkill(int playerCharacter, int skilltreeid)
    {
        try
        {
            var result = await _connection.InvokeAsync<bool>("DeleteSkill", playerCharacter, skilltreeid);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result; // Trả về kết quả xóa kỹ năng
        }
        catch(Exception ex)
        {
            Debug.Log("Gui yeu cau hoan tac that bai"+ex.Message);
            throw;
        }
    }
    public async Task<ReturnPlayer> UpdateName(string email, string newName)
    {
        try
        {
            var result = await _connection.InvokeAsync<ReturnPlayer>("UpdateName", email, newName);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result;
        }
        catch (Exception ex) {
            Debug.Log("gui yeu cau doi ten that bai"+ex.Message);
            throw;
        }
    }
    public async Task<ReturnPlayer> ResetPass(string email, string passsword, string newpassword, string renewpassword)
    {
        try
        {
            var result = await _connection.InvokeAsync<ReturnPlayer>("ResetPass", email, passsword,newpassword,renewpassword);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result;
        }
        catch (Exception ex)
        {
            Debug.Log("gui yeu cau doi mat khau that bai" + ex.Message);
            throw;
        }
    }
    public async Task<ReturnPlayerCharacter> SelectedCharacter(int playerId, int characterId)
    {
        try
        {
            var result = await _connection.InvokeAsync<ReturnPlayerCharacter>("SelectedCharacter", playerId, characterId);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result;
        }
        catch (Exception ex)
        {
            Debug.Log("gui yeu cau tra cuu nguoi choi chon nhan vat that bai" + ex.Message);
            throw;
        }
    }
    public async Task<List<PlayerAchievementDto>> GetAllAchievements(int playerCharacterId)
    {
        try
        {
            var result = await _connection.InvokeAsync<List<PlayerAchievementDto>>("GetAllAchievements", playerCharacterId);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result; // Trả về danh sách thành tích của nhân vật
        }
        catch (Exception ex)
        {
            Debug.Log("gui yeu cau lay danh sach thanh tich that bai" + ex.Message);
            throw;
        }
    }
    public async Task<PlayerAchievementDto> UpdateAchievement(int playerCharacterId, int achievementId)
    {
        try
        {
            var result = await _connection.InvokeAsync<PlayerAchievementDto>("UpdateAchievement", playerCharacterId, achievementId);
            string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return result; // Trả về kết quả cập nhật thành tích
        }
        catch (Exception ex)
        {
            Debug.Log("gui yeu cau cap nhat thanh tich that bai" + ex.Message);
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

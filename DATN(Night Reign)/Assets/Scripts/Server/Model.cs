using System.ComponentModel.DataAnnotations;
using System;

namespace server.model
{
    public class ReturnPlayerCharacterSkill
    {
        public bool status { get; set; }
        public string message { get; set; }
        public object Player_Character_Skill { get; set; }
    }
    public class ReturnPlayer
    {
        public bool status { get; set; }
        public string message { get; set; }
        public Player Player { get; set; }
    }
    public class ReturnPlayerCharacter
    {
        public bool status { get; set; }
        public string message { get; set; }
        public Player_Characters player_Characters { get; set; }
    }

    public class Player_Characters
    {
        public int Player_Character_id { get; set; }
        public int? Player_id { get; set; }
        public int? Characters_id { get; set; }
        public DateTime? Ownershipdate { get; set; } = DateTime.Now;
        public int Current_hp { get; set; }
        public int Current_exp { get; set; }
        public int level { get; set; }
        public int Total_point { get; set; }
        public int Total_coin { get; set; }
        public int Skill_Point { get; set; }
        public double Position_x { get; set; }
        public double Position_y { get; set; }
        public double Position_z { get; set; }
        public DateTime? Datesave { get; set; } = DateTime.Now;
    }
    public class PlayerCharacterSkillPoint
    {
        public int PlayerCharacterId { get; set; }
        public int SkillPoint { get; set; }
    }
    public class Player
    {
        public int Player_id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Password_Salt { get; set; }
        public string Name { get; set; }
        public decimal Total_Money { get; set; } = 0.00M;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
    public class PlayerAchievementDto
    {
        public int AchievementId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RequiredAmount { get; set; }
        public int RewardCoin { get; set; }
        public int RewardExp { get; set; }
        public int CurrentProgress { get; set; }
        public bool IsCompleted { get; set; }
        public string Status { get; set; } // "Completed", "InProgress", "NotStarted"
        public string IconPath { get; set; } // Đường dẫn đến biểu tượng của thành tựu
    }


    public class ConsumeItemRequestDTO
    {
        public int PlayerCharacterId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; } // Số lượng vật phẩm muốn tiêu thụ
    }

    public class Item
    {
        public int Item_id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string IconPath { get; set; }
    }

    public class PlayerInventoryItemDTO
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string ItemDescription { get; set; }
        public string IconPath { get; set; } // Nếu có cột IconPath trong bảng Item
    }

    public class ExchangeCurrencyRequestDTO
    {
        public int PlayerId { get; set; } // ID của Player (vì Total_Money thuộc về Player)
        public int PlayerCharacterId { get; set; } // ID của Player_Character (vì Total_coin thuộc về Player_Character)
        public string SourceCurrencyItemName { get; set; } // Tên vật phẩm tiền tệ nguồn (ví dụ: "Money", "Coin")
        public string TargetCurrencyItemName { get; set; } // Tên vật phẩm tiền tệ đích (ví dụ: "Money", "Coin")
        public decimal AmountToExchange { get; set; } // Số lượng tiền muốn đổi
    }

    public class BuyItemRequestDTO
    {
        public int PlayerCharacterId { get; set; } // ID của nhân vật thực hiện mua
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class BuyItemResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal NewPlayerTotalCoin { get; set; } // Total_Coin mới của Player_Character
    }

    public class ExchangeCurrencyResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal NewPlayerTotalMoney { get; set; } // Số Total_Money mới của Player
        public decimal NewPlayerTotalCoin { get; set; } // Số Total_Coin mới của Player_Character
    }

    public class Ranking
    {
        public string playerName { get; set; }
        public int total_point { get; set; }
        public int rank {  get; set; }
    }
    public class Battle
    {
        public string text { get; set; }
        public int point { get; set; }
    }
    public class BattleRq
    {
        public int Player_Characters_id { get; set; }
        public int Boss_id { get; set; }
        public int Death { get; set; }
        public double maxtime { get; set; }
        public double realtime { get; set; }
    }
}
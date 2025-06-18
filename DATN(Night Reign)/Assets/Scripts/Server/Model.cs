using System.ComponentModel.DataAnnotations;
using System;

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
public class Ranking
{
    public string PlayerName { get; set; }
    public int Total_point { get; set; }
    public int Rank { get; set; }
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

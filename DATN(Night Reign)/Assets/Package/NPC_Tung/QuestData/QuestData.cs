using UnityEngine;
// using UnityEngine.Localization; // Bỏ hoặc comment dòng này
// using npc.localization; // Bỏ hoặc comment dòng này nếu bạn không dùng LocalizedString tùy chỉnh của mình nữa

public enum QuestType
{
    KillEnemies,
    FindNPC,
    CollectItem
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string questName; // Đổi lại thành string
    public string giverNPCID;

    [TextArea(3, 5)]
    public string description; // Đổi lại thành string

    [Header("Quest Requirements")]
    public QuestType questType;
    public int requiredKills;
    public string targetNPCID;
    public string targetItemID;
    public int requiredItemCount = 1;

    [Header("Quest Rewards")]
    public int rewardSoul;
    public int rewardExp;

    [Header("Dialogue")]
    [TextArea(3, 5)]
    public string[] keydialogueBeforeComplete; // Đổi lại thành string[]

    [TextArea(3, 5)]
    public string[] keydialogueAfterComplete; // Đổi lại thành string[]
}
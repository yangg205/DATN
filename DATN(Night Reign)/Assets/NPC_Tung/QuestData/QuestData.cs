using UnityEngine;

public enum QuestType
{
    KillEnemies,
    FindNPC,
    CollectItem  // Thêm loại nhiệm vụ mới
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string questName;
    [TextArea(3, 5)] public string description;

    [Header("Quest Requirements")]
    public QuestType questType;

    public int requiredKills;          // Dùng nếu quest là KillEnemies
    public string targetNPCID;         // Dùng nếu quest là FindNPC

    public string targetItemID;        // Dùng nếu quest là CollectItem
    public int requiredItemCount = 1;  // Số lượng item cần thu thập

    [Header("Quest Rewards")]
    public int rewardSoul;
    public int rewardExp;

    [Header("Dialogue")]
    [TextArea(3, 5)] public string[] dialogueBeforeComplete;
    [TextArea(3, 5)] public string[] dialogueAfterComplete;
}

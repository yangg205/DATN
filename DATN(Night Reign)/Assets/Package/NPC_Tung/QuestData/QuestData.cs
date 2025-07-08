using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;

public enum QuestType
{
    KillEnemies,
    FindNPC,
    CollectItem
}

public enum QuestDialogueType
{
    BeforeComplete,
    AfterComplete,
    ObjectiveMet
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string questName; // Localization Key cho tên nhiệm vụ
    public string giverNPCID; // ID của NPC giao nhiệm vụ

    [TextArea(3, 5)]
    public string description; // Localization Key cho mô tả nhiệm vụ

    [Header("Quest Requirements")]
    public QuestType questType;
    public int requiredKills;
    public string targetNPCID;
    public string targetItemID;
    public int requiredItemCount = 1;

    [Header("Quest Rewards")]
    public int rewardCoin;
    public int rewardExp;

    // --- BỔ SUNG ĐỊA ĐIỂM NHIỆM VỤ ---
    [Header("Quest Location (Optional)")]
    [Tooltip("Check this if this quest has a specific location to mark on the map/world.")]
    public bool hasQuestLocation = false;
    [Tooltip("The world coordinates (Vector3) where the player needs to go for this quest.")]
    public Vector3 questLocation;
    [Tooltip("Optional: Custom icon for this quest's location marker. If null, a default will be used.")]
    public Sprite questLocationIcon;
    // --- KẾT THÚC BỔ SUNG ---

    [Header("Dialogues")]
    [Tooltip("Keys for dialogue lines before the quest is accepted.")]
    [TextArea(3, 5)] public string[] keydialogueBeforeComplete;
    [Tooltip("Voice clips for dialogue lines before the quest is accepted. Must match 'keydialogueBeforeComplete' in length.")]
    public AudioClip[] voiceBeforeComplete;

    [Tooltip("Keys for dialogue lines after the quest objective is met.")]
    [TextArea(3, 5)] public string[] keydialogueAfterComplete;
    [Tooltip("Voice clips for dialogue lines after the quest objective is met. Must match 'keydialogueAfterComplete' in length.")]
    public AudioClip[] voiceAfterComplete;

    [Tooltip("Keys for dialogue lines when the objective is met (can be used instead of AfterComplete).")]
    [TextArea(3, 5)] public string[] keydialogueObjectiveMet;
    [Tooltip("Voice clips for dialogue lines when the objective is met. Must match 'keydialogueObjectiveMet' in length.")]
    public AudioClip[] voiceObjectiveMet;

    public string[] GetDialogueKeys(string[] keys)
    {
        if (keys == null) return new string[0];
        return keys;
    }

    public AudioClip[] GetDialogueVoiceClips(string[] keys)
    {
        if (keys == keydialogueBeforeComplete) return voiceBeforeComplete;
        if (keys == keydialogueAfterComplete) return voiceAfterComplete;
        if (keys == keydialogueObjectiveMet) return voiceObjectiveMet;
        return new AudioClip[0];
    }

    public string GetQuestNameLocalized()
    {
        return GetLocalizedString("NhiemVu", questName);
    }

    public string GetDescriptionLocalized()
    {
        return GetLocalizedString("NhiemVu", description);
    }

    private string GetLocalizedString(string tableName, string key)
    {
        StringTable table = LocalizationSettings.StringDatabase.GetTable(tableName);
        if (table == null)
        {
            Debug.LogError($"Localization StringTable '{tableName}' not found! (Check Localization Settings và tên bảng!)");
            return $"[TABLE NOT FOUND: {tableName}]";
        }

        var entry = table.GetEntry(key);
        if (entry == null)
        {
            Debug.LogError($"❌ Key '{key}' không có trong bảng '{tableName}'");
            return $"[MISSING KEY: {key}]";
        }

        return entry.GetLocalizedString();
    }
}
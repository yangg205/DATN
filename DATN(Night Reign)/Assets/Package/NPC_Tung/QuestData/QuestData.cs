using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System;
using System.Collections.Generic; // Thêm để dùng List

public enum QuestType
{
    KillEnemies,
    FindNPC,
    CollectItem
}

public enum QuestDialogueType
{
    BeforeComplete,     // When offering the quest
    AfterComplete,      // After claiming reward and quest is done
    ObjectiveMet,       // When objective is met, but reward not claimed
    InProgress          // When quest is accepted but objective not yet met
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string questName; // Localization Key cho tên nhiệm vụ
    [Tooltip("ID của NPC giao nhiệm vụ (dùng để hiển thị tên).")]
    public string giverNPCID;

    [TextArea(3, 5)]
    public string description; // Localization Key cho mô tả nhiệm vụ

    [Header("Quest Requirements")]
    public QuestType questType;
    public int requiredKills;
    public string targetNPCID;    // Với FindNPC
    public string targetItemID;   // Với CollectItem
    public int requiredItemCount = 1;

    [Header("Quest Location (Waypoint)")]
    [Tooltip("Chỉ định nếu nhiệm vụ có một vị trí cụ thể (dùng waypoint).")]
    public bool hasQuestLocation;

    [Tooltip("Vị trí cụ thể trên bản đồ – dùng làm waypoint cho mục tiêu nhiệm vụ.")]
    public Vector3 questLocation;

    [Tooltip("Vị trí của NPC giao nhiệm vụ (dùng làm waypoint quay lại).")]
    public Vector3 giverNPCTransform;

    [Tooltip("Icon waypoint tùy chỉnh (nếu null thì dùng mặc định).")]
    public Sprite questLocationIcon;

    [Header("Quest Rewards")]
    public int rewardCoin;
    public int rewardExp;
    // --- THÊM CÁC TRƯỜNG VẬT PHẨM THƯỞNG MỚI ---
    [Tooltip("ID của vật phẩm thưởng. Có thể để trống nếu không có vật phẩm.")]
    public string rewardItemID;
    [Tooltip("Số lượng vật phẩm thưởng.")]
    public int rewardItemCount = 1;
    // ------------------------------------------

    [Header("Dialogues - Keys")]
    [Tooltip("Keys cho lời thoại trước khi nhận nhiệm vụ.")]
    [TextArea(3, 5)] public string[] keydialogueBeforeComplete;

    [Tooltip("Keys cho lời thoại sau khi hoàn thành.")]
    [TextArea(3, 5)] public string[] keydialogueAfterComplete;

    [Tooltip("Keys cho lời thoại khi mục tiêu đã đạt được.")]
    [TextArea(3, 5)] public string[] keydialogueObjectiveMet;

    [Header("Voice Clips (English)")]
    public AudioClip[] voiceBeforeComplete_EN;
    public AudioClip[] voiceAfterComplete_EN;
    public AudioClip[] voiceObjectiveMet_EN;

    [Header("Voice Clips (Vietnamese)")]
    public AudioClip[] voiceBeforeComplete_VI;
    public AudioClip[] voiceAfterComplete_VI;
    public AudioClip[] voiceObjectiveMet_VI;

    // --- Helper methods ---

    public string[] GetDialogueKeys(QuestDialogueType dialogueType)
    {
        return dialogueType switch
        {
            QuestDialogueType.BeforeComplete => keydialogueBeforeComplete,
            QuestDialogueType.AfterComplete => keydialogueAfterComplete,
            QuestDialogueType.ObjectiveMet => keydialogueObjectiveMet,
            _ => Array.Empty<string>(),
        };
    }

    public AudioClip[] GetDialogueVoiceClips(QuestDialogueType dialogueType)
    {
        var locale = LocalizationSettings.SelectedLocale?.Identifier.Code ?? "en";
        bool isVI = locale.StartsWith("vi", StringComparison.OrdinalIgnoreCase);

        return dialogueType switch
        {
            QuestDialogueType.BeforeComplete => isVI ? voiceBeforeComplete_VI : voiceBeforeComplete_EN,
            QuestDialogueType.AfterComplete => isVI ? voiceAfterComplete_VI : voiceAfterComplete_EN,
            QuestDialogueType.ObjectiveMet => isVI ? voiceObjectiveMet_VI : voiceObjectiveMet_EN,
            _ => Array.Empty<AudioClip>(),
        };
    }

    public string GetQuestNameLocalized()
    {
        return GetLocalizedString("NhiemVu", questName);
    }

    public string GetDescriptionLocalized()
    {
        return GetLocalizedString("NhiemVu", description);
    }

    public string GetTargetNPCNameLocalized()
    {
        if (string.IsNullOrEmpty(targetNPCID)) return "";
        return GetLocalizedString("NPC_Names", targetNPCID);
    }

    public string GetTargetItemNameLocalized()
    {
        if (string.IsNullOrEmpty(targetItemID)) return "";
        return GetLocalizedString("Item_Names", targetItemID);
    }

    public string GetGiverNPCNameLocalized()
    {
        if (string.IsNullOrEmpty(giverNPCID)) return "";
        return GetLocalizedString("NPC_Names", giverNPCID);
    }

    // --- Phương thức để lấy tên vật phẩm thưởng đã bản địa hóa ---
    public string GetRewardItemNameLocalized()
    {
        if (string.IsNullOrEmpty(rewardItemID)) return "";
        return GetLocalizedString("Item_Names", rewardItemID); // Giả định có bảng "Item_Names"
    }
    // -------------------------------------------------------------

    private string GetLocalizedString(string tableName, string key)
    {
        var table = LocalizationSettings.StringDatabase.GetTable(tableName);
        if (table == null)
        {
            Debug.LogError($"⚠️ Localization Table '{tableName}' not found.");
            return $"[TABLE NOT FOUND: {tableName}]";
        }

        key = key.Trim();
        var entry = table.GetEntry(key);
        if (entry == null)
        {
            Debug.LogError($"❌ Localization Key '{key}' not found in table '{tableName}'.");
            return $"[MISSING KEY: {key}]";
        }

        return entry.GetLocalizedString();
    }
}
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

    [Header("Dialogue")]
    [TextArea(3, 5)]
    public string[] keydialogueBeforeComplete; // Localization Keys cho thoại trước khi hoàn thành mục tiêu
    [TextArea(3, 5)]
    public string[] keydialogueAfterComplete;  // Localization Keys cho thoại sau khi nhiệm vụ đã hoàn thành
    [TextArea(3, 5)]
    public string[] keydialogueObjectiveMet; // Localization Keys cho thoại khi mục tiêu đã đạt được

    public string[] GetDialogueKeys(string[] keys)
    {
        if (keys == null) return new string[0];
        return keys;
    }

    public string GetQuestNameLocalized()
    {
        // Sử dụng tên bảng "NhiemVu_New"
        return GetLocalizedString("NhiemVu", questName);
    }

    public string GetDescriptionLocalized()
    {
        // Sử dụng tên bảng "NhiemVu_New"
        return GetLocalizedString("NhiemVu", description);
    }

    private string GetLocalizedString(string tableName, string key)
    {
        var table = LocalizationSettings.StringDatabase.GetTable(tableName);
        if (table == null)
        {
            Debug.LogError($"❌ Bảng '{tableName}' không tồn tại. (Check Localization Settings và tên bảng!)");
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
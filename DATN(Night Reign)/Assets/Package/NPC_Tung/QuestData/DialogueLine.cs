using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Thêm TMPro cho UIManager

// Đặt trong một file riêng để dễ quản lý hoặc có thể đặt trực tiếp trong QuestData.cs
[System.Serializable]
public struct DialogueLine
{
    [Tooltip("Key của lời thoại NPC trong Localization Quests Table")]
    public string npcDialogueKey;
    [Tooltip("Các Key của lời thoại phản hồi người chơi (nếu có). Nếu không có phản hồi, để trống.")]
    public string[] playerResponseKeys; // Có thể có nhiều lựa chọn phản hồi
    [Tooltip("Nhiệm vụ tiếp theo sẽ được kiểm tra sau khi lời thoại này kết thúc")]
    public QuestData nextQuestToCheck; // Để NPC có thể giao nhiệm vụ khác sau hội thoại
}
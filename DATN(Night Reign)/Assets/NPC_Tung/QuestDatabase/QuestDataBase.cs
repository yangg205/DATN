using UnityEngine;

[CreateAssetMenu(fileName = "Quest Database", menuName = "Quests/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    public QuestData[] quests;
}

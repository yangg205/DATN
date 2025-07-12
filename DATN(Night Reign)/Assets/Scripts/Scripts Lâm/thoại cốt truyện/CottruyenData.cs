using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data", fileName = "NewDialogueData")]
public class DialogueData : ScriptableObject
{
    [Tooltip("Danh sách key trong bảng Cutscene")]
    public List<string> keys = new List<string>();
}

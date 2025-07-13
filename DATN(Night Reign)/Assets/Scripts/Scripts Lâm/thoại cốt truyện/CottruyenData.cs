using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data", fileName = "NewDialogueData")]
public class DialogueData : ScriptableObject
{
    [Tooltip("Danh sách key trong bảng Cutscene")]
    public List<string> keys = new List<string>();

    [Tooltip("Voice tiếng Việt tương ứng với từng key")]
    public List<AudioClip> voiceVi = new List<AudioClip>();

    [Tooltip("Voice tiếng Anh tương ứng với từng key")]
    public List<AudioClip> voiceEn = new List<AudioClip>();
}

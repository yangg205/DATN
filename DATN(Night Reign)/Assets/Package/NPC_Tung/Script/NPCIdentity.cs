using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NPCIdentity : MonoBehaviour
{
    [Tooltip("ID định danh duy nhất cho NPC này (phải khớp với GiverNPCID trong QuestData)")]
    public string npcID;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Tự động gán tên GameObject làm npcID nếu đang để trống
        if (string.IsNullOrWhiteSpace(npcID))
        {
            npcID = gameObject.name;
            Debug.Log($"[NPCIdentity] Gán npcID mặc định cho {gameObject.name} → {npcID}");
            EditorUtility.SetDirty(this); // Đảm bảo lưu lại giá trị mới
        }
    }
#endif
}

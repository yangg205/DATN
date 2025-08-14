using UnityEngine;
using UnityEditor;

public class AutoLODGenerator : EditorWindow
{
    [Header("LOD Settings")]
    public float[] screenRelativeTransitionHeights = new float[] { 0.6f, 0.3f, 0.1f };
    public float cullHeight = 0.01f;

    [MenuItem("Tools/Auto LOD Generator")]
    static void ShowWindow()
    {
        GetWindow<AutoLODGenerator>("Auto LOD Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("One-time LOD Generation", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate LODs for Static Models"))
        {
            GenerateLODs();
        }
    }

    void GenerateLODs()
    {
        // Lấy tất cả GameObject trong Scene (Unity 6 API)
        GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            // Bỏ qua nếu không phải static hoặc không có MeshRenderer
            if (!obj.isStatic) continue;
            if (obj.GetComponent<MeshRenderer>() == null) continue;

            // Bỏ qua nếu đã có LODGroup
            if (obj.GetComponent<LODGroup>() != null) continue;

            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            Undo.RegisterCompleteObjectUndo(obj, "Add LODGroup");

            // Tạo LODGroup
            LODGroup lodGroup = obj.AddComponent<LODGroup>();

            // Tạo LOD levels
            Renderer renderer = obj.GetComponent<Renderer>();
            LOD[] lods = new LOD[screenRelativeTransitionHeights.Length + 1];
            for (int i = 0; i < screenRelativeTransitionHeights.Length; i++)
            {
                lods[i] = new LOD(screenRelativeTransitionHeights[i], new Renderer[] { renderer });
            }
            // LOD cuối cùng: culling
            lods[lods.Length - 1] = new LOD(cullHeight, new Renderer[] { });

            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();

            count++;
        }

        Debug.Log($"[AutoLODGenerator] Generated LODs for {count} static models.");
    }
}

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class TerrainCombinerAuto : EditorWindow
{
    // List để chứa các GameObject cha hoặc Terrain GameObject trực tiếp
    public List<GameObject> sourceTerrainContainers = new List<GameObject>();

    private Terrain targetTerrain; // Terrain lớn mới trong Scene mà dữ liệu sẽ được áp dụng lên

    // Đặt tên mặc định cho Terrain mới và asset data
    private string newTerrainGOName = "Combined_Terrain_Master";
    private string newTerrainDataAssetName = "Combined_Terrain_Data";

    // Biến để lưu trữ chỉ mục của item đang được chọn trong list (cho mục đích xóa)
    private int selectedSourceContainerIndex = -1;

    // Hệ số để tính toán độ phân giải dựa trên kích thước vật lý
    // Ví dụ: 1.0f pixelsPerMeter nghĩa là 1 pixel heightmap cho mỗi mét vật lý
    private float pixelsPerMeter = 1.0f;

    // Biến để hiển thị thông tin tính toán tạm thời
    private Vector2Int calculatedHeightmapResolution = new Vector2Int(0, 0);
    private Vector2Int calculatedAlphamapResolution = new Vector2Int(0, 0);
    private Vector3 calculatedPhysicalSize = new Vector3(0, 0, 0);

    public static TerrainCombinerAuto Instance;

    void OnEnable()
    {
        Instance = this;
        // Cập nhật thông tin tính toán ban đầu khi cửa sổ mở
        CalculateAndDisplayInitialResolutions();
    }

    void OnDestroy()
    {
        Instance = null;
    }

    [MenuItem("Tools/Terrain/Combiner Auto (Multi-Container)")] // Thêm dòng này để tạo menu item
    public static void ShowWindow()
    {
        GetWindow<TerrainCombinerAuto>("Terrain Combiner Auto (Multi-Container)");
    }

    private void OnGUI()
    {
        GUILayout.Label("Terrain Combiner Auto Settings (Multi-Container)", EditorStyles.boldLabel);

        EditorGUILayout.Space(10);

        // --- SOURCE TERRAIN CONTAINERS ---
        GUILayout.Label("Source Terrain Containers (Kéo GameObject cha hoặc Terrain vào đây)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Kéo các GameObject chứa các Terrain bạn muốn kết hợp (ví dụ: 'Map_VucLavareach', 'Map_Tuyet'). Script sẽ tự động tìm tất cả Terrain con bên trong và các TerrainLayer của chúng.", MessageType.Info);

        // Hiển thị danh sách các GameObject nguồn
        for (int i = 0; i < sourceTerrainContainers.Count; i++)
        {
            GUILayout.BeginHorizontal();
            sourceTerrainContainers[i] = (GameObject)EditorGUILayout.ObjectField($"Element {i}", sourceTerrainContainers[i], typeof(GameObject), true);

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                selectedSourceContainerIndex = i; // Đặt chỉ mục cho phần tử được chọn
            }
            GUILayout.EndHorizontal();
        }

        // Kéo thả GameObject mới vào list
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Kéo GameObject vào đây để thêm vào danh sách");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        GameObject go = draggedObject as GameObject;
                        if (go != null && !sourceTerrainContainers.Contains(go))
                        {
                            sourceTerrainContainers.Add(go);
                        }
                    }
                    sourceTerrainContainers.RemoveAll(item => item == null);
                    CalculateAndDisplayInitialResolutions(); // Cập nhật lại thông tin sau khi thêm
                    evt.Use();
                }
                break;
        }


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Source Container"))
        {
            sourceTerrainContainers.Add(null);
            selectedSourceContainerIndex = -1;
        }
        if (selectedSourceContainerIndex != -1 && selectedSourceContainerIndex < sourceTerrainContainers.Count)
        {
            if (GUILayout.Button($"Remove Selected ({selectedSourceContainerIndex})"))
            {
                sourceTerrainContainers.RemoveAt(selectedSourceContainerIndex);
                selectedSourceContainerIndex = -1;
                CalculateAndDisplayInitialResolutions(); // Cập nhật lại thông tin sau khi xóa
            }
        }
        else
        {
            GUI.enabled = false;
            GUILayout.Button("Remove Selected (None)");
            GUI.enabled = true;
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Clean Up Null Source Containers"))
        {
            sourceTerrainContainers.RemoveAll(item => item == null);
            selectedSourceContainerIndex = -1;
            CalculateAndDisplayInitialResolutions(); // Cập nhật lại thông tin sau khi dọn dẹp
        }

        EditorGUILayout.Space(10);

        // --- TARGET TERRAIN ---
        targetTerrain = (Terrain)EditorGUILayout.ObjectField("Target Terrain (Terrain lớn mới trong Scene)", targetTerrain, typeof(Terrain), true);
        EditorGUILayout.HelpBox("Nếu để trống, một Terrain mới sẽ được tạo tự động.", MessageType.Info);

        EditorGUILayout.Space(10);

        // --- CALCULATION SETTINGS ---
        pixelsPerMeter = EditorGUILayout.Slider("Pixels Per Meter (Chi tiết)", pixelsPerMeter, 0.1f, 4.0f);
        EditorGUILayout.HelpBox("Giá trị này càng cao, Terrain càng chi tiết nhưng tốn tài nguyên hơn. Thay đổi để xem ảnh hưởng đến độ phân giải bên dưới.", MessageType.Info);

        // --- DISPLAY CALCULATED RESOLUTIONS ---
        EditorGUILayout.Space(10);
        GUILayout.Label("Calculated New Terrain Properties", EditorStyles.boldLabel);
        if (sourceTerrainContainers.Count > 0 && calculatedPhysicalSize.x > 0)
        {
            EditorGUILayout.HelpBox("Các giá trị này sẽ được tính toán tự động dựa trên các Terrain nguồn và 'Pixels Per Meter'.", MessageType.Info);
            EditorGUILayout.Vector3Field("Ước tính kích thước vật lý", calculatedPhysicalSize);
            EditorGUILayout.Vector2IntField("Ước tính Heightmap Resolution", calculatedHeightmapResolution);
            EditorGUILayout.Vector2IntField("Ước tính Alphamap Resolution", calculatedAlphamapResolution);
        }
        else
        {
            EditorGUILayout.HelpBox("Vui lòng thêm các Terrain nguồn để xem các giá trị tính toán được.", MessageType.Warning);
        }

        if (GUI.changed) // Cập nhật khi có thay đổi trong GUI (ví dụ: kéo slider)
        {
            CalculateAndDisplayInitialResolutions();
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Tên cho Terrain mới (Nếu được tạo)", EditorStyles.boldLabel);
        newTerrainGOName = EditorGUILayout.TextField("GameObject Name", newTerrainGOName);
        newTerrainDataAssetName = EditorGUILayout.TextField("Data Asset Name", newTerrainDataAssetName);

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Combine And Apply Data To Target Terrain"))
        {
            ApplyCombinedData();
        }

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Find All Terrains in Containers (Debug)"))
        {
            FindAllTerrainsInContainers();
        }
    }

    private void CalculateAndDisplayInitialResolutions()
    {
        List<Terrain> allSourceTerrains = GetAllSourceTerrains();
        if (allSourceTerrains.Count == 0)
        {
            calculatedPhysicalSize = Vector3.zero;
            calculatedHeightmapResolution = Vector2Int.zero;
            calculatedAlphamapResolution = Vector2Int.zero;
            return;
        }

        Vector3 minWorldPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 maxWorldPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        float totalAvgHeight = 0;

        foreach (Terrain terrain in allSourceTerrains)
        {
            Vector3 terrainWorldPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            minWorldPos.x = Mathf.Min(minWorldPos.x, terrainWorldPos.x);
            minWorldPos.y = Mathf.Min(minWorldPos.y, terrainWorldPos.y);
            minWorldPos.z = Mathf.Min(minWorldPos.z, terrainWorldPos.z);

            maxWorldPos.x = Mathf.Max(maxWorldPos.x, terrainWorldPos.x + terrainSize.x);
            maxWorldPos.y = Mathf.Max(maxWorldPos.y, terrainWorldPos.y + terrainSize.y);
            maxWorldPos.z = Mathf.Max(maxWorldPos.z, terrainWorldPos.z + terrainSize.z);

            totalAvgHeight += terrainSize.y;
        }
        totalAvgHeight /= allSourceTerrains.Count;
        if (totalAvgHeight == 0) totalAvgHeight = 600; // Mặc định 600m nếu không có dữ liệu

        Vector3 currentTotalPhysicalSize = maxWorldPos - minWorldPos;
        currentTotalPhysicalSize.y = totalAvgHeight; // Gán chiều cao trung bình vào đây

        calculatedPhysicalSize = currentTotalPhysicalSize;

        // Tính toán độ phân giải Heightmap
        int targetHeightmapResX = Mathf.NextPowerOfTwo(Mathf.RoundToInt(calculatedPhysicalSize.x * pixelsPerMeter)) + 1;
        int targetHeightmapResZ = Mathf.NextPowerOfTwo(Mathf.RoundToInt(calculatedPhysicalSize.z * pixelsPerMeter)) + 1;

        calculatedHeightmapResolution = new Vector2Int(targetHeightmapResX, targetHeightmapResZ);

        // Tính toán độ phân giải Alphamap (thường là Heightmap Res - 1)
        int targetAlphamapResX = Mathf.NextPowerOfTwo(Mathf.RoundToInt(calculatedPhysicalSize.x * pixelsPerMeter));
        int targetAlphamapResZ = Mathf.NextPowerOfTwo(Mathf.RoundToInt(calculatedPhysicalSize.z * pixelsPerMeter));

        calculatedAlphamapResolution = new Vector2Int(targetAlphamapResX, targetAlphamapResZ);

        // Ensure Alphamap resolution is always PowerOfTwo and matches Heightmap Res - 1 if possible
        if (calculatedAlphamapResolution.x != calculatedHeightmapResolution.x - 1)
        {
            calculatedAlphamapResolution.x = Mathf.NextPowerOfTwo(calculatedHeightmapResolution.x - 1);
        }
        if (calculatedAlphamapResolution.y != calculatedHeightmapResolution.y - 1)
        {
            calculatedAlphamapResolution.y = Mathf.NextPowerOfTwo(calculatedHeightmapResolution.y - 1);
        }
    }

    private List<Terrain> GetAllSourceTerrains()
    {
        List<Terrain> foundTerrains = new List<Terrain>();
        if (sourceTerrainContainers == null || sourceTerrainContainers.Count == 0)
        {
            return foundTerrains;
        }

        foreach (GameObject go in sourceTerrainContainers)
        {
            if (go == null) continue;
            foundTerrains.AddRange(go.GetComponentsInChildren<Terrain>(true));
        }
        return foundTerrains;
    }

    private void FindAllTerrainsInContainers()
    {
        List<Terrain> foundTerrains = GetAllSourceTerrains();
        if (foundTerrains.Count == 0)
        {
            Debug.LogWarning("Không tìm thấy Terrain nào trong các GameObject đã gán.");
            return;
        }

        Debug.Log($"Đã tìm thấy {foundTerrains.Count} Terrain trong các container:");
        foreach (Terrain t in foundTerrains)
        {
            Debug.Log($"- Terrain: {t.name}, Position: {t.transform.position}, Size: {t.terrainData.size}, Heightmap Res: {t.terrainData.heightmapResolution}");
        }
    }

    private void ApplyCombinedData()
    {
        List<Terrain> allSourceTerrains = GetAllSourceTerrains();

        if (allSourceTerrains.Count == 0)
        {
            EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Terrain nào trong các 'Source Terrain Containers' đã gán. Vui lòng kiểm tra lại cấu trúc GameObject của bạn.", "OK");
            return;
        }

        // --- TỰ ĐỘNG THU THẬP VÀ GÁN TERRAIN LAYERS TỪ CÁC TERRAIN NGUỒN ---
        HashSet<TerrainLayer> collectedTerrainLayers = new HashSet<TerrainLayer>();
        foreach (Terrain sourceTerrain in allSourceTerrains)
        {
            if (sourceTerrain != null && sourceTerrain.terrainData != null && sourceTerrain.terrainData.terrainLayers != null)
            {
                foreach (TerrainLayer layer in sourceTerrain.terrainData.terrainLayers)
                {
                    if (layer != null)
                    {
                        collectedTerrainLayers.Add(layer);
                    }
                }
            }
        }

        TerrainLayer[] finalTerrainLayers = collectedTerrainLayers.ToArray();
        if (finalTerrainLayers.Length == 0)
        {
            EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy TerrainLayer nào từ các Terrain nguồn đã gán. Vui lòng đảm bảo các Terrain nguồn có TerrainLayer hợp lệ.", "OK");
            return;
        }

        // --- 1. Tính toán Bounding Box và Kích thước của Terrain mới ---
        Vector3 minWorldPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 maxWorldPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        float avgTerrainHeight = 0;

        foreach (Terrain terrain in allSourceTerrains)
        {
            Vector3 terrainWorldPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            minWorldPos.x = Mathf.Min(minWorldPos.x, terrainWorldPos.x);
            minWorldPos.y = Mathf.Min(minWorldPos.y, terrainWorldPos.y);
            minWorldPos.z = Mathf.Min(minWorldPos.z, terrainWorldPos.z);

            maxWorldPos.x = Mathf.Max(maxWorldPos.x, terrainWorldPos.x + terrainSize.x);
            maxWorldPos.y = Mathf.Max(maxWorldPos.y, terrainWorldPos.y + terrainSize.y);
            maxWorldPos.z = Mathf.Max(maxWorldPos.z, terrainWorldPos.z + terrainSize.z);

            avgTerrainHeight += terrainSize.y;
        }
        avgTerrainHeight /= allSourceTerrains.Count;

        Vector3 totalPhysicalSize = maxWorldPos - minWorldPos;
        if (avgTerrainHeight == 0) avgTerrainHeight = 600; // Mặc định 600m nếu không tính được

        // --- TÍNH TOÁN ĐỘ PHÂN GIẢI MỚI DỰA TRÊN KÍCH THƯỚC VẬT LÝ VÀ pixelsPerMeter ---
        int finalHeightmapResX = Mathf.NextPowerOfTwo(Mathf.RoundToInt(totalPhysicalSize.x * pixelsPerMeter)) + 1;
        int finalHeightmapResZ = Mathf.NextPowerOfTwo(Mathf.RoundToInt(totalPhysicalSize.z * pixelsPerMeter)) + 1;

        int finalAlphamapResX = Mathf.NextPowerOfTwo(Mathf.RoundToInt(totalPhysicalSize.x * pixelsPerMeter));
        int finalAlphamapResZ = Mathf.NextPowerOfTwo(Mathf.RoundToInt(totalPhysicalSize.z * pixelsPerMeter));

        // Đảm bảo Alphamap Resolution luôn là PowerOfTwo và khớp với Heightmap Res - 1 nếu có thể
        if (finalAlphamapResX != finalHeightmapResX - 1)
        {
            finalAlphamapResX = Mathf.NextPowerOfTwo(finalHeightmapResX - 1);
        }
        if (finalAlphamapResZ != finalHeightmapResZ - 1)
        {
            finalAlphamapResZ = Mathf.NextPowerOfTwo(finalHeightmapResZ - 1);
        }

        TerrainData newTerrainData;

        if (targetTerrain == null)
        {
            // --- Tự động tạo Terrain GameObject mới nếu chưa có Target Terrain được gán ---
            newTerrainData = new TerrainData();
            newTerrainData.heightmapResolution = finalHeightmapResX; // Sử dụng độ phân giải tính toán
            newTerrainData.alphamapResolution = finalAlphamapResX;   // Sử dụng độ phân giải tính toán
            newTerrainData.size = new Vector3(totalPhysicalSize.x, avgTerrainHeight, totalPhysicalSize.z);

            GameObject terrainGameObject = Terrain.CreateTerrainGameObject(newTerrainData);
            terrainGameObject.name = newTerrainGOName;
            terrainGameObject.transform.position = minWorldPos;
            targetTerrain = terrainGameObject.GetComponent<Terrain>();

            string dataAssetPath = $"Assets/CombinedTerrains/{newTerrainDataAssetName}_Data.asset";
            if (!AssetDatabase.IsValidFolder("Assets/CombinedTerrains"))
            {
                AssetDatabase.CreateFolder("Assets", "CombinedTerrains");
            }
            AssetDatabase.CreateAsset(newTerrainData, dataAssetPath);
            Debug.Log($"Đã tạo Terrain GameObject '{newTerrainGOName}' và TerrainData '{newTerrainDataAssetName}_Data.asset'.");
        }
        else
        {
            newTerrainData = targetTerrain.terrainData;
            Undo.RecordObject(newTerrainData, "Adjust Target Terrain Data Settings");
            newTerrainData.heightmapResolution = finalHeightmapResX; // Sử dụng độ phân giải tính toán
            newTerrainData.alphamapResolution = finalAlphamapResX;   // Sử dụng độ phân giải tính toán
            newTerrainData.size = new Vector3(totalPhysicalSize.x, avgTerrainHeight, totalPhysicalSize.z);
            EditorUtility.SetDirty(newTerrainData);
            Debug.Log($"Đã điều chỉnh độ phân giải và kích thước của Target Terrain thành Heightmap: {finalHeightmapResX}, Alphamap: {finalAlphamapResX}, Size: {totalPhysicalSize}.");

            Undo.RecordObject(targetTerrain.transform, "Move Target Terrain to Combined Position");
            targetTerrain.transform.position = minWorldPos;
            EditorUtility.SetDirty(targetTerrain.transform);
        }

        Undo.RecordObject(newTerrainData, "Set Terrain Layers");
        newTerrainData.terrainLayers = finalTerrainLayers;
        EditorUtility.SetDirty(newTerrainData);
        Debug.Log("Đã thiết lập Terrain Layers cho Terrain đích từ các Terrain nguồn.");

        GenerateAndApplyCombinedHeightmap(newTerrainData, allSourceTerrains, minWorldPos);

        GenerateAndApplyCombinedSplatmaps(newTerrainData, allSourceTerrains, minWorldPos);

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Hoàn tất kết hợp", "Dữ liệu đã được kết hợp và áp dụng thành công lên Target Terrain!", "OK");

        if (EditorUtility.DisplayDialog("Dọn dẹp Terrain cũ", "Bạn có muốn vô hiệu hóa (tắt) các Terrain nguồn sau khi kết hợp không?", "Có", "Không"))
        {
            foreach (Terrain terrain in allSourceTerrains)
            {
                if (terrain != null)
                {
                    terrain.gameObject.SetActive(false);
                }
            }
        }
    }

    private void GenerateAndApplyCombinedHeightmap(TerrainData newTerrainData, List<Terrain> allSourceTerrains, Vector3 minWorldPos)
    {
        int newRes = newTerrainData.heightmapResolution;
        float[,] newHeights = new float[newRes, newRes];

        Vector3 newTerrainSize = newTerrainData.size;

        Debug.Log("Bắt đầu kết hợp Heightmaps...");

        foreach (Terrain sourceTerrain in allSourceTerrains)
        {
            if (sourceTerrain == null || sourceTerrain.terrainData == null) continue;
            TerrainData sourceData = sourceTerrain.terrainData;
            int sourceRes = sourceData.heightmapResolution;
            float[,] sourceHeights = sourceData.GetHeights(0, 0, sourceRes, sourceRes);
            Vector3 sourceWorldPos = sourceTerrain.transform.position;

            float normalizedStartX = (sourceWorldPos.x - minWorldPos.x) / newTerrainSize.x;
            float normalizedStartZ = (sourceWorldPos.z - minWorldPos.z) / newTerrainSize.z;

            int destStartX = Mathf.RoundToInt(normalizedStartX * (newRes - 1));
            int destStartZ = Mathf.RoundToInt(normalizedStartZ * (newRes - 1));

            Debug.Log($"Kết hợp Heightmap từ '{sourceTerrain.name}' vào vị trí ({destStartX}, {destStartZ}) trên Terrain đích.");

            // Đảm bảo không đọc ngoài phạm vi của sourceHeights
            for (int z = 0; z < sourceRes; z++)
            {
                for (int x = 0; x < sourceRes; x++)
                {
                    int targetX = Mathf.Clamp(destStartX + x, 0, newRes - 1);
                    int targetZ = Mathf.Clamp(destStartZ + z, 0, newRes - 1);

                    newHeights[targetZ, targetX] = sourceHeights[z, x];
                }
            }
        }
        Undo.RecordObject(newTerrainData, "Đặt Heightmaps đã kết hợp");
        newTerrainData.SetHeights(0, 0, newHeights);
        EditorUtility.SetDirty(newTerrainData);
        Debug.Log("Heightmaps đã được kết hợp hoàn tất.");
    }

    private void GenerateAndApplyCombinedSplatmaps(TerrainData newTerrainData, List<Terrain> allSourceTerrains, Vector3 minWorldPos)
    {
        Debug.Log("Bắt đầu kết hợp Splatmaps (Textures)...");

        int newAlphaRes = newTerrainData.alphamapResolution;
        int numLayers = newTerrainData.alphamapLayers;
        float[,,] newAlphamaps = new float[newAlphaRes, newAlphaRes, numLayers];

        // Khởi tạo newAlphamaps với layer đầu tiên là 1.0f và các layer khác là 0.0f
        for (int y = 0; y < newAlphaRes; y++)
        {
            for (int x = 0; x < newAlphaRes; x++)
            {
                if (numLayers > 0)
                {
                    newAlphamaps[y, x, 0] = 1.0f;
                    for (int l = 1; l < numLayers; l++)
                    {
                        newAlphamaps[y, x, l] = 0.0f;
                    }
                }
            }
        }

        Vector3 newTerrainSize = newTerrainData.size;
        TerrainLayer[] targetTerrainLayers = newTerrainData.terrainLayers;

        foreach (Terrain sourceTerrain in allSourceTerrains)
        {
            if (sourceTerrain == null || sourceTerrain.terrainData == null) continue;
            TerrainData sourceData = sourceTerrain.terrainData;
            int sourceAlphaRes = sourceData.alphamapResolution;
            int sourceNumLayers = sourceData.alphamapLayers;
            float[,,] sourceAlphamaps = sourceData.GetAlphamaps(0, 0, sourceAlphaRes, sourceAlphaRes);
            TerrainLayer[] sourceTerrainLayers = sourceData.terrainLayers;

            Vector3 sourceWorldPos = sourceTerrain.transform.position;

            float normalizedStartX = (sourceWorldPos.x - minWorldPos.x) / newTerrainSize.x;
            float normalizedStartZ = (sourceWorldPos.z - minWorldPos.z) / newTerrainSize.z;

            int destStartX = Mathf.RoundToInt(normalizedStartX * newAlphaRes);
            int destStartZ = Mathf.RoundToInt(normalizedStartZ * newAlphaRes);

            Debug.Log($"Kết hợp Splatmaps từ '{sourceTerrain.name}' vào vị trí ({destStartX}, {destStartZ}) trên Terrain đích.");

            // Đảm bảo không đọc ngoài phạm vi của sourceAlphamaps
            for (int z = 0; z < sourceAlphaRes; z++)
            {
                for (int x = 0; x < sourceAlphaRes; x++)
                {
                    int targetX = Mathf.Clamp(destStartX + x, 0, newAlphaRes - 1);
                    int targetZ = Mathf.Clamp(destStartZ + z, 0, newAlphaRes - 1);

                    if (targetX >= 0 && targetX < newAlphaRes && targetZ >= 0 && targetZ < newAlphaRes)
                    {
                        // Reset các giá trị của pixel đích để tránh trộn lẫn không mong muốn nếu có nhiều nguồn chồng lấn
                        for (int l = 0; l < numLayers; l++)
                        {
                            newAlphamaps[targetZ, targetX, l] = 0.0f;
                        }

                        // Lấy splatmap từ terrain nguồn và ánh xạ sang terrain đích
                        if (sourceTerrainLayers != null && sourceTerrainLayers.Length > 0)
                        {
                            for (int l = 0; l < sourceNumLayers; l++)
                            {
                                TerrainLayer sourceLayer = sourceTerrainLayers.ElementAtOrDefault(l);
                                if (sourceLayer == null) continue;

                                // Tìm chỉ mục của sourceLayer trong mảng targetTerrainLayers
                                int targetLayerIndex = -1;
                                for (int i = 0; i < targetTerrainLayers.Length; i++)
                                {
                                    if (targetTerrainLayers.ElementAtOrDefault(i) == sourceLayer)
                                    {
                                        targetLayerIndex = i;
                                        break;
                                    }
                                }

                                if (targetLayerIndex != -1 && l < sourceAlphamaps.GetLength(2))
                                {
                                    newAlphamaps[targetZ, targetX, targetLayerIndex] = sourceAlphamaps[z, x, l];
                                }
                                else
                                {
                                    Debug.LogWarning($"Layer '{sourceLayer?.name}' từ terrain nguồn '{sourceTerrain.name}' không tìm thấy trong TerrainLayer của Terrain đích hoặc chỉ số layer nguồn vượt quá giới hạn. Có thể có sự không nhất quán.");
                                }
                            }
                        }

                        // Chuẩn hóa tổng các giá trị splatmap để đảm bảo tổng bằng 1.0f
                        float sum = 0;
                        for (int l = 0; l < numLayers; l++)
                        {
                            sum += newAlphamaps[targetZ, targetX, l];
                        }
                        if (sum > 0)
                        {
                            for (int l = 0; l < numLayers; l++)
                            {
                                newAlphamaps[targetZ, targetX, l] = newAlphamaps[targetZ, targetX, l] / sum;
                            }
                        }
                        else if (numLayers > 0)
                        {
                            // Nếu không có layer nào được gán cho pixel này, đặt layer đầu tiên là 1.0f
                            newAlphamaps[targetZ, targetX, 0] = 1.0f;
                        }
                    }
                }
            }
        }
        Undo.RecordObject(newTerrainData, "Đặt Alphamaps đã kết hợp");
        newTerrainData.SetAlphamaps(0, 0, newAlphamaps);
        EditorUtility.SetDirty(newTerrainData);
        Debug.Log("Splatmaps đã được kết hợp hoàn tất.");
    }
}
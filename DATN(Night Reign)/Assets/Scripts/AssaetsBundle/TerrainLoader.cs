using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class TerrainReplacer : MonoBehaviour
{
    [SerializeField] private string bundleName = "mapsamac";
    [SerializeField] private string assetNameInBundle = "MapSaMac"; // Cần phải khớp chính xác tên asset trong AssetBundle

    void Start()
    {
        StartCoroutine(ReplaceTerrain());
    }

    IEnumerator ReplaceTerrain()
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            Debug.LogError("Lỗi: 'Bundle Name' chưa được gán trong Inspector cho TerrainReplacer.");
            yield break;
        }
        if (string.IsNullOrEmpty(assetNameInBundle))
        {
            Debug.LogError("Lỗi: 'Asset Name In Bundle' chưa được gán trong Inspector cho TerrainReplacer.");
            yield break;
        }

        Terrain oldTerrain = FindAnyObjectByType<Terrain>();
        if (oldTerrain != null)
        {
            Debug.Log("Đã tìm thấy và xóa Terrain cũ: " + oldTerrain.name);
            Destroy(oldTerrain.gameObject);
        }
        else
        {
            Debug.Log("Không tìm thấy Terrain cũ trong scene để xóa.");
        }

        string fullBundlePath = Path.Combine(Application.streamingAssetsPath, bundleName);

        if (!File.Exists(fullBundlePath))
        {
            Debug.LogError("Không tìm thấy file bundle tại đường dẫn: " + fullBundlePath + ". Vui lòng kiểm tra lại tên bundle và vị trí.");
            yield break;
        }

        Debug.Log("Bắt đầu tải AssetBundle từ: " + fullBundlePath);
        AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(fullBundlePath);
        yield return bundleLoadRequest;

        AssetBundle bundle = bundleLoadRequest.assetBundle;
        if (bundle == null)
        {
            Debug.LogError("Không load được AssetBundle từ: " + fullBundlePath + ". Kiểm tra xem bundle có bị lỗi không.");
            yield break;
        }
        Debug.Log("Đã load AssetBundle thành công.");

        Debug.Log("Đang tải prefab '" + assetNameInBundle + "' từ AssetBundle...");
        AssetBundleRequest prefabLoadRequest = bundle.LoadAssetAsync<GameObject>(assetNameInBundle);
        yield return prefabLoadRequest;

        GameObject terrainPrefab = prefabLoadRequest.asset as GameObject;
        if (terrainPrefab == null)
        {
            Debug.LogError("Không tìm thấy prefab: '" + assetNameInBundle + "' trong AssetBundle. " +
                           "Kiểm tra lại tên asset trong AssetBundle Browser hoặc đảm bảo prefab đã được bao gồm.");
            bundle.Unload(true);
            yield break;
        }
        Debug.Log("Đã tải prefab '" + assetNameInBundle + "' thành công.");

        GameObject newTerrainInstance = Instantiate(terrainPrefab);
        newTerrainInstance.name = "Loaded_MapSaMac";

        Debug.Log("Đã load và thay Terrain từ AssetBundle hoàn tất. Terrain mới: " + newTerrainInstance.name);

        bundle.Unload(false);
        Debug.Log("AssetBundle đã được unload metadata, chỉ giữ lại các asset đang sử dụng.");
    }

    [ContextMenu("Log Asset Names in Bundle")]
    void LogAssetNamesInBundle()
    {
        string fullBundlePath = Path.Combine(Application.streamingAssetsPath, bundleName);
        if (!File.Exists(fullBundlePath))
        {
            Debug.LogError("Bundle file not found for inspection: " + fullBundlePath);
            return;
        }

        AssetBundle bundle = AssetBundle.LoadFromFile(fullBundlePath);
        if (bundle == null)
        {
            Debug.LogError("Failed to load bundle for inspection: " + fullBundlePath);
            return;
        }

        string[] assetNames = bundle.GetAllAssetNames();
        Debug.Log($"Assets in bundle '{bundleName}':");
        foreach (string name in assetNames)
        {
            Debug.Log("- " + name);
        }
        bundle.Unload(true);
    }
}
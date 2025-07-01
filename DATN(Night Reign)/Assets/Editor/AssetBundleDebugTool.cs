using UnityEditor;
using UnityEngine;
using System.IO;

public class AssetBundleBuilder
{
    private static string assetBundleOutputPath = "Assets/StreamingAssets";

    [MenuItem("Assets/Create/AssetBundles/Build All AssetBundles")]
    public static void BuildAllAssetBundles()
    {
        if (!Directory.Exists(assetBundleOutputPath))
        {
            Directory.CreateDirectory(assetBundleOutputPath);
            Debug.Log($"Đã tạo thư mục đầu ra AssetBundle: {assetBundleOutputPath}");
        }

        BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression;

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(assetBundleOutputPath, options, BuildTarget.StandaloneWindows64);

        if (manifest == null)
        {
            Debug.LogError("❌ Quá trình build AssetBundle thất bại! Vui lòng kiểm tra Console để biết thêm chi tiết.");
        }
        else
        {
            Debug.Log("✅ Quá trình build AssetBundle hoàn tất thành công!");
            Debug.Log($"Các AssetBundle đã được tạo tại: {assetBundleOutputPath}");

            foreach (string bundleName in manifest.GetAllAssetBundles())
            {
                Debug.Log($"- Đã build: {bundleName}");
            }
        }
    }

    [MenuItem("AssetBundles/Clear All Built AssetBundles")]
    public static void ClearBuiltAssetBundles()
    {
        if (Directory.Exists(assetBundleOutputPath))
        {
            Directory.Delete(assetBundleOutputPath, true);
            Debug.Log($"Đã xóa tất cả AssetBundle tại: {assetBundleOutputPath}");
        }
        else
        {
            Debug.Log("Không tìm thấy thư mục AssetBundle để xóa.");
        }
    }
}

using UnityEditor;
using UnityEngine;

public class BuildTerrainBundle
{
    [MenuItem("Tools/Build Terrain AssetBundle")]
    static void BuildAllAssetBundles()
    {
        string outputPath = "Assets/AssetBundles";
        if (!System.IO.Directory.Exists(outputPath))
            System.IO.Directory.CreateDirectory(outputPath);

        BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64 // hoặc Android/iOS tùy platform
        );

        Debug.Log("✅ Build thành công AssetBundle Terrain!");
    }
}

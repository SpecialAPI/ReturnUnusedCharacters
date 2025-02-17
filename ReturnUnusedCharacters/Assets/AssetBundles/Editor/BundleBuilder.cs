using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BundleBuilder
{
    public static Dictionary<BuildTarget, string> Builds = new Dictionary<BuildTarget, string>
    {
        { BuildTarget.StandaloneWindows, "Windows" },
        { BuildTarget.StandaloneLinux, "Linux" },
        { BuildTarget.StandaloneOSX, "MacOS" }
    };

    [MenuItem("Assets/Build AssetBundles")]
	public static void BuildBundles()
    {
        foreach(var kvp in Builds)
        {
            Debug.Log(kvp.Value);

            BuildPipeline.BuildAssetBundles("Assets/AssetBundles/" + kvp.Value, BuildAssetBundleOptions.None, kvp.Key);
        }
    }
}

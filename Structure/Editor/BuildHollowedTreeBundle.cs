// Unity editor script run by build-structure-bundle.sh (or from the menu), not part of the
// mod's build. Bundles the edited Hollowed Tree prefabs and whatever meshes, textures and
// materials they use; nothing else from the project goes in.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildHollowedTreeBundle
{
    private const string BundleName = "hollowedtree";
    private static readonly string[] Prefabs = { "VC_HollowedTree1", "VC_HollowedTree2" };

    // Meshes and textures load on either platform. Materials are swapped for the game's own at load, so shaders do not matter.
    private static readonly BuildTarget[] Targets = { BuildTarget.StandaloneWindows64, BuildTarget.StandaloneLinux64 };

    [MenuItem("HollowedTreeFix/Build Structure Bundle")]
    public static void Build()
    {
        List<string> paths = new();

        foreach (string name in Prefabs)
        {
            string[] found = AssetDatabase.FindAssets(name + " t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Path.GetFileNameWithoutExtension(path) == name)
                .ToArray();

            if (found.Length == 1) paths.Add(found[0]);
            else Debug.LogError($"{name}: expected one prefab with this name, found {found.Length}. {string.Join(", ", found)}");
        }

        if (paths.Count != Prefabs.Length) return;

        string[] dependencies = AssetDatabase.GetDependencies(paths.ToArray(), true)
            .Where(path => !path.EndsWith(".cs") && !path.EndsWith(".dll"))
            .ToArray();
        Debug.Log($"Bundling {paths.Count} prefabs with {dependencies.Length} assets:\n{string.Join("\n", dependencies.OrderBy(path => path))}");

        BuildTarget target = Targets.FirstOrDefault(t => BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, t));

        if (target == default)
        {
            Debug.LogError("This editor has neither the Windows nor the Linux Build Support module (Data/PlaybackEngines).");
            return;
        }

        string directory = Path.Combine(Directory.GetCurrentDirectory(), "StructureBundles");
        Directory.CreateDirectory(directory);
        string produced = Path.Combine(directory, BundleName);

        AssetBundleBuild build = new() { assetBundleName = BundleName, assetNames = paths.ToArray() };
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(directory, new[] { build }, BuildAssetBundleOptions.None, target);

        Debug.Log(manifest != null && File.Exists(produced)
            ? $"Built {produced} ({new FileInfo(produced).Length / 1024} KB, {target})"
            : "BuildAssetBundles produced nothing; see the errors above.");
    }
}

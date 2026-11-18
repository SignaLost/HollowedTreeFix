// HollowedTreeFix: ships hand-edited copies of Valheim Cuisine's Hollowed Tree locations
// (built in Unity, see Structure/) and puts them in place of the originals.
//
// State: the plugin loads the structure bundle and reports what is in it, and reports each
// Hollowed Tree the game creates. Putting the edited trees in place of Valheim Cuisine's is
// the next piece, and depends on how Valheim Cuisine registers its locations.

using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace HollowedTreeFix
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(ValheimCuisineGuid, BepInDependency.DependencyFlags.HardDependency)]
    public class HollowedTreeFixPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "NullSignal.HollowedTreeFix";
        public const string PluginName = "HollowedTreeFix";
        public const string PluginVersion = "0.1.0";

        public const string ValheimCuisineGuid = "XutzBR.ValheimCuisine";
        public const string LocationPrefix = "VC_HollowedTree";

        private const string BundleResource = "HollowedTreeFix.Resources.hollowedtree";

        internal static ManualLogSource Log;

        /// <summary>The edited trees, or null when the DLL was built without a bundle or it failed to load.</summary>
        internal static AssetBundle Structures;

        private void Awake()
        {
            Log = Logger;
            Structures = LoadBundle();
            new Harmony(PluginGuid).PatchAll(Assembly.GetExecutingAssembly());
        }

        private static AssetBundle LoadBundle()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(BundleResource))
            {
                if (stream == null)
                {
                    Log.LogWarning("Built without a structure bundle; the Hollowed Tree is left as Valheim Cuisine made it.");
                    return null;
                }

                byte[] bytes = new byte[stream.Length];
                int read = 0;
                while (read < bytes.Length)
                {
                    int n = stream.Read(bytes, read, bytes.Length - read);
                    if (n <= 0) break;
                    read += n;
                }

                AssetBundle bundle = AssetBundle.LoadFromMemory(bytes);

                if (bundle == null)
                {
                    Log.LogError("The structure bundle did not load; usually it was built with a newer Unity than the game runs.");
                    return null;
                }

                foreach (GameObject prefab in bundle.LoadAllAssets<GameObject>())
                {
                    Log.LogInfo($"Structure bundle: {prefab.name}, {prefab.GetComponentsInChildren<Transform>(true).Length} objects.");
                }

                return bundle;
            }
        }
    }
}

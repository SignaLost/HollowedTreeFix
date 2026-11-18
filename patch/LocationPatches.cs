using HarmonyLib;

namespace HollowedTreeFix.Patch
{
    [HarmonyPatch(typeof(Location), nameof(Location.Awake))]
    internal static class LocationAwakePatch
    {
        // For now this only confirms the game reaches here for a Hollowed Tree.
        private static void Postfix(Location __instance)
        {
            if (!__instance.gameObject.name.StartsWith(HollowedTreeFixPlugin.LocationPrefix, System.StringComparison.Ordinal)) return;

            HollowedTreeFixPlugin.Log.LogInfo($"{__instance.gameObject.name} created at {__instance.transform.position:0}.");
        }
    }
}

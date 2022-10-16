using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;
using MysticalMayhem.Helpers;

namespace MysticalMayhem.HarmonyPatches
{
    internal static class BlueprintsCachePatcher
    {
        [HarmonyPatch(typeof(BlueprintsCache), "Init")]
        [HarmonyPriority(Priority.LowerThanNormal)]
        internal class BlueprintsCache_Init_MM
        {
            [HarmonyPostfix]
            private static void MM_Init()
            {
                ResourceHandler.AddBundle("MM_icons");
                Settings.Initialize();
                BlueprintLoader.LoadBlueprints();
            }
        }
    }
}
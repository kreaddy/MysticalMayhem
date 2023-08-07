using HarmonyLib;
using Kingmaker;
using MysticalMayhem.Helpers;

namespace MysticalMayhem.HarmonyPatches
{
    [HarmonyPatch(typeof(GameStarter))]
    internal class GameStarterPatcher
    {
        [HarmonyPatch(nameof(GameStarter.FixTMPAssets))]
        [HarmonyPostfix]
        private static void MM_FixTMPAssets()
        {
            if (!Settings.IsEnabled("mm.no.hb")) { ModInterop.ApplyWarlockModPatches(); }
        }
    }
}

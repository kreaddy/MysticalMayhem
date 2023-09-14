using HarmonyLib;
using Kingmaker;
using Kingmaker.UI.MVVM._VM.ActionBar;
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
            if (!Settings.IsEnabled("mm.no.hb") && !Settings.IsEnabled("mm.no.warlock")) { ModInterop.ApplyWarlockModPatches(); }
        }
    }
}

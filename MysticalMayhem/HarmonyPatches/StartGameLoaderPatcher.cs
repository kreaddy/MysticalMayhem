using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem;
using MysticalMayhem.Helpers;

namespace MysticalMayhem.HarmonyPatches
{
    [HarmonyPatch(typeof(StartGameLoader))]
    internal class StartGameLoaderPatcher
    {
        [HarmonyPatch(nameof(StartGameLoader.LoadPackTOC))]
        [HarmonyPostfix]
        private static void MM_LoadPackTOC()
        {
            if (!Settings.IsEnabled("mm.no.hb")) { ModInterop.ApplyWarlockModPatches(); }
        }
    }
}

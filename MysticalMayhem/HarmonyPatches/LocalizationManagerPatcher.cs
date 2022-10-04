using HarmonyLib;
using Kingmaker.Localization;
using MysticalMayhem.Helpers;

namespace MysticalMayhem.HarmonyPatches
{
    internal class LocalizationManagerPatcher
    {
        [HarmonyPatch(typeof(LocalizationManager), "OnLocaleChanged")]
        internal class LocalizationManager_OnLocaleChanged_MM
        {
            [HarmonyPostfix]
            private static void OnLocaleChanged()
            {
                BlueprintLoader.ApplyLocalization();
            }
        }
    }
}
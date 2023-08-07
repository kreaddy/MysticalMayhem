using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using MysticalMayhem.Helpers;
using MysticalMayhem.Mechanics;

namespace MysticalMayhem.HarmonyPatches
{
    internal static class BlueprintsCachePatcher
    {
        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPriority(Priority.LowerThanNormal)]
        internal class BlueprintsCache_Init_MM
        {
            [HarmonyPostfix]
            private static void MM_Init()
            {
                ResourceHandler.AddBundle("MM_icons");
                Settings.Initialize();
                BlueprintLoader.LoadBlueprints();

                SpellComponentPatcher.PatchAssemblyForMaterialFreedom();
                DescriptorExtender.PatchUIUtilityTexts();
                DescriptorExtender.PatchTooltipTemplateAbility();

                if (Settings.IsEnabled("mm.adnd.stoneskin") && !Settings.IsEnabled("mm.no.hb")) PostPatches.ApplyStoneskinChanges();
                if (Settings.IsEnabled("mm.en.prebuff")) PostPatches.PrebuffUnits();
            }
        }
    }
}
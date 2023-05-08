using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;
using System.Linq;

namespace MysticalMayhem.HarmonyPatches
{
    [HarmonyPatch(typeof(UnitAbilityResourceCollection))]
    internal class UnitAbilityResourceCollectionPatcher
    {
        [HarmonyPatch(
            nameof(UnitAbilityResourceCollection.Restore),
            typeof(BlueprintScriptableObject),
            typeof(int),
            typeof(bool))]
        [HarmonyPrefix]
        private static bool Restore_MM(BlueprintScriptableObject blueprint, int amount, bool restoreFull)
        {
            return !NonReplenishingResources.Contains(blueprint.AssetGuidThreadSafe) || !restoreFull;
        }

        private static readonly string[] NonReplenishingResources = new string[]
        {
            "2d6ab5cf58b34468ad4040926d1f3c80"
        };
    }
}

using HarmonyLib;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
using MysticalMayhem.Mechanics;

namespace MysticalMayhem.HarmonyPatches
{
    internal class FavoredTerrainPatcher
    {
        [HarmonyPatch(typeof(FavoredTerrain))]
        internal class FavoredTerrain_MM
        {
            [HarmonyPatch("ActivateModifier")]
            [HarmonyPostfix]
            private static void ActivateModifier(FavoredTerrain __instance)
            {
                // Earth Magic check:
                // - Owner has the feat.
                if (__instance.Owner.Descriptor.GetEXFeature(FeatureExtender.Feature.EarthMagic))
                {
                    __instance.Owner.Stats.BonusCasterLevel.AddModifierUnique(1, __instance.Runtime, ModifierDescriptor.None);
                }
            }

            [HarmonyPatch("DeactivateModifier")]
            [HarmonyPostfix]
            private static void DeactivateModifier(FavoredTerrain __instance)
            {
                __instance.Owner.Stats.BonusCasterLevel.RemoveModifiersFrom(__instance.Runtime);
            }
        }
    }
}
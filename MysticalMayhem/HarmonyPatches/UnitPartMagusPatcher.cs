using HarmonyLib;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using MysticalMayhem.Mechanics.Parts;

namespace MysticalMayhem.HarmonyPatches
{
    [HarmonyPatch(typeof(UnitPartMagus))]
    internal class UnitPartMagusPatcher
    {
        [HarmonyPatch(nameof(UnitPartMagus.IsSpellFromMagusSpellList), typeof(AbilityData))]
        [HarmonyPostfix]
        public static void IsSpellFromMagusSpellList_MM(ref bool __result, UnitPartMagus __instance, AbilityData spell)
        {
            if (spell?.Spellbook?.Blueprint == UnitPartWarlock.Spellbook)
            {
                __result = __instance.Owner.Ensure<UnitPartWarlock>().GetFeature(UnitPartWarlock.Feature.CopyMagusMechanics);
            }
        }

        [HarmonyPatch("get_Spellbook")]
        [HarmonyPrefix]
        public static void get_Spellbook_MM(UnitPartMagus __instance)
        {
            if (__instance.m_Spellbook is null && __instance.Owner.Progression.GetClassLevel(__instance.Class) == 0)
            {
                if (__instance.Owner.Progression.GetClassLevel(UnitPartWarlock.Class) > 0)
                {
                    __instance.m_Spellbook = __instance.Owner.GetSpellbook(UnitPartWarlock.Class);
                }
            }
        }
    }
}
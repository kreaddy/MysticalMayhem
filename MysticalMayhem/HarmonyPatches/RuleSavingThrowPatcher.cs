using HarmonyLib;
using Kingmaker.RuleSystem.Rules;
using MysticalMayhem.Mechanics.Parts;

namespace MysticalMayhem.HarmonyPatches
{
    /// <summary>
    /// Hooks on RuleSavingThrow.
    /// </summary>
    [HarmonyPatch(typeof(RuleSavingThrow))]
    internal class RuleSavingThrowPatcher
    {
        /// <summary>
        /// Hooks on the method determining if a roll is successful.
        /// Can be used to enable autosuccess or autofailure based on conditions.
        /// </summary>
        [HarmonyPatch(nameof(RuleSavingThrow.IsSuccessRoll), typeof(int), typeof(int))]
        [HarmonyPostfix]
        private static void IsSuccessRoll_MM(RuleSavingThrow __instance, ref bool __result)
        {
            __instance.Initiator.Get<UnitPartWarlock>()?.EnsureSelfConfuse(__instance, ref __result);
        }
    }
}
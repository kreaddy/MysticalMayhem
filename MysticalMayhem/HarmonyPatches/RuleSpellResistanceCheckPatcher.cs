using HarmonyLib;
using Kingmaker.RuleSystem.Rules;
using MysticalMayhem.Mechanics;

namespace MysticalMayhem.HarmonyPatches
{
    /// <summary>
    /// Hooks on RuleSpellResistanceCheck.
    /// </summary>
    internal class RuleSpellResistanceCheckPatcher
    {
        [HarmonyPatch(typeof(RuleSpellResistanceCheck))]
        internal class RuleSpellResistanceCheck_MM
        {
            /// <summary>
            /// Hooks on the method determining if a spell is resisted.
            /// Can be used to enable autosuccess or autofailure based on conditions.
            /// </summary>
            [HarmonyPatch("get_IsSpellResisted")]
            [HarmonyPostfix]
            private static void get_IsSpellResisted_MM(RuleSpellResistanceCheck __instance, ref bool __result)
            {
                if (__instance.Initiator?.Descriptor?.GetEXFeature(FeatureExtender.Feature.PactWizardAutoPassChecks) && __instance.Roll.Result == 20)
                {
                    __result = true;
                }
            }
        }
    }
}
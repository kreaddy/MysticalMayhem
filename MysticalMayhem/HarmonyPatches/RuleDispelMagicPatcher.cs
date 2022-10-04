using HarmonyLib;
using Kingmaker.RuleSystem.Rules;
using MysticalMayhem.Mechanics;

namespace MysticalMayhem.HarmonyPatches
{
    /// <summary>
    /// Hooks on RuleDispelMagic.
    /// </summary>
    internal class RuleDispelMagicPatcher
    {
        /// <summary>
        /// Hooks on the method determining if a roll is successful.
        /// Can be used to enable autosuccess or autofailure based on conditions.
        /// </summary>
        [HarmonyPatch(typeof(RuleDispelMagic))]
        internal class RuleDispelMagic_MM
        {
            [HarmonyPatch("IsSuccessRoll", typeof(int))]
            [HarmonyPostfix]
            private static void IsSuccessRoll_MM(RuleDispelMagic __instance, ref bool __result, int d20)
            {
                // Check for Pact Wizard's capstone.
                if (__instance.Initiator?.Descriptor?.GetEXFeature(FeatureExtender.Feature.PactWizardAutoPassChecks) && d20 == 20)
                {
                    __result = true;
                }
            }
        }
    }
}
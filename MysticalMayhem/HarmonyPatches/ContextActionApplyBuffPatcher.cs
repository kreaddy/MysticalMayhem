using HarmonyLib;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using MysticalMayhem.Mechanics.Parts;
using System;

namespace MysticalMayhem.HarmonyPatches
{
    [HarmonyPatch(typeof(ContextActionApplyBuff))]
    internal class ContextActionApplyBuffPatcher
    {
        [HarmonyPatch(nameof(ContextActionApplyBuff.CalculateDuration), typeof(MechanicsContext))]
        [HarmonyPostfix]
        private static void CalculateDuration_MM(ref TimeSpan? __result, ContextActionApplyBuff __instance, MechanicsContext context)
        {
            if (!__instance.Target.IsUnit) return;
            __instance.Target.Unit.Get<UnitPartWarlock>()?.InfernalPactDoubleDuration(ref __result, __instance, context);
        }

        [HarmonyPatch(nameof(ContextActionApplyBuff.RunAction))]
        [HarmonyPostfix]
        private static void RunAction(ContextActionApplyBuff __instance)
        {
            if (!__instance.Target.IsUnit) return;
            __instance.AbilityContext?.MaybeCaster?.Get<UnitPartWarlock>()?.RegisterHex(__instance);
        }
    }
}

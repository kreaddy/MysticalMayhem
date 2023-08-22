using HarmonyLib;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using MysticalMayhem.Mechanics.Parts;

namespace MysticalMayhem.HarmonyPatches
{
    [HarmonyPatch(typeof(UnitConfusionController))]
    internal class UnitConfusionControllerPatcher
    {
        [HarmonyPatch(nameof(UnitConfusionController.TickConfusion), typeof(UnitEntityData))]
        [HarmonyPrefix]
        private static bool TickConfusion_MM(UnitConfusionController __instance, UnitEntityData unit)
        {
            if (unit.Get<UnitPartWarlock>() is null)
                return true;
            if (unit.Get<UnitPartWarlock>().TickConfusion()) return false;
            return true;
        }

        [HarmonyPatch(nameof(UnitConfusionController.SelfHarm), typeof(UnitPartConfusion))]
        [HarmonyPostfix]
        private static void SelfHarm_MM(ref UnitCommand __result, UnitPartConfusion part)
        {
            __result = part.Owner.Get<UnitPartWarlock>()?.NonLethalSelfHarm() ?? __result;
        }

        [HarmonyPatch(nameof(UnitConfusionController.AttackNearest), typeof(UnitPartConfusion))]
        [HarmonyPrefix]
        private static void AttackNearest_MM(UnitPartConfusion part)
        {
            part.Owner.Get<UnitPartWarlock>()?.HandleConfusionAttackBonus();
        }

        [HarmonyPatch(nameof(UnitConfusionController.DoNothing), typeof(UnitPartConfusion))]
        [HarmonyPostfix]
        private static void DoNothing_MM(ref UnitCommand __result, UnitPartConfusion part)
        {
            __result = part.Owner.Ensure<UnitPartWarlock>()?.Babble() ?? __result;
        }
    }
}

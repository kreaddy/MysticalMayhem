using HarmonyLib;
using Kingmaker;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility;
using MysticalMayhem.Mechanics.Parts;

namespace MysticalMayhem.HarmonyPatches
{
    [HarmonyPatch(typeof(Buff))]
    internal class BuffPatcher
    {
        [HarmonyPatch(nameof(Buff.TickMechanics))]
        [HarmonyPostfix]
        private static void TickMechanics_MM(Buff __instance)
        {
            if (__instance.Blueprint == UnitPartWarlock.DaemonicIncarnationBuff && !Game.Instance.Player.IsInCombat)
            {
                __instance.ReduceDuration(new Rounds(1).Seconds);
            }
        }
    }
}

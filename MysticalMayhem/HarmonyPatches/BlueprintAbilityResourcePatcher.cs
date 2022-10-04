using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using System;
using System.Linq;

namespace MysticalMayhem.HarmonyPatches
{
    /// <summary>
    /// Hooks on BlueprintAbilityResource to alter resource handling.
    /// </summary>
    internal class BlueprintAbilityResourcePatcher
    {
        /// <summary>
        /// Hooks on method calculating max resource amount.
        /// </summary>
        [HarmonyPatch(typeof(BlueprintAbilityResource))]
        internal class BlueprintAbilityResource_MM
        {
            private static string[] _halfStepBps = new string[]
            {
                "844812ee93394087a9389037b11cd269" // PactWizardResource
            };

            [HarmonyPatch("GetMaxAmount", typeof(UnitDescriptor))]
            [HarmonyPostfix]
            private static void GetMaxAmount(UnitDescriptor unit, ref int __result, BlueprintAbilityResource __instance)
            {
                // Half-step: when a resource is increased by a stat, only takes half the modifier.
                if (_halfStepBps.Contains(__instance.AssetGuid.ToString())) __result = CalculateHalfStepResource(__instance, unit);
            }
        }

        /// <summary>
        /// Recalculates the maximum resource amount so it only takes half a stat modifier into consideration.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private static int CalculateHalfStepResource(BlueprintAbilityResource resource, UnitDescriptor unit)
        {
            if (!resource.m_MaxAmount.IncreasedByStat)
            {
                Main.Log($"{resource.LocalizedName} is set as half-step resource but not as being increased by a stat!");
                return 0;
            };
            var num = resource.m_MaxAmount.BaseValue;
            var modifiableValueAttributeStat = unit.Stats.GetStat(resource.m_MaxAmount.ResourceBonusStat) as ModifiableValueAttributeStat;
            if (modifiableValueAttributeStat != null)
            {
                num += modifiableValueAttributeStat.Bonus / 2;
            }
            var bonus = 0;
            EventBus.RaiseEvent(unit.Unit, delegate (IResourceAmountBonusHandler handler)
            {
                handler.CalculateMaxResourceAmount(resource, ref bonus);
            });
            return Math.Max(resource.m_Min, resource.ApplyMinMax(num) + bonus);
        }
    }
}
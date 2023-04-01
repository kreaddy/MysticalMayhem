using HarmonyLib;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using MysticalMayhem.Helpers;
using MysticalMayhem.Mechanics;

namespace MysticalMayhem.HarmonyPatches
{
    internal class ContextConditionHasBuffImmunityWithDescriptorPatcher
    {
        [HarmonyPatch(typeof(ContextConditionHasBuffImmunityWithDescriptor), "CheckCondition")]
        internal class ContextConditionHasBuffImmunityWithDescriptor_CheckCondition_MM
        {
            [HarmonyPostfix]
            private static void CheckCondition_MM(ref bool __result, ContextConditionHasBuffImmunityWithDescriptor __instance)
            {
                if (!__result) { return; }
                HandleDraconicMalice(ref __result, __instance);

            }
        }

        private static void HandleDraconicMalice(ref bool __result, ContextConditionHasBuffImmunityWithDescriptor contextCondition)
        {
            Main.DebugLog($"Context Condition descriptors contain Fear or Mind-Affecting at start: " +
                $"{contextCondition.Context.SpellDescriptor.HasAnyFlag(SpellDescriptor.MindAffecting | SpellDescriptor.Fear)}");
            if (contextCondition.SpellDescriptor.HasAnyFlag(SpellDescriptor.MindAffecting | SpellDescriptor.Fear) == false) return;
            Main.DebugLog("Fear or Mind-Affecting descriptor found.");
            if (contextCondition.Target.Unit.Descriptor.GetEXFeature(FeatureExtender.Feature.DraconicMaliceCurse))    
            {
                Main.DebugLog("Target affected by Draconic Malice.");
                var curseCaster = contextCondition.Target.Unit.Descriptor.Buffs.GetBuff(BPLookup.Buff("DraconicMaliceAuraBuff", true))?.MaybeContext.MaybeCaster;
                if (curseCaster != null && curseCaster == contextCondition.Context.MaybeCaster)
                {
                    Main.DebugLog("Draconic Malice's caster found.");
                    __result = false;
                    return;
                }
            }
        }
    }
}
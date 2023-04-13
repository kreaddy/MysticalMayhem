using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using MysticalMayhem.Helpers;
using MysticalMayhem.Mechanics;

namespace MysticalMayhem.HarmonyPatches
{
    [HarmonyPatch(typeof(UnitPartSpellResistance))]
    internal class UnitPartSpellResistancePatcher
    {
        [HarmonyPatch(nameof(UnitPartSpellResistance.IsImmune), typeof(MechanicsContext), typeof(bool))]
        [HarmonyPrefix]
        private static void IsImmune_PrefixMM(UnitPartSpellResistance __instance, [CanBeNull] MechanicsContext context)
        {
            DraconicMaliceOverride(__instance, context);
        }

        [HarmonyPatch(nameof(UnitPartSpellResistance.IsImmune), typeof(MechanicsContext), typeof(bool))]
        [HarmonyPostfix]
        private static void IsImmune_PostfixMM(ref bool __result, UnitPartSpellResistance __instance, [CanBeNull] MechanicsContext context)
        {
            __instance.Owner.Ensure<UnitPartWarlock>().EnsureSelfConfuse(ref __result, context);
        }

        private static void DraconicMaliceOverride(UnitPartSpellResistance __instance, MechanicsContext context)
        {
            if (__instance.Owner.GetEXFeature(FeatureExtender.Feature.DraconicMaliceCurse) && context.SpellDescriptor.HasAnyFlag(SpellDescriptor.MindAffecting | SpellDescriptor.Fear))
            {
                var curseCaster = __instance.Owner.Buffs.GetBuff(BPLookup.Buff("DraconicMaliceAuraBuff", true))?.MaybeContext.MaybeCaster;
                if (curseCaster != null && curseCaster == context.MaybeCaster)
                {
                    context.RemoveSpellDescriptor(SpellDescriptor.MindAffecting);
                    context.RemoveSpellDescriptor(SpellDescriptor.Fear);
                    context.RemoveSpellDescriptor(SpellDescriptor.Shaken);
                    context.RemoveSpellDescriptor(SpellDescriptor.Frightened);
                    context.RemoveSpellDescriptor(SpellDescriptor.Emotion);
                    context.RemoveSpellDescriptor(SpellDescriptor.NegativeEmotion);
                }
            }
        }
    }
}

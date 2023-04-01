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
        [HarmonyPatch("IsImmune", typeof(MechanicsContext), typeof(bool))]
        [HarmonyPrefix]
        private static void IsImmune_MM(UnitPartSpellResistance __instance, [CanBeNull] MechanicsContext context, bool spellDescriptorOnly = false)
        {
            DraconicMaliceOverride(__instance, context);
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

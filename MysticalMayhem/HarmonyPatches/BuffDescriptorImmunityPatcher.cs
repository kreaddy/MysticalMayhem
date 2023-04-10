using HarmonyLib;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using MysticalMayhem.Helpers;
using MysticalMayhem.Mechanics;

namespace MysticalMayhem.HarmonyPatches
{
    internal class BuffDescriptorImmunityPatcher
    {
        [HarmonyPatch(typeof(BuffDescriptorImmunity), "IsImmune", typeof(MechanicsContext))]
        internal class BuffDescriptorImmunity_IsImmune_MM
        {
            [HarmonyPrefix]
            private static void IsImmune_MM(MechanicsContext context, BuffDescriptorImmunity __instance)
            {
                //Main.DebugLog("Starting immunity check...");
                DraconicMaliceOverride(context, __instance);
            }
        }

        private static void DraconicMaliceOverride(MechanicsContext context, BuffDescriptorImmunity __instance)
        {
            //Main.DebugLog($"Mind-Affecting: {context.SpellDescriptor.HasAnyFlag(SpellDescriptor.MindAffecting)}");
            //Main.DebugLog($"Fear: {context.SpellDescriptor.HasAnyFlag(SpellDescriptor.Fear)}");
            if (context.SpellDescriptor.HasAnyFlag(SpellDescriptor.MindAffecting | SpellDescriptor.Fear) == false) return;
            //Main.DebugLog("Fear or Mind-Affecting descriptor found.");
            //Main.DebugLog(__instance.Owner.CharacterName);
            //Main.DebugLog(__instance.Owner.Descriptor.GetEXFeature(FeatureExtender.Feature.DraconicMaliceCurse).ToString());
            if (__instance.Owner.Descriptor.GetEXFeature(FeatureExtender.Feature.DraconicMaliceCurse))    
            {
                //Main.DebugLog("Target affected by Draconic Malice.");
                var curseCaster = __instance.Owner.Descriptor.Buffs.GetBuff(BPLookup.Buff("DraconicMaliceAuraBuff", true))?.MaybeContext.MaybeCaster;
                if (curseCaster != null && curseCaster == context.MaybeCaster)
                {
                    //Main.DebugLog("Draconic Malice's caster found.");
                    context.RemoveSpellDescriptor(SpellDescriptor.MindAffecting);
                    context.RemoveSpellDescriptor(SpellDescriptor.Fear);
                    context.RemoveSpellDescriptor(SpellDescriptor.Shaken);
                    context.RemoveSpellDescriptor(SpellDescriptor.Frightened);
                    context.RemoveSpellDescriptor(SpellDescriptor.Emotion);
                    context.RemoveSpellDescriptor(SpellDescriptor.NegativeEmotion);
                    Main.DebugLog($"Context descriptors contain Fear or Mind-Affecting after processing: " +
                        $"{context.SpellDescriptor.HasAnyFlag(SpellDescriptor.MindAffecting | SpellDescriptor.Fear)}");
                }
            }
        }
    }
}
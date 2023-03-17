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
                HandleDraconicMalice(context, __instance);
            }
        }

        private static void HandleDraconicMalice(MechanicsContext context, BuffDescriptorImmunity immunity)
        {
            if (context.SpellDescriptor.HasAnyFlag(SpellDescriptor.MindAffecting | SpellDescriptor.Fear) == false) return;
            Main.DebugLog("Fear or Mind-Affecting descriptor found.");
            if (immunity.Owner.Descriptor.GetEXFeature(FeatureExtender.Feature.DraconicMaliceCurse))    
            {
                Main.DebugLog("Target affected by Draconic Malice.");
                var curseCaster = immunity.Owner.Descriptor.Buffs.GetBuff(BPLookup.Buff("DraconicMaliceAuraBuff", true))?.MaybeContext.MaybeCaster;
                if (curseCaster != null && curseCaster == context.MaybeCaster)
                {
                    Main.DebugLog("Draconic Malice's caster found.");
                    context.RemoveSpellDescriptor(SpellDescriptor.MindAffecting);
                    context.RemoveSpellDescriptor(SpellDescriptor.Fear);
                    Main.DebugLog($"Context descriptors contain Fear or Mind-Affecting after processing: " +
                        $"{context.SpellDescriptor.HasAnyFlag(SpellDescriptor.MindAffecting | SpellDescriptor.Fear)}");
                }
            }
        }
    }
}
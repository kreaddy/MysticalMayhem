using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using MysticalMayhem.Helpers;
using MysticalMayhem.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MysticalMayhem.HarmonyPatches
{
    [HarmonyPatch(typeof(Spellbook))]
    internal class SpellbookPatcher
    {
        [HarmonyPatch(nameof(Spellbook.GetSpontaneousConversionSpells), typeof(AbilityData))]
        [HarmonyPostfix]
        private static void GetSpontaneousConversionSpells_MM(AbilityData spell, ref IEnumerable<BlueprintAbility> __result,
            Spellbook __instance)
        {
            if (spell == null) return;
            var spellLevel = __instance.GetSpellLevel(spell);
            if (spellLevel <= 0) return;

            // Pact Wizard's Spontaneous Conversion check:
            // - Spell is prepared in a normal slot, not a school slot.
            // - Spell can only be converted into a patron spell of same level or lower.
            // Support for Thassilonian Specialist/Pact Wizard multi archetype provided.
            if (PostPatches.PactWizardLegalSpellbooks.Contains(__instance.Blueprint.AssetGuid.ToString()) &&
                __instance.Owner.GetEXFeature(FeatureExtender.Feature.PactWizardSpellConversion))
            {
                if (spell.SpellSlot.Type != SpellSlotType.Common) return;
                var wizard = BPLookup.Class("WizardClass");
                var selection = BPLookup.Selection("PactWizardPatronSelection", true);
                var patron = (BlueprintProgression)__instance.Owner.Progression.Selections[selection].GetSelections(1).First();
                var list = new List<BlueprintAbility>();
                var shadowSpells = new Dictionary<SpellSchool, BlueprintAbility>();
                foreach (var entry in patron.LevelEntries)
                {
                    if (entry.Level > __instance.Owner.Progression.GetClassLevel(wizard))
                        continue;

                    var components = entry.Features.Select(f => f.GetComponent<AddKnownSpell>());
                    foreach (var comp in components)
                    {
                        if (comp.SpellLevel <= spellLevel)
                        {
                            // More shadow spell nonsense.
                            var cSpell = comp.m_Spell.Get();
                            if (cSpell.GetComponent<AbilityShadowSpell>() != null)
                            {
                                var scomp = cSpell.GetComponent<AbilityShadowSpell>();
                                if (shadowSpells.ContainsKey(scomp.School))
                                {
                                    if (shadowSpells[scomp.School].GetComponent<AbilityShadowSpell>().MaxSpellLevel <= scomp.MaxSpellLevel)
                                    {
                                        list.Remove(shadowSpells[scomp.School]);
                                        list.Add(cSpell);
                                        shadowSpells.Remove(scomp.School);
                                        shadowSpells.Add(scomp.School, cSpell);
                                    }
                                    continue;
                                }
                                shadowSpells.Add(scomp.School, cSpell);
                            } // End of shadow spell nonseeeeeeeeeeeeense TT_TT
                            if (cSpell.GetComponent<AbilityVariants>() != null)
                            {
                                foreach (var variant in cSpell.GetComponent<AbilityVariants>().m_Variants)
                                    list.Add(variant.Get());
                            }
                            else list.Add(comp.m_Spell.Get());
                        }
                    }
                }
                __result = list.ToArray();
            }
        }
    }
}

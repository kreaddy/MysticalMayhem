using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using MysticalMayhem.Helpers;
using MysticalMayhem.Mechanics;
using System.Collections.Generic;
using System.Linq;

namespace MysticalMayhem.HarmonyPatches
{
    internal class AbilityDataPatcher
    {
        [HarmonyPatch(typeof(AbilityData), "get_RequireFullRoundAction")]
        internal class AbilityData_getRequireFullRoundAction_MM
        {
            [HarmonyPostfix]
            private static bool get_RequireFullRoundAction(bool result, AbilityData __instance)
            {
                // Acadamae Graduate check:
                // - Caster has the feat, isn't fatigued or exhausted.
                // - Abiity is an arcane spell from a prepared spellbook.
                // - Spell is from the Conjuration school and has the Summoning descriptor.
                if (result == true && __instance.Caster.GetEXFeature(FeatureExtender.Feature.AcadamaeGraduate) && __instance.Blueprint.IsSpell
                    && !__instance.IsSpontaneous && __instance.Caster != null && __instance.SpellSource == SpellSource.Arcane
                    && __instance.Blueprint.School == SpellSchool.Conjuration && !__instance.Caster.HasFact(BPLookup.Buff("Fatigued")) &&
                    !__instance.Caster.HasFact(BPLookup.Buff("Exhausted")) && __instance.Blueprint.SpellDescriptor.HasFlag(SpellDescriptor.Summoning))
                {
                    return false;
                }
                return result;
            }
        }

        [HarmonyPatch(typeof(AbilityData), "GetParamsFromItem", typeof(ItemEntity))]
        internal class AbilityData_GetParamsFromItems_MM
        {
            [HarmonyPostfix]
            private static void GetParamsFromItem(ref AbilityParams __result, AbilityData __instance, ItemEntity itemEntity)
            {
                // Staff-Like Wand check:
                // - User has the arcane discovery.
                // - User's caster level is higher than the item's base CL.
                // - DC is always increased by the user's Intelligence modifier.
                if (__result == null || itemEntity == null || itemEntity.Blueprint == null || __instance.Caster == null ||
                    (itemEntity.Blueprint is BlueprintItemEquipmentUsable) == false)
                {
                    return;
                }
                if ((itemEntity.Blueprint as BlueprintItemEquipmentUsable).Type != UsableItemType.Wand)
                {
                    return;
                }
                if (__instance.Caster.GetEXFeature(FeatureExtender.Feature.StaffLikeWand))
                {
                    if (__instance.Caster.GetSpellbook(BPLookup.Class("WizardClass")).CasterLevel > __result.CasterLevel)
                    {
                        __result.CasterLevel = __instance.Caster.GetSpellbook(BPLookup.Class("WizardClass")).CasterLevel;
                    }
                    __result.DC = __instance.Caster.Stats.Intelligence.Bonus + itemEntity.GetSpellLevel() + 10;
                }
            }
        }

        [HarmonyPatch(typeof(AbilityData), "get_ActionType")]
        internal class AbilityData_ActionTypeField_MM
        {
            [HarmonyPostfix]
            private static void get_ActionType(ref UnitCommand.CommandType __result, AbilityData __instance)
            {
                // Spell Synthesis check:
                // - User has the buff indicating they've used Spell Synthesis.
                // - User has the buff indicating they've casted a spell from one of their Mystic Theurge spellbooks.
                // - The spell being cast is from the MT spellbook they haven't cast yet this round.
                if (__instance.Caster == null || __instance.Caster.SpellSynthesis() == false) return;
                if (__instance.Spellbook == __instance.Caster.Ensure<UnitPartMysticTheurge>().DivineSpellbook
                    && __instance.Caster.MTHasCastArcaneSpell())
                {
                    __result = UnitCommand.CommandType.Free;
                    return;
                };
                if (__instance.Spellbook == __instance.Caster.Ensure<UnitPartMysticTheurge>().ArcaneSpellbook
                    && __instance.Caster.MTHasCastDivineSpell())
                {
                    __result = UnitCommand.CommandType.Free;
                    return;
                };
            }
        }

        [HarmonyPatch(typeof(AbilityData), "get_CanBeCastByCaster")]
        internal class AbilityData_CanBeCastByCasterField_MM
        {
            [HarmonyPostfix]
            private static void get_CanBeCastByCaster(ref bool __result, AbilityData __instance)
            {
                // Spell Synthesis check:
                // - User has the buff indicating they've used Spell Synthesis.
                // - User has the buff indicating they've casted a spell from one of their Mystic Theurge spellbooks.
                // - The spell being cast is from the same MT spellbook.
                if (__instance.Caster == null || __instance.Caster.SpellSynthesis() == false) return;
                if (__instance.Spellbook != __instance.Caster.Ensure<UnitPartMysticTheurge>().DivineSpellbook
                    && __instance.Spellbook != __instance.Caster.Ensure<UnitPartMysticTheurge>().ArcaneSpellbook)
                {
                    __result = false;
                    return;
                }
                if (__instance.Spellbook == __instance.Caster.Ensure<UnitPartMysticTheurge>().DivineSpellbook
                    && __instance.Caster.MTHasCastDivineSpell())
                {
                    __result = false;
                    return;
                };
                if (__instance.Spellbook == __instance.Caster.Ensure<UnitPartMysticTheurge>().ArcaneSpellbook
                    && __instance.Caster.MTHasCastArcaneSpell())
                {
                    __result = false;
                    return;
                };
            }
        }

        [HarmonyPatch(typeof(Spellbook), "GetSpontaneousConversionSpells", typeof(AbilityData))]
        internal class Spellbook_GetSpontaneousConversionSpells_MM
        {
            [HarmonyPostfix]
            private static void GetSpontaneousConversionSpells(AbilityData spell, ref IEnumerable<BlueprintAbility> __result,
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
                    foreach (var entry in patron.LevelEntries)
                    {
                        if (entry.Level > __instance.Owner.Progression.GetClassLevel(wizard))
                        {
                            continue;
                        }
                        var components = entry.Features.Select(f => f.GetComponent<AddKnownSpell>());
                        foreach (var comp in components)
                        {
                            if (comp.SpellLevel <= spellLevel)
                            {
                                list.Add(comp.m_Spell.Get());
                            }
                        }
                    }
                    __result = list.ToArray();
                }
            }
        }
    }
}
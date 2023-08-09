using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using MysticalMayhem.Helpers;
using MysticalMayhem.Mechanics;
using MysticalMayhem.Mechanics.Parts;
using System.Collections.Generic;

namespace MysticalMayhem.HarmonyPatches
{
    [HarmonyPatch(typeof(AbilityData))]
    internal class AbilityDataPatcher
    {
        [HarmonyPatch("get_RequireFullRoundAction")]
        [HarmonyPostfix]
        private static bool get_RequireFullRoundAction_MM(bool result, AbilityData __instance)
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

        [HarmonyPatch(nameof(AbilityData.GetParamsFromItem), typeof(ItemEntity))]
        [HarmonyPostfix]
        private static void GetParamsFromItem_MM(ref AbilityParams __result, AbilityData __instance, ItemEntity itemEntity)
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

        [HarmonyPatch("get_ActionType")]
        [HarmonyPostfix]
        private static void get_ActionType_MM(ref UnitCommand.CommandType __result, AbilityData __instance)
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

        [HarmonyPatch("get_CanBeCastByCaster")]
        [HarmonyPostfix]
        private static void get_CanBeCastByCaster_MM(ref bool __result, AbilityData __instance)
        {
            // Spell Synthesis check:
            // - User has the buff indicating they've used Spell Synthesis.
            // - User has the buff indicating they've casted a spell from one of their Mystic Theurge spellbooks.
            // - The spell being cast is from the same MT spellbook.
            if (__instance.Caster == null || !__instance.Caster.SpellSynthesis()) return;
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

        // Spontaneous conversion for shadow spells nonsense.
        [HarmonyPatch(nameof(AbilityData.AddAbilityUnique))]
        [HarmonyPrefix]
        private static bool AddAbilityUnique_MM(ref List<AbilityData> result, AbilityData ability)
        {
            if (!ability.AbilityShadowSpell) return true;

            result = result ?? new();

            foreach (BlueprintAbility blueprintAbility2 in ability.AbilityShadowSpell.GetAvailableSpells())
            {
                AbilityVariants abilityVariants2 = blueprintAbility2.AbilityVariants.Or(null);
                ReferenceArrayProxy<BlueprintAbility, BlueprintAbilityReference>? referenceArrayProxy3 = 
                    (abilityVariants2 != null) ? 
                    new ReferenceArrayProxy<BlueprintAbility, BlueprintAbilityReference>?(abilityVariants2.Variants) : null;
                if (referenceArrayProxy3 != null)
                {
                    using (ReferenceArrayProxy<BlueprintAbility, BlueprintAbilityReference>.Enumerator enumerator = referenceArrayProxy3.Value.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            BlueprintAbility replaceBlueprint3 = enumerator.Current;
                            AbilityData.AddAbilityUnique(ref result, new AbilityData(ability, replaceBlueprint3));
                        }
                        continue;
                    }
                }
                AbilityData.AddAbilityUnique(ref result, new AbilityData(ability, blueprintAbility2));
            }
            return false;
        }

        [HarmonyPatch(nameof(AbilityData.SpendOneSpellCharge))]
        [HarmonyPrefix]
        private static bool SpendOneSpellCharge_MM(AbilityData __instance)
        {
            // Warlock's Life Tap and Comfortable Insanity features.
            var part = __instance.Caster.Get<UnitPartWarlock>();
            if (part is null) return true;
            if (part.SaveSpellSlot(__instance)) return false;
            return true;
        }

        [HarmonyPatch("get_ActionType")]
        [HarmonyPostfix]
        private static void get_ActionType_MM(AbilityData __instance, ref UnitCommand.CommandType __result)
        {
            // Invoker's Desctructive Impulse.
            if (__instance.Caster.Get<UnitPartWarlock>()?.GetFeature(UnitPartWarlock.Feature.InvokerImpulse)
                && (__instance.Blueprint == BPLookup.Ability("InvokerEldritchBlastAbility", true)
                    || __instance.Blueprint == BPLookup.Ability("InvokerEldritchChainAbility", true)))
            {
                __result = UnitCommand.CommandType.Move;
            }
        }
    }
}
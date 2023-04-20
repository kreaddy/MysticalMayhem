using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using MysticalMayhem.Helpers;
using Owlcat.Runtime.UI.Tooltips;
using System;
using System.Linq;
using System.Text;

namespace MysticalMayhem.Mechanics
{
    public static class DescriptorExtender
    {
        public struct ExtendedDescriptorWrapper
        {
            private UnitDescriptor _unit;
            private BlueprintSpellbook _spellbook;
            private BlueprintAbility _ability;
            private SpellDescriptor _spellDescriptor;

            public ExtendedDescriptorWrapper(UnitDescriptor unit, BlueprintSpellbook spellbook, BlueprintAbility ability, SpellDescriptor descriptor)
            {
                _unit = unit;
                _spellbook = spellbook;
                _ability = ability;
                _spellDescriptor = descriptor;
            }

            public UnitDescriptor Unit => _unit;
            public BlueprintSpellbook Spellbook => _spellbook;
            public BlueprintAbility Ability => _ability;
            public SpellDescriptor SpellDescriptor => _spellDescriptor;
        }

        [Flags]
        public enum SpellDescriptor : long
        {
            None = 0,
            Draconic = 1 << 0,
            SpellProtection = 1 << 1,
            Tormenting = 1 << 2
        }

        /// <summary>
        /// Add a new spell descriptor to a feature, ability or buff.
        /// </summary>
        [AllowedOn(typeof(BlueprintAbility), false)]
        [AllowedOn(typeof(BlueprintBuff), false)]
        [AllowedOn(typeof(BlueprintFeature), false)]
        [AllowedOn(typeof(BlueprintAbilityAreaEffect), false)]
        [AllowMultipleComponents]
        [TypeId("f5dde5f8-8482-400f-b573-41a1a3a1990f")]
        public class AddExtendedSpellDescriptor : BlueprintComponent
        {
            public SpellDescriptor Descriptor;

        }

        /// <summary>
        /// Add a new spell descriptor based on conditions.
        /// </summary>
        [AllowedOn(typeof(BlueprintAbility), false)]
        [AllowedOn(typeof(BlueprintBuff), false)]
        [AllowedOn(typeof(BlueprintFeature), false)]
        [AllowMultipleComponents]
        [TypeId("2ba406be-582d-4952-80cf-285f23947896")]
        public class AddContextualSpellDescriptor : UnitFactComponentDelegate
        {
            public SpellDescriptor Descriptor;
            public BlueprintAbilityReference Ability;
            public BlueprintSpellListReference SpellList;
            public BlueprintSpellbookReference Spellbook;
        }

        /// <summary>
        /// Fetches the new spell descriptors from an ability blueprint.
        /// </summary>
        public static SpellDescriptor GetExtendedSpellDescriptors(BlueprintAbility ability)
        {
            var descriptors = SpellDescriptor.None;
            ability.GetComponents<AddExtendedSpellDescriptor>().ForEach(action: c => { descriptors |= c.Descriptor; });
            Main.DebugLog($"Ability [[{ability.NameSafe()}]] has the descriptors: [[{descriptors}]].");
            return descriptors;
        }

        /// <summary>
        /// Appends the localized descriptions of the new spell descriptors to the old ones.
        /// </summary>
        public static string AppendDescriptorsText(BlueprintAbility ability)
        {
            var stringBuilder = new StringBuilder();
            var descriptors = GetExtendedSpellDescriptors(ability);
            EnumUtils.GetValues<SpellDescriptor>()
                .Where(value => value > SpellDescriptor.None && descriptors.HasFlag(value))
                .ForEach(action: descriptor => { UIUtilityTexts.AddWord(stringBuilder, LocalizationManager.CurrentPack.GetText($"MM_EXD_{descriptor}")); });

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Patches the <see cref="UIUtilityTexts"/> static class to display our new descriptors on the tooltips.
        /// </summary>
        public static void PatchUIUtilityTexts()
        {
            var method = AccessTools.Method(typeof(UIUtilityTexts), nameof(UIUtilityTexts.GetSpellDescriptorsText));
            var patch = AccessTools.Method(typeof(DescriptorExtender), nameof(DescriptorExtender.GetSpellDescriptorsTextMM));
            var harmony = new Harmony(Main.Mod.Info.Id);
            harmony.Patch(method, postfix: new HarmonyMethod(patch));
        }

        private static void GetSpellDescriptorsTextMM(ref string __result, BlueprintAbility abilityBlueprint)
        {
            __result += AppendDescriptorsText(abilityBlueprint);
        }

        /// <summary>
        /// Patches the <see cref="TooltipTemplateAbility"/> class to display contextual new descriptors.
        /// </summary>
        public static void PatchTooltipTemplateAbility()
        {
            var method = AccessTools.Method(typeof(TooltipTemplateAbility), nameof(TooltipTemplateAbility.Prepare), new Type[] { typeof(TooltipTemplateType) });
            var patch = AccessTools.Method(typeof(DescriptorExtender), nameof(DescriptorExtender.PrepareMM));
            var harmony = new Harmony(Main.Mod.Info.Id);
            harmony.Patch(method, postfix: new HarmonyMethod(patch));
        }

        private static void PrepareMM(TooltipTemplateAbility __instance)
        {
            var components = __instance.m_AbilityData?.Caster?
                .Facts.m_Facts
                .SelectMany(fact => fact.SelectComponents<AddContextualSpellDescriptor>());
            if (components is null || components.Count() == 0) return;

            var descriptors = SpellDescriptor.None;
            components
                .Where(c => c.Ability?.Get() == __instance.m_AbilityData?.Blueprint || (c.Spellbook?.Get() == __instance.m_AbilityData?.Spellbook?.Blueprint
                    && (c.SpellList is null || __instance.m_AbilityData.IsInSpellList(c.SpellList.Get()))))
                .Select(filtered => filtered.Descriptor)
                .ForEach(func: descriptor => descriptors |= descriptor);

            if (descriptors == SpellDescriptor.None) return;

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(__instance.m_SpellDescriptor);
            EnumUtils.GetValues<SpellDescriptor>()
                .Where(value => value > SpellDescriptor.None && descriptors.HasFlag(value))
                .ForEach(action: descriptor => { UIUtilityTexts.AddWord(stringBuilder, LocalizationManager.CurrentPack.GetText($"MM_EXD_{descriptor}")); });

            __instance.m_SpellDescriptor = stringBuilder.ToString();
        }
    }
}

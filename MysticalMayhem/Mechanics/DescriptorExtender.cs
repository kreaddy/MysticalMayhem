using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using MysticalMayhem.Helpers;
using System;
using System.Linq;
using System.Text;

namespace MysticalMayhem.Mechanics
{
    public static class DescriptorExtender
    {
        [Flags]
        public enum SpellDescriptor : long
        {
            None = 0,
            Draconic = 1 << 0,
            SpellProtection = 1 << 1
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
        /// Fetches the new spell descriptors from an ability blueprint.
        /// </summary>
        public static SpellDescriptor GetExtendedSpellDescriptors(BlueprintAbility ability)
        {
            var descriptors = SpellDescriptor.None;
            ability.GetComponents<AddExtendedSpellDescriptor>().ForEach(action: c => { descriptors |= c.Descriptor; });
            //Main.DebugLog($"Ability [[{ability.NameSafe()}]] has the descriptors: [[{descriptors}]].");
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
        /// Patches the UIUtilityText static class to display our new descriptors on the tooltips.
        /// </summary>
        public static void PatchUIUtilityTexts()
        {
            var method = AccessTools.Method(typeof(UIUtilityTexts), "GetSpellDescriptorsText");
            var patch = AccessTools.Method(typeof(DescriptorExtender), "GetSpellDescriptorsTextMM");
            var harmony = new Harmony(Main.Mod.Info.Id);
            harmony.Patch(method, postfix: new HarmonyMethod(patch));
        }

        private static void GetSpellDescriptorsTextMM(ref string __result, BlueprintAbility abilityBlueprint)
        {
            __result += AppendDescriptorsText(abilityBlueprint);
        }
    }
}

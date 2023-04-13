using HarmonyLib;
using Kingmaker.UnitLogic.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owlcat.Runtime.UI.MVVM;
using Kingmaker.UI.MVVM._VM.ServiceWindows.Spellbook.KnownSpells;
using Kingmaker.UI.MVVM._VM.ServiceWindows.Spellbook.Metamagic;
using Kingmaker.Blueprints.Classes;
using Kingmaker.RuleSystem.Rules;
using MysticalMayhem.Mechanics;
using static Kingmaker.Blueprints.Classes.FeatureGroup;
using MysticalMayhem.Helpers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
/*
namespace MysticalMayhem.HarmonyPatches
{
    [HarmonyPatch(typeof(RuleCollectMetamagic))]
    internal class RuleCollectMetamagicPatcher
    {
        [HarmonyPatch(nameof(RuleCollectMetamagic.AddMetamagic), typeof(Feature))]
        [HarmonyPostfix]
        private static void AddMetamagic_MM(RuleCollectMetamagic __instance, Feature metamagicFeature)
        {
            if (__instance.Spell == null || __instance.Spellbook.Blueprint.CharacterClass.AssetGuid != BPLookup.Class("WizardClass").AssetGuid) return;
            __instance.Spellbook.Owner.Progression.Features.Enumerable
                .Where(feature => feature.Blueprint.Groups.Contains(OracleCurse) || feature.Blueprint.Groups.Contains(WitchPatron))
                .ForEach(feature => CheckContainerAndReduceCost(feature, __instance, metamagicFeature));
        }

        private static void CheckContainerAndReduceCost(Feature feature, RuleCollectMetamagic rule, Feature metamagic)
        {
            var component = metamagic.GetComponent<AddMetamagicFeat>();
            if (component != null) { return; }
            var level = rule.Spellbook.Owner.Progression.GetProgression(feature.Blueprint as BlueprintProgression).Level;
            var container = feature.GetComponent<SpellListContainer>();
            var spell = rule.Spell;
            if (container != null && container.SpellList.ContainsKey(spell) && container.SpellList[spell] <= level && component.Metamagic != Metamagic.CompletelyNormal)
            {
                if (rule.m_SpellLevel + component.Metamagic.DefaultCost() > 11)
                {
                    if (!rule.SpellMetamagics.Contains(metamagic) && (rule.Spell.AvailableMetamagic & component.Metamagic) == component.Metamagic)
                    {
                        rule.SpellMetamagics.Add(metamagic);
                    }
                }
            }
        }
    }
}*/
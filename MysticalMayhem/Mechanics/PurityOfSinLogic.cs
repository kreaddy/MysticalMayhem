using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using MysticalMayhem.Helpers;
using System.Collections.Generic;

namespace MysticalMayhem.Mechanics
{
    /// <summary>
    /// This component adds a bonus to caster level if a spell matches the caster's Sin Magic specialization.
    /// </summary>
    [TypeId("888838d4-8aa2-47a0-b504-8bf548f30e29")]
    [AllowedOn(typeof(BlueprintFeature), false)]
    public class PurityOfSinLogic : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>,
        IRulebookHandler<RuleCalculateAbilityParams>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public int Bonus;
        private Dictionary<SpellSchool, string> _dict;

        public PurityOfSinLogic()
        {
            _dict = new()
            {
                { SpellSchool.Abjuration, "ThassAbjurationSpellbook" },
                { SpellSchool.Conjuration, "ThassConjurationSpellbook" },
                { SpellSchool.Enchantment, "ThassEnchantmentSpellbook" },
                { SpellSchool.Evocation, "ThassEvocationSpellbook" },
                { SpellSchool.Illusion, "ThassIllusionSpellbook" },
                { SpellSchool.Necromancy, "ThassNecromancySpellbook" },
                { SpellSchool.Transmutation, "ThassTransmutationSpellbook" }
            };
        }

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt.Initiator != Owner || evt.AbilityData == null || evt.Spell == null || evt.Spell.School == SpellSchool.None ||
                evt.Spell.School == SpellSchool.Universalist || evt.AbilityData.Spellbook == null) return;
            if (_dict[evt.Spell.School] != null && BPLookup.Spellbook(_dict[evt.Spell.School]).AssetGuid == evt.AbilityData.SpellbookBlueprint.AssetGuid)
            {
                evt.AddBonusCasterLevel(Bonus);
                // If the caster also has the Mythic version, the bonus also applies to the DC.
                if (evt.Initiator.Descriptor.GetEXFeature(FeatureExtender.Feature.MythicPurityOfSin))
                {
                    evt.AddBonusDC(Bonus);
                }
            }
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        { }
    }
}
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using MysticalMayhem.Helpers;

namespace MysticalMayhem.Mechanics.Components
{
    /// <summary>
    /// This component deals damage to characters who cast spells. If they have fewer HP than the damage they don't die, the spell just fizzles.
    /// </summary>
    [TypeId("f1af8b5c-33b4-4c9e-bf93-9445eac37250")]
    [AllowedOn(typeof(BlueprintBuff), false)]
    public class FlagellantMortifiedCastingDamage : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleCastSpell>,
        IRulebookHandler<RuleCastSpell>, ISubscriber, IInitiatorRulebookSubscriber
    {
        protected bool AlreadyTriggeredOnce = false;

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            if (AlreadyTriggeredOnce || !evt.Spell.Spellbook.Blueprint.AssetGuid.Equals(BPLookup.Spellbook("ClericSpellbook").AssetGuid)) return;
            var mult = evt.Initiator.Descriptor.GetEXFeature(FeatureExtender.Feature.GreaterMortifiedCasting) ? 2 : 1;
            if (evt.Initiator.HPLeft <= evt.Spell.SpellLevel * mult)
            {
                evt.ForceFail = true;
                return;
            }
            AlreadyTriggeredOnce = true;
            evt.Initiator.Damage += evt.Spell.SpellLevel * mult;
        }

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
            if (AlreadyTriggeredOnce || !evt.Spell.Spellbook.Blueprint.AssetGuid.Equals(BPLookup.Spellbook("ClericSpellbook").AssetGuid)) return;
            var mult = evt.Initiator.Descriptor.GetEXFeature(FeatureExtender.Feature.GreaterMortifiedCasting) ? 2 : 1;
            if (evt.Initiator.HPLeft <= evt.Spell.SpellLevel * mult)
            {
                evt.ForceFail = true;
                return;
            }
        }
    }
}
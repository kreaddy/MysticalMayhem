using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using MysticalMayhem.Helpers;
using System;

namespace MysticalMayhem.Mechanics
{
    [TypeId("0f7606be-35ac-4835-b610-74d70b5c4e75")]
    [AllowedOn(typeof(BlueprintFeature), false)]
    public class WhispersOfMadnessLogic : UnitFactComponentDelegate, ITargetRulebookHandler<RuleApplySpell>, IRulebookHandler<RuleApplySpell>,
        ITargetRulebookHandler<RuleSavingThrow>, IRulebookHandler<RuleSavingThrow>, ISubscriber, ITargetRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleApplySpell evt)
        {
        }

        public void OnEventDidTrigger(RuleApplySpell evt)
        {
            Main.DebugLog($"Whispers of Madness: owner is {Owner.CharacterName}, spell target is {evt.GetRuleTarget().CharacterName} and initiator is {evt.Initiator.CharacterName}.");
            Main.DebugLog($"Whispers of Madness: context has the Mind-Affecting descriptor {evt.Context.SpellDescriptor.HasFlag(SpellDescriptor.MindAffecting)}.");

            if (Owner != evt.GetRuleTarget() || !evt.Context.SpellDescriptor.HasFlag(SpellDescriptor.MindAffecting) || Owner == evt.Initiator) { return; }
            var levels = Owner.Progression.GetClassLevel(BPLookup.Class("WarlockClass", true));
            var damage = new EnergyDamage(new DiceFormula(_failedWillSave ? Math.Max(levels / 2, 1) : 1, DiceType.D6), Owner.Stats.Wisdom.Bonus, DamageEnergyType.Magic);

            Main.DebugLog($"Whispers of Madness feedback: {damage}.");

            var rule = new RuleDealDamage(Owner, evt.Initiator, damage);
            Rulebook.Trigger(rule);

            _failedWillSave = false;
        }

        public void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            _failedWillSave = false;
        }

        public void OnEventDidTrigger(RuleSavingThrow evt)
        {
            if (evt.Type == SavingThrowType.Will) _failedWillSave = true;
        }

        private bool _failedWillSave;
    }
}

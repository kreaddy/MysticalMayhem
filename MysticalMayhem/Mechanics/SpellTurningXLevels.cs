using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Controllers;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace MysticalMayhem.Mechanics
{
    /// <summary>
    /// This component reflects 1d4 + X levels of spell to the caster.
    /// </summary>
    [AllowedOn(typeof(BlueprintBuff), false)]
    [TypeId("c0dd366b-76a0-4000-bc86-df1d024e9a34")]
    public class SpellTurningXLevels : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>, ISubscriber, ITargetRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
            if (_doOnce == false)
            {
                _totalLevels = RulebookEvent.Dice.D4 + BonusLevels;
                _doOnce = true;
                Main.DebugLog($"Spell Turning max capacity: {_totalLevels}.");
            }
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            AbilityExecutionProcess result = evt.Result;
            if (!evt.Success || result == null || evt.SpellTarget.Unit == evt.Initiator) { return; }
            if (_totalLevels <= 0 || evt.Spell.Range == Kingmaker.UnitLogic.Abilities.Blueprints.AbilityRange.Touch) { return; }
            evt.SetSuccess(false);
            evt.CancelAbilityExecution();
            if (evt.Spell.SpellLevel <= _totalLevels)
            {
                AbilityExecutionContext context = result.Context.CloneFor(Owner, evt.Initiator);
                Game.Instance.AbilityExecutor.Execute(context);
                EventBus.RaiseEvent<ISpellTurningHandler>(h =>
                {
                    h.HandleSpellTurned(evt.Initiator, Owner, evt.Spell);
                }, true);
            }
            _totalLevels -= evt.Spell.SpellLevel;

            if (_totalLevels <= 0)
            {
                Owner.Buffs.RemoveFact(Fact);
                Main.DebugLog("Spell Turning expired: removing buff.");
            }
        }

        private bool _doOnce;
        private int _totalLevels;

        [SerializeField]
        private int BonusLevels;

        [SerializeField]
        private DescriptorExtender.SpellDescriptor Descriptor;
    }
}

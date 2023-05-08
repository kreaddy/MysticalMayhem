using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace MysticalMayhem.Mechanics.Actions
{
    /// <summary>
    /// This component directly converts damage dealt into hit points.
    /// </summary>
    [TypeId("49833c6b-bf5a-4b9f-a282-22bc17fa9b3c")]
    [AllowedOn(typeof(BlueprintAbility))]
    [AllowedOn(typeof(BlueprintBuff))]
    public class ContextActionDrainHP : ContextAction, IDealDamageProvider, IHealDamageProvider
    {
        public override string GetCaption()
        {
            return "Absorbs hit points.";
        }

        public RuleDealDamage GetDamageRule(UnitEntityData initiator, UnitEntityData target)
        {
            var formula = new DiceFormula(Value.DiceCountValue.Calculate(Context), Value.DiceType);
            var damage = new DirectDamage(formula);
            var rule = new RuleDealDamage(initiator, target, damage);
            return rule;
        }

        public RuleHealDamage GetHealRule(UnitEntityData initiator, UnitEntityData target)
        {
            var rule = new RuleHealDamage(initiator, target, _result);
            return rule;
        }

        public override void RunAction()
        {
            if (Target.Unit is null) return;
            var result = Context.TriggerRule(GetDamageRule(Context.MaybeCaster, Target.Unit));
            _result = result.RawResult;
            Context.TriggerRule(GetHealRule(Context.MaybeCaster, Context.MaybeCaster));
        }

        public ContextDiceValue Value;
        private int _result;
    }
}

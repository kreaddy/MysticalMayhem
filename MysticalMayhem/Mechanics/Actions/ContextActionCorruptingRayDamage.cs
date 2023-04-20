using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using MysticalMayhem.Mechanics.Parts;

namespace MysticalMayhem.Mechanics.Actions
{
    /// <summary>
    /// This component exclusively deals 1d6 damage per tormenting spell/hex that were active on the target when the user started its turn.
    /// </summary>
    [TypeId("6993f941-7081-4b88-8b90-a404d001f29d")]
    [AllowedOn(typeof(BlueprintAbility))]
    public class ContextActionCorruptingRayDamage : ContextAction, IDealDamageProvider
    {
        public override string GetCaption()
        {
            return "Deal 1d6 damage + 1d6 per tormenting effect.";
        }

        public RuleDealDamage GetDamageRule(UnitEntityData initiator, UnitEntityData target)
        {
            var part = target.Get<UnitPartTormentingEffects>();
            var rolls = part is null ? 1 : part.GetEffectsCount(initiator) + 1;
            var formula = new DiceFormula() { m_Dice = DiceType.D6, m_Rolls = rolls };
            var damage = new EnergyDamage(formula, DamageEnergyType.Magic);
            var rule = new RuleDealDamage(initiator, target, damage);
            return rule;
        }

        public override void RunAction()
        {
            if (Target.Unit is null) return;
            Context.TriggerRule(GetDamageRule(Context.MaybeCaster, Target.Unit));
        }
    }
}
using Kingmaker;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Kingmaker;
using TurnBased.Controllers;

namespace MysticalMayhem.Mechanics.Commands
{
    internal class UnitSelfHarmNonLethal : UnitCommand
    {
        /// <summary>
        /// This command is identical to <see cref="Kingmaker.UnitLogic.Commands.UnitSelfHarm"/> except it can never reduce HP below 1.
        /// </summary>
        public UnitSelfHarmNonLethal() : base(CommandType.Standard, null, false)
        {
        }

        public override void TriggerAnimation()
        {
            StartAnimation(UnitAnimationType.Hit, null);
        }

        public override ResultType OnAction()
        {
            var physicalDamage = new PhysicalDamage(new ModifiableDiceFormula(new DiceFormula(1, DiceType.D8)), 0, PhysicalDamageForm.Bludgeoning);
            physicalDamage.AddModifier(new Modifier(Executor.Stats.Strength.Bonus, Kingmaker.EntitySystem.Stats.StatType.Strength));

            var rule = new RuleDealDamage(Executor, Executor, physicalDamage);
            rule.MinHPAfterDamage = 1;
            Rulebook.Trigger(rule);

            if (CombatController.IsInTurnBasedCombat() && Executor.IsCurrentUnit())
            {
                Game.Instance.TurnBasedCombatController.CurrentTurn.ForceToEnd(true);
            }
            return ResultType.Success;
        }
    }
}

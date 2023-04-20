using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility;
using MysticalMayhem.Mechanics.Parts;
using System.Linq;

namespace MysticalMayhem.Mechanics.Components
{
    [TypeId("6334020d-2429-4c19-8ad5-dec76a99b9db")]
    [AllowedOn(typeof(BlueprintBuff))]
    public class InfectiousGuiltLogic : UnitBuffComponentDelegate, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>
    {
        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            Main.DebugLog($"Target HP: {Owner.Descriptor.Stats.HitPoints}. Took {Owner.Descriptor.Damage}");
            if (evt.Target == Owner && (Owner.Descriptor.Stats.HitPoints <= Owner.Descriptor.Damage))
            {
                var batches = Owner
                    .Ensure<UnitPartTormentingEffects>()
                    .GetBatches();

                foreach (var batch in batches)
                {
                    var caster = batch.Key;
                    var newTarget = EntityBoundsHelper
                        .FindUnitsInRange(Owner.Position, 30)
                        .Where(u => u.IsEnemy(caster))
                        .FirstOrDefault();

                    if (newTarget != null)
                    {
                        Main.DebugLog($"Infectious Guilt: new target {newTarget.CharacterName} found!");
                        var dc = 10 + (caster.Progression.GetClassLevel(UnitPartWarlock.Class) / 2) + caster.Stats.Intelligence.Bonus;
                        var roll = new RuleSavingThrow(newTarget, SavingThrowType.Will, dc);

                        var rollResult = Rulebook.Trigger(roll);

                        if (rollResult.Success) return;
                        Main.DebugLog("Infectious Guilt's saving throw failed!");
                        foreach (var effect in batch)
                        {
                            var source = effect.Source;
                            var projectiles = effect.ProjectileCount;
                            var hex = effect.Hex;
                            var bundle = effect.Bundle;
                            var rounds = effect.Rounds;

                            newTarget.Ensure<UnitPartTormentingEffects>().AddEffect(source, caster, bundle, projectiles, rounds, hex);
                        }

                        GameHelper.ApplyBuff(newTarget, UnitPartWarlock.InfectiousGuiltBuff);

                        foreach (var buff in Owner.Buffs)
                        {
                            if (buff.Context?.SpellDescriptor.HasFlag(SpellDescriptor.Hex) is true && buff.Context?.MaybeCaster?.Descriptor == caster.Descriptor)
                            {
                                newTarget.Buffs.AddBuff(buff.Blueprint, buff.Context, buff.PlannedDuration);
                            }
                        }
                    }
                }
            }
        }

    }
}
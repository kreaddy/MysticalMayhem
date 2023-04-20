using JetBrains.Annotations;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using System.Collections.Generic;
using System.Linq;

namespace MysticalMayhem.Mechanics.Parts
{
    internal class UnitPartTormentingEffects : UnitPart, IUnitNewCombatRoundHandler
    {
        private List<TormentingEffect> _activeEffects = new();

        public void AddEffect(BlueprintAbility source, UnitEntityData caster, [CanBeNull] IDamageBundleReadonly bundle, int count = 1, int rounds = 2, bool hex = false)
        {
            if (_activeEffects
                .Where(e => e.Source == source)
                .Count() == 0)
            {
                Main.DebugLog($"Tormenting effect rounds: {rounds}");
                _activeEffects.Add(new TormentingEffect(source, caster, bundle, count, rounds, hex,
                    caster.Ensure<UnitPartWarlock>().GetFeature(UnitPartWarlock.Feature.Affliction).Count));
            }
        }

        public void Clear()
        {
            _activeEffects.Clear();
        }

        public int GetEffectsCount(UnitEntityData caster)
        {
            return _activeEffects
                .Where(e => e.Caster == caster)
                .Count();
        }

        public Stack<TormentingEffect> GetEffects(UnitEntityData caster)
        {
            return _activeEffects
                .Where(e => e.Caster == caster)
                .ToStack();
        }

        public IEnumerable<IGrouping<UnitEntityData, TormentingEffect>> GetBatches()
        {
            return _activeEffects
                .GroupBy(effect => effect.Caster);
        }

        public void HandleNewCombatRound(UnitEntityData unit)
        {
            if (Owner.State.IsDead) { Clear(); Dispose(); return; }

            ProcessAffliction(unit);
            ProcessEcho(unit);
        }

        /// <summary>
        /// If the caster of the tormenting effect has the Affliction feature then the unit takes additional damage
        /// at the start of the caster's round. (1d6 per effect, or 1d8 with the Unstable Affliction fiendish blessing.)
        /// If there is more than 1 caster we proceed by batches, one batch per caster.
        /// </summary>
        protected void ProcessAffliction(UnitEntityData unit)
        {
            var batches = GetBatches();

            foreach (var batch in batches)
            {
                var caster = batch.Select(effect => effect.Caster).First();
                if (caster != unit || caster.State.IsHelpless) continue;

                var diceType = batch.Select(effect => effect.AfflictionCount).First() == 2 ? DiceType.D8 : DiceType.D6;
                var dice = new DiceFormula() { m_Dice = diceType, m_Rolls = batch.Count() };
                var damage = new EnergyDamage(dice, Kingmaker.Enums.Damage.DamageEnergyType.Magic);
                var rule = new RuleDealDamage(caster, Owner, damage);

                Rulebook.Trigger(rule);
            }
        }

        /// <summary>
        /// If this unit is under the effect of a tormenting spell the saved damage bundle is inflicted again
        /// at the beginning of the original caster's turn.
        /// </summary>
        protected void ProcessEcho(UnitEntityData unit)
        {
            var toDispose = new Queue<TormentingEffect>();

            foreach (var effect in _activeEffects)
            {
                if (unit != effect.Caster) continue;
                effect.Rounds--;
                if (effect.Hex) continue;
                var bundle = new DamageBundle(effect.Bundle.ToArray());

                for (int i = 0; i < effect.ProjectileCount; i++)
                {
                    var rule = new RuleDealDamage(effect.Caster, Owner, bundle);
                    Rulebook.Trigger(rule);
                }

                if (effect.Rounds <= 0)
                {
                    Main.DebugLog("Tormenting spell ends.");
                    toDispose.Enqueue(effect);
                }

                while (toDispose.Count > 0) _activeEffects.Remove(toDispose.Dequeue());

                if (_activeEffects.Empty() && Owner.HasFact(UnitPartWarlock.InfectiousGuiltBuff))
                {
                    GameHelper.RemoveBuff(Owner, UnitPartWarlock.InfectiousGuiltBuff);
                }
            }
        }

        internal class TormentingEffect
        {
            private BlueprintAbility _source;
            private UnitEntityData _caster;
            private IDamageBundleReadonly _bundle;
            private int _rounds;
            private int _projectileCount;
            private bool _hex;
            private int _affliction;

            public BlueprintAbility Source => _source;
            public UnitEntityData Caster => _caster;
            public IDamageBundleReadonly Bundle => _bundle;
            public int Rounds { get => _rounds; set => _rounds = value; }
            public int ProjectileCount => _projectileCount;
            public bool Hex => _hex;
            public int AfflictionCount => _affliction;

            public TormentingEffect(BlueprintAbility source, UnitEntityData caster, IDamageBundleReadonly bundle, int count, int rounds, bool hex, int affliction)
            {
                _source = source;
                _caster = caster;
                _bundle = bundle;
                _rounds = rounds;
                _projectileCount = count;
                _hex = hex;
                _affliction = affliction;
            }
        }
    }
}

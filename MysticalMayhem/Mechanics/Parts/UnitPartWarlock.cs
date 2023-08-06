using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using MysticalMayhem.HarmonyPatches;
using MysticalMayhem.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MysticalMayhem.Mechanics.Parts
{
    public class UnitPartWarlock : OldStyleUnitPart, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>,
        IInitiatorRulebookHandler<RuleApplyMetamagic>, IRulebookHandler<RuleApplyMetamagic>, ITargetRulebookHandler<RuleApplySpell>, IRulebookHandler<RuleApplySpell>,
        ITargetRulebookHandler<RuleSavingThrow>, IRulebookHandler<RuleSavingThrow>, IInitiatorRulebookHandler<RulePrepareDamage>, IRulebookHandler<RulePrepareDamage>,
        IInitiatorRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>,
        ITargetRulebookHandler<RuleAttackRoll>
    {

        public static BlueprintCharacterClass Class => BPLookup.Class("WarlockClass", true);
        public static BlueprintBuff Confusion => BPLookup.Buff("ConfusionBuff");
        public static BlueprintSpellbook Spellbook => BPLookup.Spellbook("WarlockSpellbook", true);
        public static BlueprintSpellList TormentingList => BPLookup.SpellList("WarlockTormentingSpellList", true);
        public static BlueprintBuff TormentingSpellcasting => BPLookup.Buff("WarlockTormentingBuff", true);
        public static BlueprintBuff InfectiousGuiltBuff => BPLookup.Buff("WarlockInfectiousGuiltBuff", true);
        public static BlueprintBuff DaemonicIncarnationBuff => BPLookup.Buff("WarlockDaemonic20Buff", true);

        private readonly Dictionary<Feature, CountableFlag> List = new();

        private bool _failedWillSave;

        public enum Feature
        {
            LifeTap,
            MaleficiumRank,
            MinusOneToConfusionRolls,
            ComfortableInsanity,
            CanAlwaysSelfConfuse,
            AlwaysRoll0WhenConfused,
            WhispersOfMadness,
            TormentingFocus,
            TormentingFocusMythic,
            DoubleDurationOfHarmfulBuffs,
            Affliction,
            InfectiousGuilt,
            InfernalIncarnation,
            DaemonicHunger,
            CopyMagusMechanics,
            IgnoreLifeTapCost,
            AbyssalFlanking,
            InvokerImpulse
        }

        public void AddFeature(Feature type)
        {
            var feature = GetFeature(type);
            feature.Retain();
        }

        public void RemoveFeature(Feature type)
        {
            var feature = GetFeature(type);
            feature.Release();
        }

        public void ClearFeature(Feature type)
        {
            var feature = GetFeature(type);
            feature.ReleaseAll();
        }

        public CountableFlag GetFeature(Feature type)
        {
            List.TryGetValue(type, out var feature);
            if (feature == null)
            {
                feature = new CountableFlag();
                List[type] = feature;
            }
            return feature;
        }

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt.Spell is null) return;

            ProcessMaleficiumAbilityParams(evt);
            ProcessTormentingFocus(evt);
            ProcessInfernalIncarnationDC(evt);
        }

        /// <summary>
        /// Maleficium adds DC and CL to spells with the Evil descriptor depending on feature rank.
        /// Rank 1: +1 DC. Rank 3: +2 DC. Rank 4: +2 CL.
        /// </summary>
        protected void ProcessMaleficiumAbilityParams(RuleCalculateAbilityParams evt)
        {
            if (Owner.Unit != evt.Initiator) return;

            var descriptor = evt.Spell.SpellDescriptor;
            descriptor = UnitPartChangeSpellElementalDamage.ReplaceSpellDescriptorIfCan(evt.Initiator, descriptor);

            if (!descriptor.HasAnyFlag(SpellDescriptor.Evil)) return;

            var rank = GetFeature(Feature.MaleficiumRank).Count;
            evt.AddBonusDC(rank >= 3 ? 2 : 1);
            if (rank == 4) evt.AddBonusCasterLevel(2);
        }

        /// <summary>
        /// Tormenting Focus adds +1 DC to spells with the Tormenting descriptor. Greater Tormenting Focus does the same.
        /// Mythic version doubles the bonus.
        /// </summary>
        protected void ProcessTormentingFocus(RuleCalculateAbilityParams evt)
        {
            if (!IsTormentingSpell(evt.AbilityData)) return;
            var bonus = GetFeature(Feature.TormentingFocus).Count * (1 + GetFeature(Feature.TormentingFocusMythic).Count);
            Main.DebugLog($"Tormenting Focus bonus DC = {bonus}.");
            evt.AddBonusDC(bonus, Kingmaker.Enums.ModifierDescriptor.Focus);
        }

        /// <summary>
        /// The Infernal Incarnation feature gives a +2 bonus to the DC of hexes and warlock spells.
        /// </summary>
        protected void ProcessInfernalIncarnationDC(RuleCalculateAbilityParams evt)
        {
            if (!GetFeature(Feature.InfernalIncarnation)) return;
            if (evt.Reason?.Context?.SpellDescriptor.HasFlag(SpellDescriptor.Hex) is true || evt.AbilityData.Spellbook?.Blueprint == Spellbook)
            {
                evt.AddBonusDC(2, Kingmaker.Enums.ModifierDescriptor.UntypedStackable);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }

        public void OnEventAboutToTrigger(RuleApplyMetamagic evt)
        {
        }

        public void OnEventDidTrigger(RuleApplyMetamagic evt)
        {
            ProcessMaleficiumMetamagic(evt);
        }

        /// <summary>
        /// At rank 2 Maleficium also reduces the combined cost of metamagic when applied to spells with the Evil descriptor by 1.
        /// Unlike the CL and DC increase, this only applies if the spell has the descriptor in the blueprint itself.
        /// </summary>
        protected void ProcessMaleficiumMetamagic(RuleApplyMetamagic evt)
        {
            if (Owner.Unit != evt.Initiator || evt.Spell.SpellDescriptor.HasAnyFlag(SpellDescriptor.Evil)) return;

            var rank = GetFeature(Feature.MaleficiumRank).Count;
            if (rank < 2) return;

            var min = evt.AppliedMetamagics.Contains(Metamagic.CompletelyNormal) ? evt.BaseLevel - 1 : evt.BaseLevel;
            if (evt.BaseLevel + evt.Result.SpellLevelCost > min) evt.Result.SpellLevelCost -= 1;

            Main.DebugLog($"Spell level after Maleficium: {evt.Result.SpellLevelCost}.");
        }


        public void OnEventAboutToTrigger(RuleApplySpell evt)
        {
        }

        public void OnEventDidTrigger(RuleApplySpell evt)
        {
            ProcessWhispersOfMadness(evt);
        }

        /// <summary>
        /// Whispers of Madness causes an attacker to take damage when targeting an unit with the feature with an ability that
        /// possesses the Mind-Affecting descriptor. Damage is 1d6 + Wisdom modifier, but if the ability requires a Will saving throw
        /// and the target failed it the damage is upgraded to 1d6 per 2 warlock levels + Wisdom modifier.
        /// </summary>
        protected void ProcessWhispersOfMadness(RuleApplySpell evt)
        {
            if (!GetFeature(Feature.WhispersOfMadness)) return;

            Main.DebugLog($"Whispers of Madness: owner is {Owner.CharacterName}, spell target is {evt.GetRuleTarget().CharacterName} and initiator is {evt.Initiator.CharacterName}.");
            Main.DebugLog($"Whispers of Madness: context has the Mind-Affecting descriptor {evt.Context.SpellDescriptor.HasFlag(SpellDescriptor.MindAffecting)}.");

            if (Owner.Unit != evt.GetRuleTarget() || !evt.Context.SpellDescriptor.HasFlag(SpellDescriptor.MindAffecting) || Owner == evt.Initiator) return;


            var levels = Owner.Progression.GetClassLevel(Class);
            var damage = new EnergyDamage(new DiceFormula(_failedWillSave ? Math.Max(levels / 2, 1) : 1, DiceType.D6), Owner.Stats.Wisdom.Bonus, DamageEnergyType.Magic);

            Main.DebugLog($"Whispers of Madness feedback: {damage}.");

            var rule = new RuleDealDamage(Owner.Unit, evt.Initiator, damage);
            Rulebook.Trigger(rule);

            _failedWillSave = false;
        }

        /// <summary>
        /// Just make sure the flag for a failed Will save is reset before making a new saving throw.
        /// </summary>
        public void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            _failedWillSave = false;
        }

        /// <summary>
        /// Set the flag indicating a Will saving throw has been failed.
        /// </summary>
        public void OnEventDidTrigger(RuleSavingThrow evt)
        {
            if (evt.Type == SavingThrowType.Will) _failedWillSave = true;
        }

        public void OnEventAboutToTrigger(RulePrepareDamage evt)
        {
        }

        public void OnEventDidTrigger(RulePrepareDamage evt)
        {
            if (evt.Reason?.Ability is null || evt.Initiator != Owner.Unit) return;

            ProcessTormentingSpellEffect(evt);
        }

        /// <summary>
        /// When a tormenting spell is cast, its immediate damage is reduced by half.
        /// Then the same damage bundle is re-applied for 2 additional rounds (3 with Infernal Incarnation).
        /// </summary>
        protected void ProcessTormentingSpellEffect(RulePrepareDamage evt)
        {
            if (!IsTormentingSpell(evt.Reason.Ability)) return;

            evt.DamageBundle
                .ForEach(action: damage => damage.m_BonusPercent = -50);

            var projectileCount = evt.Reason.Ability.AbilityDeliverProjectile is null ? 1 :
                Math.Min(evt.Reason.Context[evt.Reason.Ability.AbilityDeliverProjectile.MaxProjectilesCountRank],
                    evt.Reason.Ability.AbilityDeliverProjectile.m_Projectiles.Count());

            evt.GetRuleTarget()
                .Ensure<UnitPartTormentingEffects>()
                .AddEffect(evt.Reason.Ability.Blueprint, Owner.Unit, evt.DamageBundle, projectileCount, GetFeature(Feature.InfernalIncarnation) ? 3 : 2);

            if (GetFeature(Feature.InfectiousGuilt)) GameHelper.ApplyBuff(evt.GetRuleTarget(), InfectiousGuiltBuff);
        }

        /// <summary>
        /// Warlock spells are cast for free if the following conditions are fullfilled:
        /// 1) Life Tap is active. The caster gets a stacking HP debuff instead (or loses 1 minute of the Infernal Incarnation buff).
        /// 2) The caster has Comfortable Insanity and is under the Confusion effect.
        /// 
        /// If this method returns true, then <see cref="AbilityDataPatcher.SpendOneSpellCharge_MM"/> will prevent the spending of the slot.
        /// </summary>
        public bool SaveSpellSlot(AbilityData abilityData)
        {
            if (abilityData.SpellbookBlueprint.CharacterClass != Class) return false;

            if (GetFeature(Feature.ComfortableInsanity) && Owner.HasFact(Confusion)) return true;

            if (GetFeature(Feature.LifeTap) && abilityData.SpellLevel > 0)
            {
                if (GetFeature(Feature.InfernalIncarnation))
                {
                    Owner.Buffs.GetBuff(BPLookup.Buff("WarlockInfernal20Buff", true)).ReduceDuration(TimeSpan.FromMinutes(1));
                }
                else if (Owner.HPLeft > abilityData.SpellLevel * 2 && !GetFeature(Feature.IgnoreLifeTapCost))
                {
                    GameHelper.ApplyBuff(Owner, BPLookup.Buff($"LifeTap{abilityData.SpellLevel * 2}", true));
                }
                return true;
            }
            return false;
        }

        #region Confusion Override

        /// <summary>
        /// Part of the confusion effect override.
        /// Used by <see cref="UnitConfusionControllerPatcher.SelfHarm_MM"/> to replace regular self harm with a non lethal version
        /// when the unit has the Comfortable Insanity feature.
        /// </summary>  
        public UnitCommand NonLethalSelfHarm()
        {
            return GetFeature(Feature.ComfortableInsanity) ? new Commands.UnitSelfHarmNonLethal() : (UnitCommand)null;
        }

        /// <summary>
        /// Part of the confusion effect override.
        /// Used by <see cref="UnitConfusionControllerPatcher.DoNothing_MM"/> to replace "do nothing" with the Babblr ability
        /// when the unit has the Comfortable Insanity feature.
        /// </summary>  
        public UnitCommand Babble()
        {
            if (!GetFeature(Feature.ComfortableInsanity)) return null;

            var babble = Owner.Abilities.GetAbility(BPLookup.Ability("WarlockBabble", true)).Data;
            UnitEntityData target = null;

            // First pass: check for a target within 30 feet that is not already confused and also an enemy.
            foreach (var entity in EntityBoundsHelper.FindEntitiesInRange(Owner.Unit.Position, 30))
            {
                var unit = (UnitEntityData)entity;
                if (unit.IsEnemy(Owner.Unit) && !unit.State.HasCondition(UnitCondition.Confusion))
                {
                    target = unit;
                    break;
                }
            }

            // Second pass: include already confused enemies.
            if (target == null)
            {
                foreach (var entity in EntityBoundsHelper.FindEntitiesInRange(Owner.Unit.Position, 30))
                {
                    var unit = (UnitEntityData)entity;
                    if (unit.IsEnemy(Owner.Unit) && unit.State.HasCondition(UnitCondition.Confusion))
                    {
                        target = unit;
                        break;
                    }
                }
            }

            // Third pass: check for allies.
            if (target == null)
            {
                foreach (var entity in EntityBoundsHelper.FindEntitiesInRange(Owner.Unit.Position, 30))
                {
                    var unit = (UnitEntityData)entity;
                    if (unit.IsAlly(Owner.Unit))
                    {
                        target = unit;
                        break;
                    }
                }
            }

            return target != null ? new UnitUseAbility(babble, target) : (UnitCommand)null;
        }

        /// <summary>
        /// Part of the confusion effect override.
        /// Used by <see cref="UnitConfusionControllerPatcher.AttackNearest_MM"/>.
        /// When the confused unit has the Comfortable Insanity feature and rolls "Attack nearest" for action,
        /// apply a buff on the unit.
        /// </summary>  
        public void HandleConfusionAttackBonus()
        {
            if (!GetFeature(Feature.ComfortableInsanity)) return;
            GameHelper.ApplyBuff(Owner, BPLookup.Buff("WarlockInsanityABModifiers", true), new Rounds(1));
        }

        /// <summary>
        /// Part of the confusion effect override.
        /// Used by <see cref="UnitConfusionControllerPatcher.TickConfusion_MM"/>. Adjust rolls in two ways:
        /// 1) The confused unit has the Devotion feature of the Great Old Patron. In this case 1 is always substracted to the results of the roll.
        /// 2) The confused unit is under the Manifestation effect of the same patron. In this case the roll's result is always adjusted to 0.
        /// 
        /// If this method returns false the standard confusion controller is used. If it returns true, it is skipped.
        /// </summary>
        public bool TickConfusion()
        {
            if (!GetFeature(Feature.MinusOneToConfusionRolls)) return false;

            var unit = Owner.Unit;
            var part = unit.Ensure<UnitPartConfusion>();
            var flag = !unit.CombatState.HasCooldownForCommand(UnitCommand.CommandType.Standard);

            if (Game.Instance.TimeController.GameTime - part.RoundStartTime > UnitConfusionController.RoundDuration && flag)
            {
                var ruleRollDice = Rulebook.Trigger(new RuleRollDice(unit, new DiceFormula(1, DiceType.D100)));
                var num = unit.Descriptor.State.HasCondition(UnitCondition.AttackNearest) ? 100 : ruleRollDice.Result - 1;
                if (GetFeature(Feature.AlwaysRoll0WhenConfused)) num = 0;
                Main.DebugLog($"Confusion roll: {num}");
                part.State = num < 26
                    ? ConfusionState.ActNormally
                    : num < 51 ? ConfusionState.DoNothing : num < 76 ? ConfusionState.SelfHarm : ConfusionState.AttackNearest;
                if (part.State == ConfusionState.ActNormally)
                {
                    part.ReleaseControl();
                }
                else
                {
                    part.RetainControl();
                }
                EventBus.RaiseEvent(delegate (IConfusionRollResultHandler x)
                {
                    x.HandleConfusionRollResult(unit, part.State);
                }, true);
                part.RoundStartTime = Game.Instance.TimeController.GameTime;
                var cmd = part.Cmd;
                cmd?.Interrupt(true);
                part.Cmd = null;
            }
            if (part.Cmd == null && unit.Descriptor.State.CanAct && part.State != ConfusionState.ActNormally)
            {
                part.Cmd = flag
                    ? part.State switch
                    {
                        ConfusionState.DoNothing => UnitConfusionController.DoNothing(part),
                        ConfusionState.SelfHarm => UnitConfusionController.SelfHarm(part),
                        ConfusionState.AttackNearest => UnitConfusionController.AttackNearest(part),
                        _ => throw new ArgumentOutOfRangeException(),
                    }
                    : UnitConfusionController.DoNothing(part);
                if (part.Cmd != null)
                {
                    part.Owner.Unit.Commands.Run(part.Cmd);
                }
            }

            return true;
        }

        #endregion

        /// <summary>
        /// Used by <see cref="UnitPartSpellResistancePatcher.IsImmune_PostfixMM"/> to ignore immunity to the Mind-Affecting descriptor in
        /// the context of a unit using an ability to inflict confusion on itself.
        /// Requires the Enthralling Madness feature (CanAlwaysSelfConfuse).
        /// </summary>
        public void EnsureSelfConfuse(ref bool originalResult, MechanicsContext context)
        {
            if (!GetFeature(Feature.CanAlwaysSelfConfuse) || context.MaybeCaster != Owner) return;
            if (context.SpellDescriptor.HasFlag(SpellDescriptor.MindAffecting)) originalResult = false;
        }

        /// <summary>
        /// Used by <see cref="RuleSavingThrowPatcher.IsSuccessRoll_MM"/> to force-fail the saving throw
        /// the context of a unit using an ability to inflict confusion on itself.
        /// Requires the Enthralling Madness feature (CanAlwaysSelfConfuse).
        /// </summary>
        public void EnsureSelfConfuse(RuleSavingThrow rule, ref bool originalResult)
        {
            if (!GetFeature(Feature.CanAlwaysSelfConfuse) || rule.Reason.Context.MaybeCaster != Owner) return;
            if (rule.Reason.Context.SpellDescriptor.HasFlag(SpellDescriptor.MindAffecting)) originalResult = false;
        }

        /// <summary>
        /// Used by <see cref="ContextActionApplyBuffPatcher.CalculateDuration_MM"/> to double the duration of harmful buffs applied to
        /// the unit if they have the Infernal patron (via the DoubleDurationOfHarmfulBuffs feature).
        /// </summary>
        public void InfernalPactDoubleDuration(ref TimeSpan? duration, ContextActionApplyBuff action, MechanicsContext context)
        {
            if (!GetFeature(Feature.DoubleDurationOfHarmfulBuffs)) return;

            if (action.Target.Unit.IsEnemy(context.MaybeCaster)) duration.Value.Add((TimeSpan)duration);
        }

        /// <summary>
        /// Used bt <see cref="ContextActionApplyBuffPatcher.RunAction"/> to update add hexes to the list of tormenting effects.
        /// Hexes don't need to be used with the Tormenting Spellcasting ability active, just having the Pact feature
        /// of the Infernal patron is enough.
        /// If Infernal Incarnation is active, the duration of hexes is increased by 1 round.
        /// </summary>
        public void RegisterHex(ContextActionApplyBuff action)
        {
            if (action.AbilityContext is null || !GetFeature(Feature.DoubleDurationOfHarmfulBuffs)) return;
            if (action.AbilityContext.SpellDescriptor.HasFlag(SpellDescriptor.Hex))
            {
                var target = action.GetBuffTarget(ContextData<MechanicsContext.Data>.Current.Context);
                if (target.Buffs.GetBuff(action.Buff) is null) return;
                var duration = action.CalculateDuration(action.Context).Value.Seconds / 6;
                if (GetFeature(Feature.InfernalIncarnation))
                {
                    target.Buffs.GetBuff(action.Buff).IncreaseDuration(new Rounds(1).Seconds);
                    duration++;
                }
                target
                    .Ensure<UnitPartTormentingEffects>()
                    .AddEffect(action.AbilityContext.AbilityBlueprint, Owner.Unit, null, 1, duration, true);

                if (GetFeature(Feature.InfectiousGuilt)) GameHelper.ApplyBuff(target, InfectiousGuiltBuff);
                Main.DebugLog($"Infernal hex applied to {target.CharacterName}.");
            }
        }

        public bool IsTormentingSpell(AbilityData spell)
        {
            return Owner.Buffs.GetBuff(TormentingSpellcasting) != null && spell.IsInSpellList(TormentingList) && spell.SpellbookBlueprint == Spellbook;
        }

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            if (evt.Initiator != Owner.Unit) return;
            ProcessDaemonicHunger(evt);
        }

        /// <summary>
        /// If the unit cast a warlock spell while in possession of the Daemonic Hunger feature, it takes damage equal to the spell's level.
        /// Unlike Life Tap or the Flagellant's Mortified Casting feature, it is entirely possible to die from this.
        /// </summary>
        protected void ProcessDaemonicHunger(RuleCastSpell evt)
        {
            if (!GetFeature(Feature.DaemonicHunger) || evt.Spell.Spellbook?.Blueprint != Spellbook) return;
            GameHelper.DealDirectDamage(Owner, Owner, evt.Spell.SpellLevel);
        }

        /// <summary>
        /// If the initiator or target of an attack roll has the Abyssal patron's pact feature, they gain +2 to AB and +4 to
        /// critical confirmation.
        /// </summary>
        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if ((evt.Initiator == Owner.Unit && GetFeature(Feature.AbyssalFlanking)) || (evt.Target == Owner.Unit && GetFeature(Feature.AbyssalFlanking)))
            {
                if (!evt.Target.CombatState.IsFlanked) return;
                evt.AddModifier(2, BonusType.Flanking, Kingmaker.Enums.ModifierDescriptor.UntypedStackable);
                evt.CriticalConfirmationBonus += 4;
            }
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }
}
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using MysticalMayhem.Helpers;
using System;
using System.Collections.Generic;

namespace MysticalMayhem.Mechanics
{
    public class UnitPartWarlock : OldStyleUnitPart
    {
        public BlueprintCharacterClass Class => BPLookup.Class("WarlockClass", true);
        public BlueprintBuff Confusion => BPLookup.Buff("ConfusionBuff");

        private readonly Dictionary<Feature, CountableFlag> List = new();

        public enum Feature
        {
            LifeTap,
            MinusOneToConfusionRolls,
            ComfortableInsanity
        }

        public void AddFeature(Feature type)
        {
            CountableFlag feature = GetFeature(type);
            feature.Retain();
        }

        public void RemoveFeature(Feature type)
        {
            CountableFlag feature = GetFeature(type);
            feature.Release();
        }

        public void ClearFeature(Feature type)
        {
            CountableFlag feature = GetFeature(type);
            feature.ReleaseAll();
        }

        public CountableFlag GetFeature(Feature type)
        {
            CountableFlag feature;
            List.TryGetValue(type, out feature);
            if (feature == null)
            {
                feature = new CountableFlag();
                List[type] = feature;
            }
            return feature;
        }

        public bool HasProcessedLifeTap(AbilityData abilityData)
        {

            return false;
        }

        public bool SaveSpellSlot(AbilityData abilityData)
        {
            if (GetFeature(Feature.LifeTap) && abilityData.SpellLevel > 0 && Owner.HPLeft > abilityData.SpellLevel * 2 &&
                abilityData.SpellbookBlueprint.CharacterClass.AssetGuid == Class.AssetGuid)
            {
                GameHelper.ApplyBuff(Owner, BPLookup.Buff($"LifeTap{abilityData.SpellLevel * 2}", true));
                return true;
            }

            if (GetFeature(Feature.ComfortableInsanity) && Owner.HasFact(Confusion) && abilityData.SpellbookBlueprint.CharacterClass.AssetGuid == Class.AssetGuid)
            {
                return true;
            }
            return false;
        }

        public UnitCommand NonLethalSelfHarm()
        {
            if (GetFeature(Feature.ComfortableInsanity)) return new UnitSelfHarmNonLethal();
            return null;
        }

        public UnitCommand Babble()
        {
            if (!GetFeature(Feature.ComfortableInsanity)) return null;

            var babble = Owner.Abilities.GetAbility(BPLookup.Ability("WarlockBabble", true)).Data;
            UnitEntityData target = null;

            // First pass: check for a target within 30 feet that is not already confused and also an enemy.
            foreach (UnitGroupMemory.UnitInfo unitInfo in Owner.Unit.Memory.UnitsList)
            {
                var unit = unitInfo.Unit;
                if (unit.IsEnemy(Owner.Unit) && !unit.State.HasCondition(UnitCondition.Confusion) && Owner.Unit.SqrDistanceTo(unit) <= 30)
                {
                    target = unit;
                    break;
                }
            }
            // Second pass: include already confused enemies.
            if (target == null)
            {
                foreach (UnitGroupMemory.UnitInfo unitInfo in Owner.Unit.Memory.UnitsList)
                {
                    var unit = unitInfo.Unit;
                    if (unit.IsEnemy(Owner.Unit) && unit.State.HasCondition(UnitCondition.Confusion) && Owner.Unit.SqrDistanceTo(unit) <= 30)
                    {
                        target = unit;
                        break;
                    }
                }
            }
            // Third pass: check for allies.
            if (target == null)
            {
                foreach (UnitGroupMemory.UnitInfo unitInfo in Owner.Unit.Memory.UnitsList)
                {
                    var unit = unitInfo.Unit;
                    if (unit.IsAlly(Owner.Unit) && Owner.Unit.SqrDistanceTo(unit) <= 30)
                    {
                        target = unit;
                        break;
                    }
                }
            }
            if (target != null) return new UnitUseAbility(babble, target);
            return null;
        }

        public void HandleConfusionAttackBonus()
        {
            if (!GetFeature(Feature.ComfortableInsanity)) return;
            GameHelper.ApplyBuff(Owner, BPLookup.Buff("WarlockInsanityABModifiers", true), new Rounds(1));
        }

        public bool TickConfusion(UnitConfusionController controller)
        {
            if (!GetFeature(Feature.MinusOneToConfusionRolls)) return false;

            var unit = Owner.Unit;
            var part = unit.Ensure<UnitPartConfusion>();
            var flag = !unit.CombatState.HasCooldownForCommand(UnitCommand.CommandType.Standard);

            if (Game.Instance.TimeController.GameTime - part.RoundStartTime > UnitConfusionController.RoundDuration && flag)
            {
                RuleRollDice ruleRollDice = Rulebook.Trigger(new RuleRollDice(unit, new DiceFormula(1, DiceType.D100)));
                int num = unit.Descriptor.State.HasCondition(UnitCondition.AttackNearest) ? 100 : ruleRollDice.Result - 1;
                Main.DebugLog($"Confusion roll: {num}");
                if (num < 26)
                {
                    part.State = ConfusionState.ActNormally;
                }
                else if (num < 51)
                {
                    part.State = ConfusionState.DoNothing;
                }
                else if (num < 76)
                {
                    part.State = ConfusionState.SelfHarm;
                }
                else
                {
                    part.State = ConfusionState.AttackNearest;
                }
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
                UnitCommand cmd = part.Cmd;
                if (cmd != null)
                {
                    cmd.Interrupt(true);
                }
                part.Cmd = null;
            }
            if (part.Cmd == null && unit.Descriptor.State.CanAct && part.State != ConfusionState.ActNormally)
            {
                if (flag)
                {
                    switch (part.State)
                    {
                        case ConfusionState.DoNothing:
                            part.Cmd = UnitConfusionController.DoNothing(part);
                            break;
                        case ConfusionState.SelfHarm:
                            part.Cmd = UnitConfusionController.SelfHarm(part);
                            break;
                        case ConfusionState.AttackNearest:
                            part.Cmd = UnitConfusionController.AttackNearest(part);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    part.Cmd = UnitConfusionController.DoNothing(part);
                }
                if (part.Cmd != null)
                {
                    part.Owner.Unit.Commands.Run(part.Cmd);
                }
            }

            return true;
        }
    }
}
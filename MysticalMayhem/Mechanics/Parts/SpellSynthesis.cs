using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;

namespace MysticalMayhem.Mechanics.Parts
{
    internal static class SpellSynthesis
    {
        public class SpellSynthesisUnitPart : OldStyleUnitPart
        {
            public bool SpellSynthesisActivated = false;
            public bool HasCastArcaneSpell = false;
            public bool HasCastDivineSpell = false;
            public UnitCommand.CommandType CastingTime;

            public void Clear()
            {
                SpellSynthesisActivated = false;
                HasCastArcaneSpell = false;
                HasCastDivineSpell = false;
                RemoveSelf();
            }
        }

        [AllowedOn(typeof(BlueprintBuff), false)]
        [TypeId("e2755d66-d9c0-446b-aff0-e2327d91e177")]
        public class AddSpellSynthesisUnitPart : UnitBuffComponentDelegate
        {
            public override void OnTurnOn()
            {
                Owner.Ensure<SpellSynthesisUnitPart>().SpellSynthesisActivated = true;
            }

            public override void OnTurnOff()
            {
                Owner.Ensure<SpellSynthesisUnitPart>().Clear();
            }
        }

        [AllowedOn(typeof(BlueprintBuff), false)]
        [TypeId("fe14773b-352d-4179-aaee-eb8086d37626")]
        public class SpellSynthesisMonitor : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleCastSpell>,
            IRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleSpellResistanceCheck>, IRulebookHandler<RuleSpellResistanceCheck>,
            IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, ISubscriber,
            IInitiatorRulebookSubscriber
        {
            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
            }

            public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
            {
                if (Owner != evt.Initiator) return;
                if (evt.Context.SourceAbilityContext.Ability.Spellbook == Owner.Ensure<UnitPartMysticTheurge>().DivineSpellbook
                    || evt.Context.SourceAbilityContext.Ability.Spellbook == Owner.Ensure<UnitPartMysticTheurge>().ArcaneSpellbook)
                {
                    evt.AddSpellPenetration(2);
                }
            }

            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (Owner != evt.Initiator) return;
                if (evt.AbilityData.Spellbook == Owner.Ensure<UnitPartMysticTheurge>().DivineSpellbook
                    || evt.AbilityData.Spellbook == Owner.Ensure<UnitPartMysticTheurge>().ArcaneSpellbook)
                {
                    evt.AddBonusDC(2);
                }
            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            {
                if (Owner != evt.Initiator) return;
                if (evt.Spell?.Spellbook == Owner.Ensure<UnitPartMysticTheurge>().DivineSpellbook)
                {
                    Owner.Ensure<SpellSynthesisUnitPart>().HasCastDivineSpell = true;
                    Owner.Ensure<SpellSynthesisUnitPart>().CastingTime = evt.Spell.Blueprint.ActionType;
                }
                if (evt.Spell?.Spellbook == Owner.Ensure<UnitPartMysticTheurge>().ArcaneSpellbook)
                {
                    Owner.Ensure<SpellSynthesisUnitPart>().HasCastArcaneSpell = true;
                    Owner.Ensure<SpellSynthesisUnitPart>().CastingTime = evt.Spell.Blueprint.ActionType;
                }

                if (Owner.Descriptor.MTHasCastDivineSpell() && Owner.Descriptor.MTHasCastArcaneSpell())
                    Buff.Remove();
            }

            public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {
            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }
    }

    internal static class SpellSynthesisUnitEntityDataExtension
    {
        internal static bool SpellSynthesis(this UnitDescriptor _instance)
        {
            return _instance.Get<SpellSynthesis.SpellSynthesisUnitPart>()?.SpellSynthesisActivated == true;
        }

        internal static bool MTHasCastArcaneSpell(this UnitDescriptor _instance)
        {
            return _instance.Get<SpellSynthesis.SpellSynthesisUnitPart>()?.HasCastArcaneSpell == true;
        }

        internal static bool MTHasCastDivineSpell(this UnitDescriptor _instance)
        {
            return _instance.Get<SpellSynthesis.SpellSynthesisUnitPart>()?.HasCastDivineSpell == true;
        }
    }
}
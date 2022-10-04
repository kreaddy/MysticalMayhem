using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using MysticalMayhem.Helpers;

namespace MysticalMayhem.Mechanics
{
    [TypeId("fe0cb74b-36b9-498b-8317-b68685910df0")]
    [AllowedOn(typeof(BlueprintFeature), false)]
    public class AcadamaeGraduateFatigue : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            if (evt.Spell != null && !evt.Spell.IsSpontaneous && evt.Success && evt.Context.SpellSchool == SpellSchool.Conjuration &&
                evt.Context.SpellDescriptor.HasFlag(SpellDescriptor.Summoning))
            {
                var result = GameHelper.CheckStatResult(evt.Initiator, StatType.SaveFortitude, 15 + evt.Spell.SpellLevel);
                if (!result)
                {
                    evt.Initiator.AddBuff(BPLookup.Buff("Fatigued"), evt.Initiator, System.TimeSpan.FromMinutes(1));
                }
            }
        }

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
        }
    }
}
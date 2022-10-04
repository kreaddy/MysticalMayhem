using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using MysticalMayhem.Helpers;

namespace MysticalMayhem.Mechanics
{
    /// <summary>
    /// This component re-rolls certain types of D20 in exchange for the Pact Wizard's unique resource.
    /// </summary>
    [TypeId("c1b4d123-4f8e-4415-a2ad-fc9e6e0059b3")]
    [AllowedOn(typeof(BlueprintBuff), false)]
    public class PactWizardRerollLogic : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleRollD20>, IRulebookHandler<RuleRollD20>,
    ISubscriber, IInitiatorRulebookSubscriber
    {
        private bool CanApply(RuleRollD20 evt)
        {
            var prev = Rulebook.CurrentContext.PreviousEvent;
            if ((prev is RuleInitiativeRoll || prev is RuleSpellResistanceCheck || prev is RuleDispelMagic || prev is RuleSavingThrow ||
                prev is RuleCheckConcentration) && evt.Initiator == Owner)
            {
                return true;
            }
            return false;
        }

        public void OnEventAboutToTrigger(RuleRollD20 evt)
        {
            if (!CanApply(evt)) return;
            evt.AddReroll(1, true, Buff);
        }

        public void OnEventDidTrigger(RuleRollD20 evt)
        {
            if (!CanApply(evt)) return;
            var res = BPLookup.Resource("PactWizardResource", true);
            Owner.Resources.Spend(res, 1);
            if (Owner.Resources.GetResourceAmount(res) == 0)
            {
                Owner.RemoveFact(Buff);
            }
        }
    }
}
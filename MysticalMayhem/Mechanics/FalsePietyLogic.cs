using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using MysticalMayhem.Helpers;

namespace MysticalMayhem.Mechanics
{
    [TypeId("a60ff533-1095-4105-b1ae-306fef368d02")]
    [AllowedOn(typeof(BlueprintFeature), false)]
    public class FalsePietyLogic : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
            if (Owner == evt.Initiator && evt.Spell.UseMagicDeviceDC != null && evt.Spell.IsInSpellList(BPLookup.SpellList("ClericSpellList")))
            {
                Owner.AddFact(BPLookup.Buff("FalsePietyUMDBonus", true));
            }
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            Owner.RemoveFact(BPLookup.Buff("FalsePietyUMDBonus", true));
        }
    }
}
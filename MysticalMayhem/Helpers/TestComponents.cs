using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace MysticalMayhem.Helpers
{
    [TypeId("59f9736b-8f2c-4b90-8aa9-05b55ec1d200")]
    [AllowedOn(typeof(BlueprintFeature), false)]
    public class TestComponents : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleEnterStealth>, IRulebookHandler<RuleEnterStealth>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public void OnEventDidTrigger(RuleEnterStealth evt)
        {
        }

        public void OnEventAboutToTrigger(RuleEnterStealth evt)
        {
            GameHelper.GainExperience(50000, evt.Initiator);
            evt.Initiator.Progression.GainMythicExperience(1);
            evt.Initiator.Spellbooks.ForEach(s => s.Rest());
            evt.Initiator.Resources.FullRestoreAll();
        }
    }
}
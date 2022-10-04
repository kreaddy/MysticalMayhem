using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using MysticalMayhem.Helpers;
using System.Linq;
using static Kingmaker.Blueprints.Classes.FeatureGroup;

namespace MysticalMayhem.Mechanics
{
    /// <summary>
    /// This component reduces the combined level adjustment of metamagic by 1 for spells learnt by a patron or a curse.
    /// </summary>
    [AllowedOn(typeof(BlueprintFeature), false)]
    [TypeId("fb656052-fe08-4ad4-a8f9-b13854adf460")]
    public class PactWizardMetamagicCost : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleApplyMetamagic>,
        IRulebookHandler<RuleApplyMetamagic>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleApplyMetamagic evt)
        {
            if (evt.Spellbook == null || evt.Initiator != Owner) return;

            Owner.Progression.Features.Enumerable
                .Where(feature => feature.Blueprint.Groups.Contains(OracleCurse) || feature.Blueprint.Groups.Contains(WitchPatron))
                .ForEach(feature => CheckContainerAndReduceCost(feature, evt));
        }

        private void CheckContainerAndReduceCost(Feature feature, RuleApplyMetamagic evt)
        {
            var level = Owner.Progression.GetProgression(feature.Blueprint as BlueprintProgression).Level;
            var container = feature.GetComponent<SpellListContainer>();
            if (container != null && container.SpellList.ContainsKey(evt.Spell) && container.SpellList[evt.Spell] <= level && evt.AppliedMetamagics.Count > 0)
            {
                evt.ReduceCost(1);
                return;
            }
        }

        public void OnEventDidTrigger(RuleApplyMetamagic evt)
        {
        }
    }
}
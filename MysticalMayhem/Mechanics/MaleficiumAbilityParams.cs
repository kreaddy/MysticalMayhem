using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using MysticalMayhem.Helpers;

namespace MysticalMayhem.Mechanics
{
    /// <summary>
    /// This component adds DC and CL to spells with the Evil descriptor depending on feature rank.
    /// Rank 1: +1 DC. Rank 3: +2 DC. Rank 4: +2 CL.
    /// </summary>
    [TypeId("0894a6de-5880-4d05-bd98-91ddfd7720f3")]
    [AllowedOn(typeof(BlueprintFeature), false)]
    public class MaleficiumAbilityParams : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (Owner == evt.Initiator && evt.Spell != null)
            {
                var descriptor = evt.Spell.SpellDescriptor;
                descriptor = UnitPartChangeSpellElementalDamage.ReplaceSpellDescriptorIfCan(Owner, descriptor);
                if (!descriptor.HasAnyFlag(SpellDescriptor.Evil)) return;
                var rank = Owner.GetFact(BPLookup.Feature("WarlockMaleficium", true)).GetRank();
                evt.AddBonusDC(rank >= 3 ? 2: 1);
                if (rank == 4) { evt.AddBonusCasterLevel(2); }
            }
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }
}

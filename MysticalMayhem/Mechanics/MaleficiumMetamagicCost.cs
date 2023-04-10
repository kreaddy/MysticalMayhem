using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using MysticalMayhem.Helpers;

namespace MysticalMayhem.Mechanics
{
    /// <summary>
    /// This component reduces the metamagic cost of spells with the Evil descriptor by 1.
    /// Feature rank must be at least 2 and the cost cannot be lower than 0 (or -1 with Completely Normal Spell).
    /// </summary>
    [TypeId("c60e4a23-4015-4f2f-a78a-cecb66d907f3")]
    [AllowedOn(typeof(BlueprintFeature), false)]
    public class MaleficiumMetamagicCost : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleApplyMetamagic>, IRulebookHandler<RuleApplyMetamagic>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleApplyMetamagic evt)
        {
        }

        public void OnEventDidTrigger(RuleApplyMetamagic evt)
        {
            if (evt.Initiator != Owner || !evt.Spell.SpellDescriptor.HasFlag(SpellDescriptor.Evil)) return;

            var rank = Owner.GetFact(BPLookup.Feature("WarlockMaleficium", true)).GetRank();
            if (rank < 2) return;

            var min = evt.AppliedMetamagics.Contains(Metamagic.CompletelyNormal) ? evt.BaseLevel - 1 : evt.BaseLevel;
            if (evt.BaseLevel + evt.Result.SpellLevelCost > min) evt.Result.SpellLevelCost -= 1;
            Main.DebugLog($"Spell level after Maleficium: {evt.Result.SpellLevelCost}.");
        }
    }
}

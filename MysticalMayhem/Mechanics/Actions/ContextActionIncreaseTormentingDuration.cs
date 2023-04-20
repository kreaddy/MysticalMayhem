using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using MysticalMayhem.Mechanics.Parts;

namespace MysticalMayhem.Mechanics.Actions
{
    [TypeId("d2c7ff9b-7a3e-483e-953e-dad214ab7ea5")]
    [AllowedOn(typeof(BlueprintAbility))]
    public class ContextActionIncreaseTormentingDuration : ContextAction
    {
        public override string GetCaption()
        {
            return "Increase tormenting effects duration.";
        }

        public override void RunAction()
        {
            var caster = Context.MaybeCaster?.Descriptor;
            if (caster is null) return;

            var part = Target.Unit.Get<UnitPartTormentingEffects>();
            if (part is null) return;

            var effects = part.GetEffects(caster);
            while (effects.Count > 0) effects.Pop().Rounds++;

            foreach (var buff in Target.Unit.Buffs)
            {
                Main.DebugLog($"Checking buff {buff.Name} cast by {buff.MaybeContext.MaybeCaster}");
                if (buff.Context.SpellDescriptor.HasFlag(SpellDescriptor.Hex) && buff.Context.MaybeCaster?.Descriptor == caster)
                {
                    Main.DebugLog("Increasing tormenting hex's duration");
                    buff.IncreaseDuration(new Rounds(1).Seconds);
                }
            }
        }
    }
}

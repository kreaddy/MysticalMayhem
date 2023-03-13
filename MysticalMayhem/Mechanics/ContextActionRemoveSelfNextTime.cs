using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace MysticalMayhem.Mechanics
{
    [TypeId("8f870b13-dacd-40da-8c65-3ecba11f718f")]
    public class ContextActionRemoveSelfNextTime : ContextAction
    {
        private int _cdown = 1;

        public override string GetCaption()
        {
            return "Remove self but not after X";
        }

        public override void RunAction()
        {
            if (_cdown > 0) { _cdown -= 1; return; }
            Buff.Data data = ContextData<Buff.Data>.Current;
            if (data != null)
            {
                data.Buff.Remove();
                return;
            }
            AreaEffectContextData areaEffectContextData = ContextData<AreaEffectContextData>.Current;
            if (areaEffectContextData != null)
            {
                areaEffectContextData.Entity.ForceEnd();
                return;
            }
        }
    }
}

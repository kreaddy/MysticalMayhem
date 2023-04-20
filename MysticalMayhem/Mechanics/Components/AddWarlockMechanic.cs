using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic;
using MysticalMayhem.Mechanics.Parts;

namespace MysticalMayhem.Mechanics.Components
{
    [AllowMultipleComponents]
    [TypeId("00dbeecf-56d0-41d5-9410-d8a116d9b4e1")]
    public class AddWarlockMechanic : UnitFactComponentDelegate
    {
        public UnitPartWarlock.Feature Feature;

        public override void OnTurnOn()
        {
            Owner.Ensure<UnitPartWarlock>().AddFeature(Feature);
        }

        public override void OnTurnOff()
        {
            Owner.Ensure<UnitPartWarlock>().RemoveFeature(Feature);
        }
    }
}

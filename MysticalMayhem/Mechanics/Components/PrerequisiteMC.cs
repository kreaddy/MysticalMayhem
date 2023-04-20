using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Localization;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;

namespace MysticalMayhem.Mechanics.Components
{
    [TypeId("122610a7-509a-4673-abcd-c07c5d9ee5c8")]
    public class PrerequisiteMC : Prerequisite
    {
        public override bool CheckInternal(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
        {
            return unit.Unit.IsMainCharacter;
        }

        public override string GetUITextInternal(UnitDescriptor unit)
        {
            return LocalizationManager.CurrentPack.GetText("MM_MC_Only");
        }
    }
}
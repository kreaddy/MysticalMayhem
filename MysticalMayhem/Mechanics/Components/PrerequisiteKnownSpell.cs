using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Localization;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Utility;
using System.Linq;
using UnityEngine;

namespace MysticalMayhem.Mechanics.Components
{
    [TypeId("863e9bbf-b671-4dd0-b9d7-18bfc499ba42")]
    public class PrerequisiteKnownSpell : Prerequisite
    {
        public override bool CheckInternal([CanBeNull] FeatureSelectionState selectionState, [NotNull] UnitDescriptor unit, [CanBeNull] LevelUpState state)
        {
            return unit.Spellbooks
                .Where(b => b.IsKnown(_spell.Get()))
                .Empty() == false;
        }

        public override string GetUITextInternal(UnitDescriptor unit)
        {
            return LocalizationManager.CurrentPack.GetText("MM_PrerequisiteSpell") + $" {_spell.Get().Name}";
        }

        [NotNull]
        [SerializeField]
        private BlueprintAbilityReference _spell;

    }
}

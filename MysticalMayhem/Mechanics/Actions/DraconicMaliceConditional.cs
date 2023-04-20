using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using MysticalMayhem.Helpers;
using UnityEngine;

namespace MysticalMayhem.Mechanics.Actions
{
    [TypeId("9a81667c-0099-45b2-a8f9-ed9e8a8dab44")]
    public class DraconicMaliceConditional : GameAction
    {
        public override string GetDescription()
        {
            return "";
        }

        public override void RunAction()
        {
            if (_isEnemy.CheckCondition() && !_noUndead.CheckCondition() && !_noConstruct.CheckCondition())
                SubList.Run();
        }

        public override string GetCaption()
        {
            return "";
        }

        [SerializeField]
        private ActionList SubList;

        private ContextConditionHasFact _noUndead;
        private ContextConditionHasFact _noConstruct;
        private ContextConditionIsEnemy _isEnemy;

        public DraconicMaliceConditional()
        {
            _noUndead = new() { m_Fact = BPLookup.Feature("TypeUndead").ToReference<BlueprintUnitFactReference>() };
            _noConstruct = new() { m_Fact = BPLookup.Feature("TypeConstruct").ToReference<BlueprintUnitFactReference>() };
            _isEnemy = new();
        }
    }
}

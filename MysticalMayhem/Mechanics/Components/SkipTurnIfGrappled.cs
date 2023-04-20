using Kingmaker;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using TurnBased.Controllers;

namespace MysticalMayhem.Mechanics.Components
{
    /// <summary>
    /// If this component's owner is grappled at the start of its turn, then it skips its turn entirely.
    /// </summary>
    [TypeId("e93105ce-72f8-4b58-b84e-5f7fadb3b288")]
    public class SkipTurnIfGrappled : UnitFactComponentDelegate, IUnitNewCombatRoundHandler
    {
        public void HandleNewCombatRound(UnitEntityData unit)
        {
            if (Owner != unit || Owner.Get<UnitPartGrappleTarget>()?.Initiator == null) return;
            Owner.Commands.Run(new UnitDoNothing());
            if (CombatController.IsInTurnBasedCombat())
            {
                Game.Instance.TurnBasedCombatController.CurrentTurn.ForceToEnd(true);
            }
        }
    }
}

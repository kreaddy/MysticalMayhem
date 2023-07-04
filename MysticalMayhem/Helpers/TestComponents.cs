﻿using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using UnityEngine;

namespace MysticalMayhem.Helpers
{
    [TypeId("59f9736b-8f2c-4b90-8aa9-05b55ec1d200")]
    [AllowedOn(typeof(BlueprintFeature), false)]
    public class TestComponents : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleEnterStealth>, IRulebookHandler<RuleEnterStealth>,
        ISubscriber, IInitiatorRulebookSubscriber
    {
        public void OnEventDidTrigger(RuleEnterStealth evt)
        {
        }

        public void OnEventAboutToTrigger(RuleEnterStealth evt)
        {
            GameHelper.GainExperience(50000, evt.Initiator);
            evt.Initiator.Progression.GainMythicExperience(1);
            evt.Initiator.Spellbooks.ForEach(s => s.Rest());
            evt.Initiator.Resources.FullRestoreAll();
            var unit = BPLookup.Unit("CR1AirElemental");
            var pos = Game.Instance.ClickEventsController.WorldPosition;
            var offset = 5f * Random.insideUnitSphere;
            var realPos = new Vector3(pos.x + offset.x, pos.y, pos.z + offset.z);
            //Game.Instance.Player.Inventory.Add(ResourcesLibrary.TryGetBlueprint<BlueprintItem>("e536dc0b5dd89e14db611b0f03272a67"));
            //Game.Instance.EntityCreator.SpawnUnit(unit, realPos, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
            //Game.Instance.EntityCreator.SpawnUnit(unit, realPos, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
        }
    }
}
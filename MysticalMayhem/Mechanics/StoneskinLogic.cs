using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Components;
using MysticalMayhem.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MysticalMayhem.Mechanics
{
    [TypeId("5f69eec8-3d7f-4223-9576-2a77182f7d0b")]
    internal class StoneskinLogic : UnitBuffComponentDelegate, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>,
        ISubscriber, ITargetRulebookSubscriber
    {
        protected int Skins;

        public override void OnActivate()
        {
            Skins = Math.Max(Math.Min(Context.Params.CasterLevel / 2, 10), 1);
            base.OnActivate();
        }

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        { }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
            var dmgToRemove = new List<BaseDamage>();
            dmgToRemove.Capacity = Skins;

            evt.DamageBundle
                .Where(dmg => dmg.Type == DamageType.Physical)
                .ForEach(dmg => dmgToRemove.Add(dmg));

            dmgToRemove.TrimExcess();
            Skins -= dmgToRemove.Count();

            dmgToRemove.ForEach(dmg => dmg.SetMaximum(0));

            if (Skins <= 0) Buff.Remove();
        }
    }
}
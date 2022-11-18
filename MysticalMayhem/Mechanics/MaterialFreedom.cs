using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic;
using System.Collections.Generic;
using System.Linq;

namespace MysticalMayhem.Mechanics
{
    /// <summary>
    /// Container class holding the relevant mechanics subclasses.
    /// </summary>
    internal static class MaterialFreedom
    {
        /// <summary>
        /// Stores which material components are ignored and which Fact has this effect. Used by MaterialFreedomUnitPart.
        /// </summary>
        private class IgnoredComponent
        {
            private BlueprintItemReference item;
            private EntityFact sourceFact;

            public BlueprintItemReference Item { get => item; set => item = value; }
            public EntityFact SourceFact { get => sourceFact; set => sourceFact = value; }
        }

        /// <summary>
        /// Monitors the state of the mechanic as the character gains or lose Facts.
        /// </summary>
        public class MaterialFreedomUnitPart : OldStyleUnitPart
        {
            private readonly List<IgnoredComponent> ItemList = new ();

            public void AddEntry(BlueprintItemReference item, EntityFact source_fact)
            {
                ItemList.Add(new IgnoredComponent { Item = item, SourceFact = source_fact });
            }

            public void RemoveEntry(EntityFact source_fact)
            {
                ItemList.RemoveAll(e => e.SourceFact == source_fact);
                if (!ItemList.Any())
                {
                    RemoveSelf();
                }
            }

            public bool HasItem(BlueprintItemReference item)
            {
                var entry = ItemList.First(e => e.Item.Guid == item.Guid);
                return entry != null;
            }
        }

        /// <summary>
        /// Serializable component attached to a BlueprintFeature. Assigns the UnitPart to a character taking the feature.
        /// </summary>
        [AllowedOn(typeof(BlueprintFeature), false)]
        [AllowMultipleComponents]
        [TypeId("ae0e613d-4721-4ed6-825e-1b541c27b5a9")]
        public class IgnoreSpellMaterialComponent : UnitFactComponentDelegate
        {
            public override void OnTurnOn()
            {
                Owner.Ensure<MaterialFreedomUnitPart>().AddEntry(Item, Fact);
            }

            public override void OnTurnOff()
            {
                Owner.Ensure<MaterialFreedomUnitPart>().RemoveEntry(Fact);
            }

            public BlueprintItemReference Item;
        }
    }
}
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System.Collections.Generic;

namespace MysticalMayhem.Mechanics.Components
{
    /// <summary>
    /// This component stores the data of AddKnownSpell components into a dictionary.
    /// Used by the Pact Wizard's metamagic cost reducing feature for the sake of simplicity.
    /// </summary>
    [TypeId("a03cdabb-708a-4565-8faa-6a1d94954d82")]
    [AllowedOn(typeof(BlueprintProgression), false)]
    public class SpellListContainer : BlueprintComponent
    {
        private Dictionary<BlueprintAbility, int> _spellList = new();

        public Dictionary<BlueprintAbility, int> SpellList { get => _spellList; set => _spellList = value; }
    }
}
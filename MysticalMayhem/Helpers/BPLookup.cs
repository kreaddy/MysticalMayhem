using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MysticalMayhem.Helpers
{
    internal static class BPLookup
    {
        private static Dictionary<string, string> _repo;
        private static Dictionary<string, string> _modRepo = new();

        public static BlueprintAbility Ability(string id, bool mod = false) => GetBP<BlueprintAbility>(id, mod);

        public static BlueprintActivatableAbility Activatable(string id, bool mod = false) => GetBP<BlueprintActivatableAbility>(id, mod);
        public static BlueprintAbilityAreaEffect AoE(string id, bool mod = false) => GetBP<BlueprintAbilityAreaEffect>(id, mod);

        public static BlueprintArchetype Archetype(string id, bool mod = false) => GetBP<BlueprintArchetype>(id, mod);

        public static BlueprintBuff Buff(string id, bool mod = false) => GetBP<BlueprintBuff>(id, mod);

        public static BlueprintCharacterClass Class(string id, bool mod = false) => GetBP<BlueprintCharacterClass>(id, mod);

        public static BlueprintFeature Feature(string id, bool mod = false) => GetBP<BlueprintFeature>(id, mod);
        public static BlueprintParametrizedFeature ParametrizedFeature(string id, bool mod = false) => GetBP<BlueprintParametrizedFeature>(id, mod);

        public static BlueprintProgression Progression(string id, bool mod = false) => GetBP<BlueprintProgression>(id, mod);

        public static BlueprintAbilityResource Resource(string id, bool mod = false) => GetBP<BlueprintAbilityResource>(id, mod);

        public static BlueprintFeatureSelection Selection(string id, bool mod = false) => GetBP<BlueprintFeatureSelection>(id, mod);

        public static BlueprintSpellbook Spellbook(string id, bool mod = false) => GetBP<BlueprintSpellbook>(id, mod);

        public static BlueprintSpellList SpellList(string id, bool mod = false) => GetBP<BlueprintSpellList>(id, mod);

        public static BlueprintStatProgression StatProgression(string id, bool mod = false) => GetBP<BlueprintStatProgression>(id, mod);

        public static T GetBP<T>(string id, bool mod = false) where T : BlueprintScriptableObject
        {
            if (_repo == null) { BuildRepo(); }
            return ResourcesLibrary.TryGetBlueprint<T>(BlueprintGuid.Parse(mod ? _modRepo[id] : _repo[id]));
        }

        public static string GetGuid(string id)
        {
            if (_repo == null) { BuildRepo(); }
            return _repo[id];
        }

        private static void BuildRepo()
        {
            var serializer = new JsonSerializer();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MysticalMayhem.BPRepo.json"))
            using (StreamReader streamReader = new StreamReader(stream))
            using (JsonTextReader reader = new JsonTextReader(streamReader))
            {
                _repo = serializer.Deserialize<Dictionary<string, string>>(reader);
            }
        }

        public static void AddModBP(string name, string guid)
        {
            Main.DebugLog($"Added blueprint: {name} with guid: {guid}");
            _modRepo.Add(name, guid);
        }

        public static void ClearAllModData()
        {
            _modRepo.Clear();
        }
    }
}
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace MysticalMayhem.Helpers
{
    public class BlueprintContainer
    {
        private string assetGuid;
        private SimpleBlueprint blueprint;
        private bool homebrew = false;
        private string[] patchList = new string[0];

        [JsonProperty]
        public string AssetGuid { get => assetGuid; set => assetGuid = value; }

        [JsonProperty]
        public SimpleBlueprint Blueprint { get => blueprint; set => blueprint = value; }

        [JsonProperty]
        public bool Homebrew { get => homebrew; set => homebrew = value; }

        [JsonProperty]
        public string[] PatchList { get => patchList; set => patchList = value; }

        public static void UIGroupPatchGeneric(string[] args)
        {
            var cclass = BPLookup.Class(args[0]);
            var uiGroup = new UIGroup() { m_Features = new() };

            args.Pop().ForEach(feature => uiGroup.m_Features.Add(new()
            {
                guid = BPLookup.GetGuid(feature),
                deserializedGuid = BlueprintGuid.Parse(BPLookup.GetGuid(feature))
            }));

            cclass.Progression.UIGroups = cclass.Progression.UIGroups.Push(uiGroup);
        }

        public void AddArchetypeTo(string[] args)
        {
            var cclass = BPLookup.Class(args[0]);
            cclass.m_Archetypes = cclass.m_Archetypes.Push(Blueprint.ToReference<BlueprintArchetypeReference>());
        }

        public void AddAsArcaneDiscovery(string[] _)
        {
            if (UnityModManagerNet.UnityModManager.FindMod("TabletopTweaks-Base") != null)
            {
                var selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>(BPLookup.GetGuid("TTTArcaneDiscoverySelection"));
                if (selection == null) return;
                selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());
            }
            else
            {
                AddAsWizardFeat(_);
            }
        }

        public void AddAsDeity(string[] _)
        {
            var selection = BPLookup.Selection("DeitySelection");
            selection.m_AllFeatures = selection.m_AllFeatures
                .Push(Blueprint.ToReference<BlueprintFeatureReference>())
                .OrderBy(deity => deity.Get().NameSafe())
                .ToArray();
        }

        public void AddAsFeat(string[] _)
        {
            var selection = BPLookup.Selection("FeatSelection");
            selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());

            selection = BPLookup.Selection("MythicExtraFeatSelection");
            selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());
        }

        public void AddAsHexcrafterArcana(string[] _)
        {
            var selection = BPLookup.Selection("HexcrafterArcanaSelection");
            selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());

            // If TTT is present, make the arcana available to the relevant Extra Magus Arcana selection.
            if (UnityModManagerNet.UnityModManager.FindMod("TabletopTweaks-Base") != null)
            {
                selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("dcd025d2-b492-4f76-9321-55adcf8057f0");
                if (selection == null) return;
                selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());
            }
        }

        public void AddAsMagusArcana(string[] _)
        {
            var selection = BPLookup.Selection("MagusArcanaSelection");
            selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());

            selection = BPLookup.Selection("EldritchScionArcanaSelection");
            selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());

            selection = BPLookup.Selection("HexcrafterArcanaSelection");
            selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());

            // If TTT is present, make the arcana available to the Extra Magus Arcana selections.
            if (UnityModManagerNet.UnityModManager.FindMod("TabletopTweaks-Base") != null)
            {
                selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("9b394c91-008d-4db3-8af7-74bd340672dc");
                if (selection == null) return;
                selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());

                selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("1e7dba2f-2790-49c9-8e91-8aa4775ef72b");
                selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());

                selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("dcd025d2-b492-4f76-9321-55adcf8057f0");
                selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());
            }
        }

        public void AddAsMythicAbility(string[] _)
        {
            var selection = BPLookup.Selection("MythicAbilitySelection");
            selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());

            selection = BPLookup.Selection("ExtraMythicAbilitySelection");
            selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());
        }

        public void AddAsMythicFeat(string[] _)
        {
            var selection = BPLookup.Selection("MythicFeatSelection");
            selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());
        }

        public void AddAsWizardFeat(string[] _)
        {
            AddAsFeat(_);
            var selection = BPLookup.Selection("WizardFeatSelection");
            selection.m_AllFeatures = selection.m_AllFeatures.Push(Blueprint.ToReference<BlueprintFeatureReference>());
        }

        public void AddIcon(string[] args)
        {
            (Blueprint as BlueprintUnitFact).m_Icon = ResourceHandler.Sprites[args[0]];
        }

        public void AddToProgression(string[] args)
        {
            var progression = BPLookup.Progression(args[0]);
            var levelEntries = progression.LevelEntries.Where(le => le.Level == int.Parse(args[1]));
            if (levelEntries.Count() == 0)
            {
                LevelEntry levelEntry = new()
                {
                    Level = int.Parse(args[1]),
                    m_Features = new() { Blueprint.ToReference<BlueprintFeatureBaseReference>() }
                };
                progression.LevelEntries = progression.LevelEntries.Push(levelEntry);
            }
            else
            {
                levelEntries.First().m_Features.Add(Blueprint.ToReference<BlueprintFeatureBaseReference>());
            }
        }

        public void CopyIconFromAbility(string[] args)
        {
            (Blueprint as BlueprintUnitFact).m_Icon = BPLookup.Ability(args[0]).m_Icon;
        }

        public void CopySelectionChoices(string[] args)
        {
            (Blueprint as BlueprintFeatureSelection).m_Features = BPLookup.Selection(args[0]).m_Features;
            (Blueprint as BlueprintFeatureSelection).m_AllFeatures = BPLookup.Selection(args[0]).m_AllFeatures;
        }

        public void FinalizeBP()
        {
            FixPrefabLinks();
            FixComponentNames();

            if (Homebrew && Settings.IsEnabled("mm.no.hb")) return;

            ApplyPatches();
        }

        public void RegisterClass(string[] _)
        {
            var root = BPLookup.GetBP<BlueprintRoot>("BlueprintRoot").Progression;
            var classList = root.m_CharacterClasses.Where(c => !c.Get().PrestigeClass).ToList();
            var prestigeList = root.m_CharacterClasses.Where(c => c.Get().PrestigeClass).ToList();
            if ((Blueprint as BlueprintCharacterClass).PrestigeClass)
            {
                prestigeList.Add(Blueprint.ToReference<BlueprintCharacterClassReference>());
                prestigeList = prestigeList.OrderBy(c => c.Get().NameSafe()).ToList();
            }
            else
            {
                classList.Add(Blueprint.ToReference<BlueprintCharacterClassReference>());
                classList = classList.OrderBy(c => c.Get().NameSafe()).ToList();
            }
            root.m_CharacterClasses = classList.Concat(prestigeList).ToArray();
        }

        public void RegisterSpellList(string[] _)
        {
            (Blueprint as BlueprintSpellList)
                .SpellsByLevel
                .ForEach(list => list.m_Spells
                .ForEach(spell => spell.Get().ComponentsArray = spell.Get().ComponentsArray
                .Push(new SpellListComponent()
                {
                    SpellLevel = list.SpellLevel,
                    m_SpellList = (Blueprint as BlueprintSpellList).ToReference<BlueprintSpellListReference>()
                })));
        }

        public void SKPPatch(string[] args)
        {
            string[] patchParts = args[0].Split(':');
            if (Type.GetType("MysticalMayhem.SKP.Starion").GetMethod(patchParts[0]) != null)
            {
                MethodInfo method = Type.GetType("MysticalMayhem.SKP.Starion").GetMethod(patchParts[0]);
                method.Invoke(this, new object[] { });
            }
        }

        public void SpecialPatch(string[] args)
        {
            string[] patchParts = args[0].Split(':');
            if (typeof(PostPatches).GetMethod(patchParts[0]) != null)
            {
                MethodInfo method = typeof(PostPatches).GetMethod(patchParts[0]);
                method.Invoke(this, new object[] { });
            }
        }

        private void ApplyPatches()
        {
            if (PatchList == null) return;
            foreach (string patchType in PatchList)
            {
                string[] patchParts = patchType.Split(':');
                if (GetType().GetMethod(patchParts[0]) != null)
                {
                    MethodInfo method = GetType().GetMethod(patchParts[0]);
                    method.Invoke(this, new object[] { patchParts.Pop() });
                }
            }
        }

        private void FixComponentNames()
        {
            if (Blueprint is BlueprintScriptableObject == false) return;

            int i = 0;
            foreach (BlueprintComponent component in (Blueprint as BlueprintScriptableObject).Components)
            {
                component.name = $"${component.GetType()}#{i}";
                i++;
            }
            Blueprint.OnEnable();
        }

        private void FixPrefabLinks()
        {
            if (Blueprint.GetType() != typeof(BlueprintBuff)) return;

            var buff = Blueprint as BlueprintBuff;
            if (buff.FxOnStart == null) buff.FxOnStart = new();
            if (buff.FxOnRemove == null) buff.FxOnRemove = new();
        }
    }
}
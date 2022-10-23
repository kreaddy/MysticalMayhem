using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using MysticalMayhem.Helpers;
using MysticalMayhem.Mechanics;
using System.Linq;

namespace MysticalMayhem
{
    public static class PostPatches
    {
        public static readonly string[] PactWizardLegalSpellbooks = new string[]
        {
            "5a38c9ac8607890409fcb8f6342da6f4", "5785f40e7b1bfc94ea078e7156aa9711", "97cd3941ce333ce46ae09436287ed699",
            "74b87962a97d56c4583979216631eb4c", "05b105ddee654db4fb1547ba48ffa160", "9e4b96d7b02f8c8498964aeee6eaef9b",
            "cbc30bcc7b8adec48a53a6540f5596ae", "58b15cc36ceda8942a7a29aafa755452", "d09794fb6f93e4a40929a965b434070d"
        };

        #region Patches

        // Patch called by the PactWizardArchetype blueprint, mostly to add spells from patrons and curses to its wizard spellbook.
        public static void PactWizard()
        {
            var wizard = BPLookup.Class("WizardClass");
            var arch = BPLookup.Archetype("PactWizard", true);

            var patronList = BPLookup.Selection("WitchPatronSelection").m_AllFeatures;

            // Patch the patrons to progress with the wizard class.
            foreach (BlueprintFeatureReference reference in patronList)
            {
                var patron = (BlueprintProgression)reference.Get();
                patron.m_Classes = patron.m_Classes.Push(
                    new BlueprintProgression.ClassWithLevel
                    {
                        m_Class = wizard.ToReference<BlueprintCharacterClassReference>(),
                        AdditionalLevel = 0
                    }
                );
                patron.m_Archetypes = patron.m_Archetypes.Push(
                    new BlueprintProgression.ArchetypeWithLevel
                    {
                        m_Archetype = arch.ToReference<BlueprintArchetypeReference>(),
                        AdditionalLevel = 0
                    }
                );

                // Patch the patron spells so they're added to the wizard's spellbook.
                var container = new SpellListContainer()
                {
                    name = $"PactWizardSLContainer_{patron.AssetGuid}"
                };

                foreach (var entry in patron.LevelEntries)
                {
                    var feature = (BlueprintFeature)entry.Features.First();
                    var component = entry.Features.Select(f => f.GetComponent<AddKnownSpell>()).First();
                    var newComponent = new AddKnownSpell()
                    {
                        name = $"PactWizardAdd{component.Spell.AssetGuid}",
                        m_CharacterClass = wizard.ToReference<BlueprintCharacterClassReference>(),
                        m_Archetype = arch.ToReference<BlueprintArchetypeReference>(),
                        m_Spell = component.m_Spell,
                        SpellLevel = component.SpellLevel
                    };
                    feature.ComponentsArray = feature.ComponentsArray.Push(newComponent);
                    if (!container.SpellList.ContainsKey(component.Spell))
                    {
                        container.SpellList.Add(component.Spell, entry.Level);
                    }
                }

                patron.ComponentsArray = patron.ComponentsArray.Push(container);
            }

            var curseList = BPLookup.Selection("OracleCurseSelection").m_AllFeatures;

            // Patch the curses giving spells so they add them to the spellbook.
            foreach (var reference in curseList)
            {
                var curse = (BlueprintProgression)reference.Get();

                var container = new SpellListContainer()
                {
                    name = $"PactWizardSLContainer_{curse.AssetGuid}"
                };

                foreach (var entry in curse.LevelEntries)
                {
                    var feature = (BlueprintFeature)entry.Features.First();
                    var component = entry.Features.Select(f => f.GetComponent<AddKnownSpell>()).First();
                    if (component != null)
                    {
                        var newComponent = new AddKnownSpell()
                        {
                            name = $"PactWizardAdd{component.Spell.AssetGuid}",
                            m_CharacterClass = wizard.ToReference<BlueprintCharacterClassReference>(),
                            m_Archetype = arch.ToReference<BlueprintArchetypeReference>(),
                            m_Spell = component.m_Spell,
                            SpellLevel = component.SpellLevel
                        };
                        feature.ComponentsArray = feature.ComponentsArray.Push(newComponent);
                        if (!container.SpellList.ContainsKey(component.Spell))
                        {
                            container.SpellList.Add(component.Spell, entry.Level);
                        }
                    }
                }

                curse.ComponentsArray = curse.ComponentsArray.Push(container);
            }

            wizard.m_Archetypes = wizard.m_Archetypes.Push(arch.ToReference<BlueprintArchetypeReference>());
        }

        // Patch called by the Razmir blueprint to bar deity-dependant classes (Paladin, Cleric, Warpriest) to be picked by a Razmir worshipper.
        public static void RazmirDeity()
        {
            var classes = new BlueprintCharacterClass[]
            {
                BPLookup.Class("ClericClass"), BPLookup.Class("WarpriestClass"), BPLookup.Class("PaladinClass"), BPLookup.Class("InquisitorClass")
            };

            foreach (var cl in classes)
            {
                var comp = new PrerequisiteNoFeature()
                {
                    name = $"noRazmir{cl.AssetGuid}",
                    Group = Prerequisite.GroupType.All,
                    HideInUI = true,
                    m_Feature = new()
                    {
                        guid = BPLookup.GetGuid("Razmir"),
                        deserializedGuid = BlueprintGuid.Parse(BPLookup.GetGuid("Razmir"))
                    }
                };
                cl.ComponentsArray = cl.ComponentsArray.Push(comp);
            }
        }

        // Patch called by the RazmiranPriestArchetype blueprint, mostly to disable features in bloodlines.
        public static void RazmiranPriest()
        {
            var selection = BPLookup.Selection("SorcererBloodlineSelection");
            foreach (var bloodline in selection.AllFeatures)
            {
                var entry = (bloodline as BlueprintProgression).GetLevelEntry(3);
                RazmiranPriestAddSpellKnownPatch(entry);
                entry = (bloodline as BlueprintProgression).GetLevelEntry(5);
                RazmiranPriestAddSpellKnownPatch(entry);
                entry = (bloodline as BlueprintProgression).GetLevelEntry(9);
                entry.m_Features
                    .Where(f => f.Get().GetComponent<AddKnownSpell>() == null)
                    .ForEach(f => BanArchetypeInFeature(f.Get(), "SorcererClass", "RazmiranPriestArchetype"));
            }

            // Now we forbid all gods but Razmir when the player has levels in the class.
            BPLookup.Selection("DeitySelection").m_AllFeatures
            .Where(f => f.guid != BPLookup.GetGuid("Razmir"))
            .ForEach(f => BanArchetypeInFeature(f.Get(), "SorcererClass", "RazmiranPriestArchetype"));
        }

        // Patch called by the BlueprintCache hook, perform blueprint edits to the Stoneskin buff and ability blueprints.
        public static void ApplyStoneskinChanges()
        {
            var buffs = new BlueprintBuff[]
            {
                BPLookup.Buff("StoneskinBuff"), BPLookup.Buff("StoneskinMassBuff")
            };

            buffs.ForEach(buff =>
                buff.ComponentsArray = new BlueprintComponent[] { new StoneskinLogic() { name = $"{buff.AssetGuid}mmstoneskinlogic" } });

            var spell = BPLookup.Ability("Stoneskin");
            spell.m_Description.m_Key = "MM_Stoneskin_Desc";
            spell.LocalizedDuration.m_Key = "MM_12hours";
            spell.MaterialComponent.Count = 10;
            spell.CanTargetFriends = false;
            spell.Range = AbilityRange.Personal;

            spell = BPLookup.Ability("StoneskinMass");
            spell.m_Description.m_Key = "MM_StoneskinMass_Desc";
            spell.MaterialComponent.Count = 20;
        }

        #endregion Patches

        #region Helpers

        private static void RazmiranPriestAddSpellKnownPatch(LevelEntry entry)
        {
            entry.m_Features
                .Where(f => f.Get().GetComponent<AddKnownSpell>() != null)
                .ForEach(f => BanArchetypeInFeature(f.Get(), "SorcererClass", "RazmiranPriestArchetype"));
        }

        private static void BanArchetypeInFeature(BlueprintFeatureBase feature, string className, string archName)
        {
            feature.ComponentsArray = feature.ComponentsArray.Push(
                new PrerequisiteNoArchetype()
                {
                    CheckInProgression = true,
                    m_CharacterClass = BPLookup.Class(className).ToReference<BlueprintCharacterClassReference>(),
                    m_Archetype = new()
                    {
                        guid = BPLookup.GetGuid(archName),
                        deserializedGuid = BlueprintGuid.Parse(BPLookup.GetGuid(archName))
                    },
                    Group = Prerequisite.GroupType.All,
                    name = $"RazmirPrereq{feature.ComponentsArray.Length}"
                });
        }

        #endregion Helpers
    }
}
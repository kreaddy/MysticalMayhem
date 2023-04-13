using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using MysticalMayhem.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityModManagerNet;

namespace MysticalMayhem
{
    public static class ModInterop
    {
        private static bool IsCOPlusEnabled() => IsModEnabled("CharacterOptionsPlus");
        private static bool IsECEnabled() => IsModEnabled("ExpandedContent");
        private static bool IsFirebirdEnabled() => IsModEnabled("TomeOfTheFirebird");
        private static bool IsTTTBaseEnabled() => IsModEnabled("TabletopTweaks-Base");
        private static bool IsModEnabled(string name) => UnityModManager.modEntries.Where(mod => mod.Info.Id.Equals(name) && mod.Enabled && !mod.ErrorOnLoading).Any();

        public static void ApplyWarlockModPatches()
        {
            #region Spelllist Patches
            var spellList = BPLookup.SpellList("WarlockSpellList", true);
            Dictionary<string, int> spellData;

            try
            {
                spellData = new()
                {
                    { "0da2046b4517427bb9b2e304ea6342bf", 2 },
                    { "8a76293f5ab8485da95ef6293a11358c", 3 }
                };
                BlueprintContainer.AddToSpellList(spellList, spellData);
            }
            catch
            {
                Main.Log("Failed to inject DLC4 spells.");
            }

            if (IsCOPlusEnabled()) 
            {
                spellData = new()
                {
                    { "4445e61d-3dff-4985-9eb5-37169a0e85bc", 1 },
                    { "a3a7e60e-7866-46c4-99b8-ad685e625c06", 1 },
                    { "6177af1b-a096-4f58-a0a0-c02778e95483", 1 },
                    { "ba47baf2-982a-4c4c-82fc-af65dab915af", 2 },
                    { "103e53fe-89de-48a1-b85e-9efcb1de4860", 3 },
                    { "7af385de-a758-4984-8ab9-14b484b509f3", 4 },
                    { "aaed2bc8-7c24-4737-83f6-df4c520888ee", 6 },
                    { "e12e8537-7bfc-415e-9d50-87456dd0a3a1", 8 }
                };
                BlueprintContainer.AddToSpellList(spellList, spellData);
            }
            if (IsECEnabled()) 
            {
                spellData = new()
                {
                    { "e28f4633-c0a2-425d-8895-adf20cb22f8f", 3 },
                    { "e023af1a-f9c1-4754-9a8e-7bd246967861", 3 },
                    { "80189142-f7c6-40f3-9195-defdc9777b27", 4 },
                    { "ff31ae1a-be3c-418d-b784-2dcc76eca7ee", 7 },
                    { "a8be30dd-f370-42d5-b56f-faa8eae976d6", 7 },
                    { "a8be30dd-f370-42d5-b56f-faa8eae976d6", 9 }
                };
                BlueprintContainer.AddToSpellList(spellList, spellData);
            }
            if (IsFirebirdEnabled()) 
            {
                spellData = new()
                {
                    { "bbbd603f-9da4-4838-b8a8-5124aaa6ba40", 3 },
                    { "017970c3-8663-4351-bfde-a34d6a4d4c74", 4 },
                    { "2d94de25-24d0-4e63-84b4-c509c8eece88", 4 },
                    { "d982739b-77e4-44c3-96ad-b063de013255", 6 }
                };
                BlueprintContainer.AddToSpellList(spellList, spellData);
            }
            if (IsTTTBaseEnabled())
            {
                spellData = new()
                {
                    { "dafdc0ee-f437-4785-aa82-7bf5b2059bf0", 9 }
                };
                BlueprintContainer.AddToSpellList(spellList, spellData);
            }
            #endregion

            #region Cult Caster
            var cultCaster = BPLookup.Selection("WarlockCultCaster", true);
            var features = BPLookup.Selection("WizardFeatSelection")
                .AllFeatures
                .m_Array
                .Where(f => f.Get().GetComponent<AddMetamagicFeat>() != null);
            cultCaster.m_AllFeatures = features.ToArray();
            #endregion
        }
    }
}

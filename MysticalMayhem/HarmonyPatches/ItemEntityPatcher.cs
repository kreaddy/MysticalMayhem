using HarmonyLib;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using MysticalMayhem.Helpers;
using MysticalMayhem.Mechanics;
using System.Linq;
using static MysticalMayhem.Mechanics.FeatureExtender.Feature;

namespace MysticalMayhem.HarmonyPatches
{
    internal class ItemEntityPatcher
    {
        [HarmonyPatch(typeof(ItemEntity), "SpendCharges", typeof(UnitDescriptor))]
        internal class ItemEntity_SpendCharges_MM
        {
            [HarmonyPrefix]
            private static bool SpendCharges(ref bool __result, ItemEntity __instance, UnitDescriptor user)
            {
                // Razmiran Channel check:
                // - User has the feature.
                // - Item mimics a spell found on the Cleric spell list.
                ItemEntityUsable item = __instance as ItemEntityUsable;
                if (user.GetEXFeature(RazmiranChannel) && item?.Ability?.Blueprint?.IsInSpellList(BPLookup.SpellList("ClericSpellList")) == true)
                {
                    // Now we fetch the user's sorcerer spellbook and the slot level needed.
                    var level = item.GetSpellLevel() + 1;
                    var spellbook = user.Spellbooks.Where(s => s.Blueprint.AssetGuidThreadSafe == BPLookup.GetGuid("SorcererSpellbook")).First();

                    if (spellbook.GetSpontaneousSlots(level) > 0)
                    {
                        // User has an appropriate spell slot, so we use it and prevents the magic item from depleting its charges.
                        spellbook.RestoreSpontaneousSlots(level, -1);
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
using HarmonyLib;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UnitLogic;
using MysticalMayhem.Helpers;

namespace MysticalMayhem.HarmonyPatches
{
    [HarmonyPatch(typeof(ActionBarSpellbookHelper))]
    internal class Patch_ActionBarSpellbookHelper
    {
        [HarmonyPatch(nameof(ActionBarSpellbookHelper.IsEquals), typeof(SpellSlot), typeof(SpellSlot))]
        [HarmonyPostfix]
        private static void IsEquals_MM(ref bool __result, SpellSlot s1, SpellSlot s2)
        {
            if (!Settings.IsEnabled("mm.spellbar.fix"))
                return;

            __result = s1.SpellShell.Blueprint == s2.SpellShell.Blueprint
            && s1.SpellLevel == s2.SpellLevel
            && s1.m_Spellbook == s2.m_Spellbook;
        }
    }
}
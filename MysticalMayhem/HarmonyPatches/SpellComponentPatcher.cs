using HarmonyLib;
using Kingmaker.UnitLogic.Abilities;
using MysticalMayhem.Mechanics;

namespace MysticalMayhem.HarmonyPatches
{
    /// <summary>
    /// Hijacks the checks for required spell components. This needs to be staggered so the actual patching is done after blueprints are loaded.
    /// For now only Material Freedom makes use of it.
    /// </summary>
    internal static class SpellComponentPatcher
    {
        public static void PatchAssemblyForMaterialFreedom()
        {
            var harmony = new Harmony(Main.Mod.Info.Id);
            var method = AccessTools.Method(typeof(AbilityData), "get_HasEnoughMaterialComponent");
            var patch = AccessTools.Method(typeof(MaterialFreedom_AbilityData), "MF_get_HasEnoughMaterialComponent");
            harmony.Patch(method, postfix: new HarmonyMethod(patch));
            method = AccessTools.Method(typeof(AbilityData), "SpendMaterialComponent");
            patch = AccessTools.Method(typeof(MaterialFreedom_AbilityData), "MF_SpendMaterialComponent");
            harmony.Patch(method, prefix: new HarmonyMethod(patch));
        }

        private static class MaterialFreedom_AbilityData
        {
            private static bool MF_SpendMaterialComponent(AbilityData __instance)
            {
                if (!__instance.RequireMaterialComponent) { return true; }
                var part = __instance.Caster?.Get<MaterialFreedom.MaterialFreedomUnitPart>();
                if (part != null && part.HasItem(__instance.Blueprint.MaterialComponent.m_Item))
                {
                    return false;
                }
                return true;
            }

            private static void MF_get_HasEnoughMaterialComponent(AbilityData __instance, ref bool __result)
            {
                if (__result || !__instance.RequireMaterialComponent) { return; }
                var part = __instance.Caster?.Get<MaterialFreedom.MaterialFreedomUnitPart>();
                if (part != null && part.HasItem(__instance.Blueprint.MaterialComponent.m_Item))
                {
                    __result = true;
                }
            }
        }
    }
}
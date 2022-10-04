using HarmonyLib;
using System;
using System.Reflection;
using UnityModManagerNet;

namespace MysticalMayhem
{
    internal static class Main
    {
        public static bool Enabled;

        public static UnityModManager.ModEntry Mod;

        public static void Log(string msg)
        {
            Mod.Logger.Log(msg);
        }

        public static void DebugLog(string msg)
        {
#if DEBUG
            Log(msg);
#endif
        }

        public static void Error(Exception exception, string message)
        {
            Log(message);
            Log(exception.ToString());
        }

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            Mod = modEntry;
            modEntry.OnToggle = OnToggle;
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }
    }
}
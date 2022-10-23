using Kingmaker.Localization;
using ModMenu.Settings;
using System.Collections.Generic;
using Menu = ModMenu.ModMenu;

namespace MysticalMayhem.Helpers
{
    internal static class Settings
    {
        private static readonly string RootKey = "mm.settings";

        private static Dictionary<string, LocalizedString> strings = new();

        internal static bool IsEnabled(string key)
        {
            return Menu.GetSettingValue<bool>(GetKey(key));
        }

        internal static void Initialize()
        {
            var settings = SettingsBuilder.New(RootKey, GetString("MM_S_Title"))
                .AddToggle(Toggle.New(GetKey("mm.no.hb"), defaultValue: false, GetString("MM_S_NoHomebrew"))
                .WithLongDescription(GetString("MM_S_NoHomebrew_Desc")))
                .AddToggle(Toggle.New(GetKey("mm.adnd.stoneskin"), defaultValue: true, GetString("MM_S_Adnd_Stoneskin"))
                .WithLongDescription(GetString("MM_S_Andn_Stoneskin_Desc")));

            Menu.AddSettings(settings);
        }

        private static LocalizedString GetString(string key)
        {
            if (!strings.ContainsKey(key)) strings.Add(key, new LocalizedString() { m_Key = key });
            return strings[key];
        }

        private static string GetKey(string partialKey)
        {
            return $"{RootKey}.{partialKey}";
        }
    }
}
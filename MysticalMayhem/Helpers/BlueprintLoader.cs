using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace MysticalMayhem.Helpers
{
    public static class BlueprintLoader
    {
        private static HashSet<string> _blueprints = new();

        public static void LoadBlueprints()
        {
            foreach (string str in _blueprints)
            {
                ResourcesLibrary.BlueprintsCache.RemoveCachedBlueprint(BlueprintGuid.Parse(str));
            }
            _blueprints.Clear();
            BPLookup.ClearAllModData();

            ApplyLocalization();

            BindNewTypes();

            Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(res => res.Contains("Blueprints"))
                .ForEach(res => Load(res));
        }

        public static void ApplyLocalization()
        {
            var localizationPack = LocalizationManager.LoadPack(Path.Combine(Main.Mod.Path, "Localization", LocalizationManager.CurrentLocale +
                ".json"), LocalizationManager.CurrentLocale);
            if (localizationPack == null)
            {
                localizationPack = LocalizationManager.LoadPack(Path.Combine(Main.Mod.Path, "Localization", "enGB.json"),
                    LocalizationManager.CurrentLocale);
            }

            LocalizationPack currentPack = LocalizationManager.CurrentPack;
            if (currentPack == null)
            {
                return;
            }
            var descriptions = localizationPack.m_Strings.Keys
                .Where(k => k.Contains("Desc") || k.Contains("Buff"))
                .ToArray();

            for (var i = 0; i < descriptions.Count(); i++)
            {
                var text = DescriptionTools.TagEncyclopediaEntries(localizationPack.m_Strings[descriptions[i]].Text);
                localizationPack.m_Strings[descriptions[i]] = new() { Text = text };
            }
            currentPack.AddStrings(localizationPack);
        }

        private static void BindNewTypes()
        {
            var binder = (GuidClassBinder)Json.Serializer.Binder;
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                TypeIdAttribute customAttribute = type.GetCustomAttribute<TypeIdAttribute>();
                if (customAttribute != null)
                {
                    binder.m_GuidToTypeCache.Remove(customAttribute.GuidString);
                    binder.m_TypeToGuidCache.Remove(type);
                    binder.AddToCache(type, customAttribute.GuidString);
                }
            }
        }

        private static void Load(string res)
        {
            Main.DebugLog($"Attempting to parse {res}...");
            MMBlueprintJsonWrapper blueprintJsonWrapper = Parse(res);

            BlueprintContainer container = new BlueprintContainer()
            {
                AssetGuid = blueprintJsonWrapper.AssetId,
                Blueprint = blueprintJsonWrapper.Data,
                PatchList = blueprintJsonWrapper.PatchList,
                Homebrew = blueprintJsonWrapper.Homebrew
            };
            blueprintJsonWrapper.Data.OnEnable();
            if (ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse(blueprintJsonWrapper.AssetId)) != null)
            {
                throw new Exception($"Guid {container.AssetGuid} used by {res} already exists! Most likely a mod conflict!");
            }
            ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(BlueprintGuid.Parse(blueprintJsonWrapper.AssetId), blueprintJsonWrapper.Data);
            container.FinalizeBP();
            _blueprints.Add(blueprintJsonWrapper.AssetId);
        }

        private static MMBlueprintJsonWrapper Parse(string res)
        {
            MMBlueprintJsonWrapper blueprintJsonWrapper;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res))
            {
                using StreamReader streamReader = new(stream);
                using JsonTextReader jsonTextReader = new(streamReader);
                blueprintJsonWrapper = Json.Serializer.Deserialize<MMBlueprintJsonWrapper>(jsonTextReader);
            }
            if (blueprintJsonWrapper.Data != null)
            {
                var name = res.Split('.');
                blueprintJsonWrapper.Data.name = name[name.Length - 2];
                blueprintJsonWrapper.Data.AssetGuid = BlueprintGuid.Parse(blueprintJsonWrapper.AssetId);
                blueprintJsonWrapper.LoadedFromPath = res;

                BPLookup.AddModBP(blueprintJsonWrapper.Data.name, blueprintJsonWrapper.AssetId);
            }
            return blueprintJsonWrapper;
        }

        public class MMBlueprintJsonWrapper : BlueprintJsonWrapper
        {
            public string[] PatchList;
            public bool Homebrew;

            [OnDeserializing]
            internal new void OnDeserializing(StreamingContext context)
            {
                Json.BlueprintBeingRead = new BlueprintJsonWrapper()
                {
                    AssetId = AssetId,
                    Data = Data
                };
            }
        }
    }
}
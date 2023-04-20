using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace MysticalMayhem.Helpers
{
    // I mostly took this from bubbles: https://github.com/factubsio/BubbleGauntlet
    // Thank you British man *pien face*
    internal static class ResourceHandler
    {
        private static Dictionary<string, Sprite> sprites = new();

        public static Dictionary<string, Sprite> Sprites { get => sprites; }

        private static UnityEngine.Object[] _assets;

        private static HashSet<string> _loadedBundles = new();

        public static void RemoveBundle(string name, bool unloadAll = false)
        {
            AssetBundle bundle;
            if (bundle = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(x => x.name == name))
                bundle.Unload(unloadAll);

            if (unloadAll)
            {
                _loadedBundles.Clear();
                Sprites.Clear();
            }
        }

        public static void AddBundle(string name)
        {
            try
            {
                AssetBundle bundle;

                RemoveBundle(name, _loadedBundles.Contains(name));

                var path = Path.Combine(Main.Mod.Path, "AssetBundles", name);

                bundle = AssetBundle.LoadFromFile(path);
                if (!bundle) throw new Exception($"Failed to load AssetBundle! {Main.Mod + name}");

                _assets = bundle.LoadAllAssets();
#if false
                foreach (var obj in _assets)
                {
                    Main.DebugLog($"Found asset <{obj.name}> of type [{obj.GetType()}]");
                }
#endif
                foreach (var obj in _assets)
                {
                    if (obj is Sprite sprite)
                        Sprites[obj.name] = sprite;
                }

                _loadedBundles.Add(name);

                RemoveBundle(name);
            }
            catch (Exception ex)
            {
                Main.Error(ex, "LOADING ASSET");
            }
        }

        public static Sprite LoadInternal(string folder, string file)
        {
            return Image2Sprite.Create($"{Main.Mod.Path}Assets{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}{file}.png");
        }
        // Loosely based on https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
        public static class Image2Sprite
        {
            public static string icons_folder = "";
            public static Sprite Create(string filePath)
            {
                var bytes = File.ReadAllBytes(icons_folder + filePath);
                var texture = new Texture2D(64, 64, TextureFormat.DXT5, false);
                _ = texture.LoadImage(bytes);
                return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0, 0));
            }
        }
    }
}
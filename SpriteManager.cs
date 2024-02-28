using System.Collections.Generic;
using System.IO;
using MelonLoader;
using UnityEngine;

namespace MoreQOD
{
    public class SpriteManager
    {
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<Sprite> sprites = new List<Sprite>();
        private readonly AssetBundle _bundle;
        public readonly Sprite MinimapMarkerCommon;
        public readonly Sprite MinimapMarkerRare;
        public readonly Sprite MinimapMarkerEpic;
        public readonly Sprite MinimapMarkerMythic;
        public readonly Sprite MinimapMarkerImmortal;
        public readonly Sprite MinimapMarkerSimple;


        public SpriteManager()
        {
            MelonLogger.Msg(Application.dataPath);
            _bundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath,
                Path.Combine(Application.dataPath, "../Mods/MoreQOD"), "MoreQODAssets"));
            if (_bundle == null)
            {
                MelonLogger.Error("Could not load asset bundle MoreQODAssets");
            }
            else
            {
                foreach (string allAssetName in _bundle.GetAllAssetNames())
                    MelonLogger.Msg(allAssetName + " " + _bundle.LoadAsset(allAssetName).GetType());
            }

            MinimapMarkerSimple = AddSprite("assets/minimap_marker_simple.png");
            MinimapMarkerCommon = AddSprite("assets/minimap_marker_common.png");
            MinimapMarkerRare = AddSprite("assets/minimap_marker_rare.png");
            MinimapMarkerEpic = AddSprite("assets/minimap_marker_epic.png");
            MinimapMarkerMythic = AddSprite("assets/minimap_marker_mythic.png");
            MinimapMarkerImmortal = AddSprite("assets/minimap_marker_immortal.png");
        }

        private Sprite CreateSprite(string name)
        {
            Texture2D texture = _bundle.LoadAsset<Texture2D>(name);
            Rect rect = new Rect(0.0f, 0.0f, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(texture, rect, pivot);
        }


        private Sprite AddSprite(string assetPath)
        {
            Sprite sprite = CreateSprite(assetPath);
            sprites.Add(sprite);
            return sprite;
        }
    }
}
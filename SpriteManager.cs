using System.Collections.Generic;
using System.IO;
using MelonLoader;
using TMPro;
using UnityEngine;

namespace MoreQOD
{
    public class SpriteManager
    {
        public readonly Sprite MinimapMarkerCommon;
        public readonly Sprite MinimapMarkerEpic;
        public readonly Sprite MinimapMarkerImmortal;
        public readonly Sprite MinimapMarkerMythic;
        public readonly Sprite MinimapMarkerRare;
        public readonly Sprite MinimapMarkerSimple;

        public readonly Sprite[] RerollButton;

        public readonly TMP_SpriteAsset RerollSpriteAsset;
        public readonly Dictionary<string, TMP_SpriteAsset> spriteAssets = new();

        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<Sprite> sprites = new();
        private AssetBundle bundle;

        public SpriteManager()
        {
            init();
            MinimapMarkerSimple = AddSprite("minimap_marker_simple.png");
            MinimapMarkerCommon = AddSprite("minimap_marker_common.png");
            MinimapMarkerRare = AddSprite("minimap_marker_rare.png");
            MinimapMarkerEpic = AddSprite("minimap_marker_epic.png");
            MinimapMarkerMythic = AddSprite("minimap_marker_mythic.png");
            MinimapMarkerImmortal = AddSprite("minimap_marker_immortal.png");
            RerollButton = new[] { AddSprite("shoptab_reroll_spr_01.png"), AddSprite("shoptab_reroll_spr_02.png") };
            RerollSpriteAsset = bundle.LoadAsset<TMP_SpriteAsset>("assets/reroll_icon.asset");
            MelonLogger.Msg("Sprite Manager initialized");
        }

        private void init()
        {
            bundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath,
                Path.Combine(Application.dataPath, "../Mods/MoreQOD"), "MoreQODAssets"));
            if (bundle == null)
                MelonLogger.Error("Could not load asset bundle MoreQODAssets");
            else
                foreach (string allAssetName in bundle.GetAllAssetNames())
                    MelonLogger.Msg(allAssetName + " " + bundle.LoadAsset(allAssetName).GetType());
        }

        private Sprite CreateSprite(string name)
        {
            Texture2D texture = bundle.LoadAsset<Texture2D>(name);
            Rect rect = new(0.0f, 0.0f, texture.width, texture.height);
            Vector2 pivot = new(0.5f, 0.5f);
            return Sprite.Create(texture, rect, pivot);
        }
        
        private Sprite AddSprite(string assetPath)
        {
            assetPath = "assets/" + assetPath;
            Sprite sprite = CreateSprite(assetPath);
            sprites.Add(sprite);
            return sprite;
        }
    }
}
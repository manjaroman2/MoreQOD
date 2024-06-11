using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Death.Data;
using Death.WorldGen;
using HarmonyLib;
using MelonLoader;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MoreQOD
{
    public static class Utils
    {
        public static string GetAllFootprints(Exception x)
        {
            StackTrace st = new(x, true);
            StackFrame[] frames = st.GetFrames();
            StringBuilder traceString = new();

            foreach (StackFrame frame in frames)
            {
                if (frame.GetFileLineNumber() < 1)
                    continue;

                traceString.Append("File: " + frame.GetFileName());
                traceString.Append(", Method:" + frame.GetMethod().Name);
                traceString.Append(", LineNumber: " + frame.GetFileLineNumber());
                traceString.Append("  -->  ");
            }

            return traceString.ToString();
        }
        // public static void PrintShopTreasureClasses()
        // {
        //     using IEnumerator<ShopTreasureClass> empEnumerator = Database.ShopTreasureClasses.GetEnumerator();
        //     while (empEnumerator.MoveNext())
        //     {
        //         ShopTreasureClass sc = empEnumerator.Current;
        //         MelonLogger.Msg($"{sc?.TreasureClass} {sc?.FirstAppearPlayTimeMin} {sc?.Weight}");
        //     }
        // }

        public static void FindSpriteAssets(List<string> spriteAssetNames)
        {
            Dictionary<string, TMP_SpriteAsset> allSpriteAssets = new();
            spriteAssetNames = spriteAssetNames
                .Where(spriteAssetName => !MoreQOD.Instance.spriteManager.spriteAssets.ContainsKey(spriteAssetName))
                .ToList();
            if (spriteAssetNames.Count == 0) return;
            foreach (TMP_Text tmpText in Object.FindObjectsOfType<TMP_Text>())
            {
                if (tmpText == null || tmpText.spriteAsset == null) continue;
                if (!allSpriteAssets.ContainsKey(tmpText.spriteAsset.name))
                    allSpriteAssets[tmpText.spriteAsset.name] = tmpText.spriteAsset;

                List<string> spriteAssetNamesCopy = new(spriteAssetNames);
                foreach (var spriteAssetName in spriteAssetNamesCopy)
                    if (!MoreQOD.Instance.spriteManager.spriteAssets.ContainsKey(spriteAssetName) &&
                        tmpText.spriteAsset.name == spriteAssetName)
                    {
                        MoreQOD.Instance.spriteManager.spriteAssets[spriteAssetName] = tmpText.spriteAsset;
                        spriteAssetNames.Remove(spriteAssetName);
                        // MelonLogger.Msg(spriteAssetName);                        
                    }

                if (spriteAssetNames.Count == 0) break;
            }

            if (spriteAssetNames.Count > 0)
            {
                foreach (var spriteAssetName in spriteAssetNames)
                    MelonLogger.Msg($"Could not find spriteAsset {spriteAssetName}");

                MelonLogger.Msg(string.Join(", ", new List<string>(allSpriteAssets.Keys)));
                MelonLogger.Msg(
                    "This isn't an issue, you probably don't have Quality Of Death installed, but this mod still works without it! (You'll be Missing the Gold Icon icon in shop though)");
            }
        }

        public static void CopyValues(TMP_Text from, TMP_Text to)
        {
            if (!(from != null))
                return;
            to.font = from.font;
            to.fontSize = from.fontSize;
            to.fontStyle = from.fontStyle;
            to.alignment = from.alignment;
            to.enableAutoSizing = from.enableAutoSizing;
            to.fontSizeMin = from.fontSizeMin;
            to.fontSizeMax = from.fontSizeMax;
            to.color = from.color;
            to.enableWordWrapping = from.enableWordWrapping;
            to.richText = from.richText;
            to.text = from.text;
            to.raycastTarget = from.raycastTarget;
            to.enableAutoSizing = from.enableAutoSizing;
            to.autoSizeTextContainer = from.autoSizeTextContainer;
            to.overflowMode = from.overflowMode;
            to.isOrthographic = from.isOrthographic;
            to.enableCulling = from.enableCulling;
            to.isOverlay = from.isOverlay;
            to.alpha = from.alpha;
            to.enableVertexGradient = from.enableVertexGradient;
            to.colorGradient = from.colorGradient;
            to.colorGradientPreset = from.colorGradientPreset;
            to.spriteAsset = from.spriteAsset;
            to.tintAllSprites = from.tintAllSprites;
            to.overrideColorTags = from.overrideColorTags;
            to.faceColor = from.faceColor;
            to.outlineColor = from.outlineColor;
            to.outlineWidth = from.outlineWidth;
            RectTransform component1 = from.GetComponent<RectTransform>();
            RectTransform component2 = to.GetComponent<RectTransform>();
            component2.anchorMin = component1.anchorMin;
            component2.anchorMax = component1.anchorMax;
            component2.pivot = component1.pivot;
            component2.anchoredPosition = component1.anchoredPosition;
            component2.sizeDelta = component1.sizeDelta;
        }

        public static void DumpDatabase()
        {
            MelonLogger.Msg(Database.MapObjects);
            foreach (MapObjectData mo in Database.MapObjects.All)
                MelonLogger.Msg(
                    $"{mo.Id}\n      {mo.Code}\n      {mo.Size}\n      {mo.Value}\n      {mo.Weight}\n      {mo.MaxInstances}\n      {mo.ResourcePath}\n      {mo.MapMarkerPath}\n      {mo.Tags}\n      {mo.Implementation}");
        }

        public static void DumpGameObject()
        {
            foreach (GameObject rootGameObject in SceneManager.GetActiveScene()
                         .GetRootGameObjects())
            {
                MelonLogger.Msg(rootGameObject);
                IterateChildren(rootGameObject, (obj, i) =>
                {
                    StringBuilder sb = new();
                    sb.Append(new string(' ', 2 * i));
                    sb.Append(i.ToString());
                    sb.Append(" Fields:");
                    sb.Append(obj.GetType().GetFields(AccessTools.all).Length);
                    sb.Append(" Active:");
                    sb.Append(obj.activeInHierarchy);
                    sb.Append(obj.activeSelf);
                    sb.Append(" Tag:");
                    sb.Append(obj.tag);
                    sb.Append(" Name:");
                    sb.Append(obj.name);
                    MelonLogger.Msg(sb.ToString());
                }, 0);
            }
        }

        private static void IterateChildren(GameObject gameObject, Action<GameObject, int> childHandler, int baseLayer)
        {
            DoIterate(gameObject, childHandler, baseLayer);
        }

        private static void DoIterate(GameObject gameObject, Action<GameObject, int> childHandler, int layer)
        {
            foreach (Transform child in gameObject.transform)
            {
                GameObject o = child.gameObject;
                childHandler(o, layer);
                DoIterate(o, childHandler, layer + 1);
            }
        }
    }
}
using Death.Run.Behaviours.Entities;
using Death.Run.Systems;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace MoreQOD
{
    public static class XpRange
    {
        private static bool activated = false;
        private static Behaviour_GemCollector behaviourGemCollector;

        private static void on()
        {
            MelonLogger.Msg("XpRange: ON");
            /*
            
            if (behaviourGemCollector == null) return;
            object RadiusObj = typeof(Behaviour_GemCollector).GetProperty("Radius", AccessTools.all)
                ?.GetValue(behaviourGemCollector);
            if (RadiusObj == null)
            {
                MelonLogger.Error("Cannot get property Behaviour_GemCollector.Radius");
                return;
            }

            float Radius = (float)RadiusObj;

            // List<Material> materials = new List<Material>();
            // foreach (Material material in Object.FindObjectsOfType<Material>())
            // {
            //     materials.Add(material);
            // }
            // MelonLogger.Msg(materials.Count);
            // MelonLogger.Msg(materials[0]);

            // MelonLogger.Msg(ConfigManager.Get<RenderingConfig>().Shadows.DefaultMaterial);


            // Dictionary<string, Material> materials = new Dictionary<string, Material>();
            Dictionary<Type, List<Renderer>> renderers = new Dictionary<Type, List<Renderer>>();
            foreach (Renderer renderer in Object.FindObjectsOfType<Renderer>())
            {
                if (renderer == null) continue;
                if (!renderers.ContainsKey(renderer.GetType()))
                {
                    renderers[renderer.GetType()] = new List<Renderer>();
                }

                renderers[renderer.GetType()].Add(renderer);
                foreach (Material rendererMaterial in renderer.materials)
                {
                    Materials.TryAdd(rendererMaterial.name, rendererMaterial);
                }
            }

            MelonLogger.Msg($"Materials: {Materials.Count}");
            foreach ((var key, Material value) in Materials)
            {
                MelonLogger.Msg($"  >{key}<");
            }


            Behaviour_Player player = Object.FindObjectOfType<Behaviour_Player>();
            if (player == null) return;
            XpRangeMonoBehaviour xpRangeMonoBehaviour = player.gameObject.AddComponent<XpRangeMonoBehaviour>();
            xpRangeMonoBehaviour.gameObject.SetActive(true);
            xpRangeMonoBehaviour.segments = 128;
            xpRangeMonoBehaviour.radius = 5.0f;
            
            
            */

            // MelonLogger.Msg(materials);

            // foreach ((Type key, List<Renderer> value) in renderers)
            // {
            //     MelonLogger.Msg($"{key} {value.Count}");
            // }

            // Behaviour_Player player = Object.FindObjectOfType<Behaviour_Player>();
            // LineRenderer line = player.Entity.gameObject.AddComponent<LineRenderer>();

            // line.material = new Material(Shader.Find(""))

            // GameObject gameObject = new GameObject("GUI_XpRange");
            // gameObject.transform.SetParent(player.Entity.transform);
            // GUI_XpRange guiXpRange = gameObject.gameObject.AddComponent<GUI_XpRange>();
            // guiXpRange.xradius = Radius;
            // guiXpRange.yradius = Radius; 
            // gameObject.SetActive(true);

            // MelonLogger.Msg($"{Radius} Gem collection Radius");

            // foreach ((var key, TMP_SpriteAsset value) in spriteAssets)
            // {
            //     MelonLogger.Msg($"{key} {value.spriteCharacterTable.Count}");
            // }

            // Dictionary<string, List<TMP_SpriteAsset>> assets = new Dictionary<string, List<TMP_SpriteAsset>>();
            // foreach (TMP_Text tmpText in Object.FindObjectsOfType<TMP_Text>())
            // {
            //     if (tmpText == null || tmpText.spriteAsset == null) continue;
            //     if (!assets.ContainsKey(tmpText.spriteAsset.name))
            //     {
            //         assets[tmpText.spriteAsset.name] = new List<TMP_SpriteAsset>();
            //     }
            //
            //     assets[tmpText.spriteAsset.name].Add(tmpText.spriteAsset);
            // }
            // foreach (var assetsKey in assets.Keys)
            // {
            //     MelonLogger.Msg(assetsKey);
            // }
            // Utils.PrintShopTreasureClasses();
        }

        private static void off()
        {
            MelonLogger.Msg("XpRange: OFF");
            
        }
        
        public static void toggle()
        {
            activated = !activated;
            if (activated) on();
            else off();
        }
        
        [HarmonyPatch(typeof(System_PlayerManager))]
        public class System_PlayerManagerPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnInit")]
            private static void POST_OnInit()
            {
                behaviourGemCollector =
                    Object.FindObjectOfType<Behaviour_GemCollector>();
                if (behaviourGemCollector == null)
                {
                    MelonLogger.Error("Cannot find Behaviour_GemCollector!");
                    return;
                }
                object Radius = typeof(Behaviour_GemCollector).GetProperty("Radius", AccessTools.all)
                    ?.GetValue(behaviourGemCollector);
                if (Radius == null)
                {
                    MelonLogger.Error("Cannot invoke Behaviour_GemCollector.Radius()!");
                    return;
                }
                MelonLogger.Msg($"{(float)Radius} Gem collection Radius");
            }
        }
    }
}
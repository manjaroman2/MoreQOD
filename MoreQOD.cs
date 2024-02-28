using System.Collections.Generic;
using System.Reflection;
using Death.Run.UserInterface.Items;
using HarmonyLib;
using MelonLoader;
using TMPro;
using UnityEngine;

namespace MoreQOD
{
    public class MoreQOD : MelonMod
    {
        public static readonly Dictionary<string, Material> Materials = new();

        public static readonly Dictionary<string, TMP_SpriteAsset> spriteAssets = new();

        public static bool IsRun;
        public static SpriteManager spriteManager;

        public override void OnInitializeMelon()
        {
            spriteManager = new SpriteManager();
            ShopImprovements.Init();
            // Event.AddListener(new EventListener<Event_InteractableFocusGained>(FindButtonSpriteAsset));
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            switch (sceneName)
            {
                case "Scene_Run":
                    IsRun = true;
                    break;
                case "Scene_RunGUI":
                    break;
                case "Scene_TimesRealm":
                    IsRun = false;
                    ShopImprovements.OnRunExit();
                    // Utils.FindSpriteAssets(new List<string>(new[]
                    //     { "_S_ButtonPrompts", "TextIcons_Items", "_S_ButtonPrompts_Small" }));
                    break;
            }

            LoggerInstance.Msg($"Scene {sceneName} with build index {buildIndex} has been loaded!");
        }

        public override void OnLateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ShopImprovements.Reroll();
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                ShopImprovements.Upgrade();
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                XpRange.toggle();
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                StashImprovements.NextPage();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                StashImprovements.PreviousPage();
            }
        }
    }
}
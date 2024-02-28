using System.Collections.Generic;
using System.Reflection;
using Death.Run.Behaviours.Entities;
using Death.Run.UserInterface.Items;
using HarmonyLib;
using MelonLoader;
using TMPro;
using UnityEngine;

namespace MoreQOD
{
    public class MoreQOD : MelonMod
    {
        private static readonly MethodInfo StashTabManager_SelectPrev =
            typeof(GUI_StashTabManager).GetMethod(nameof(GUI_StashTabManager.SelectPrev), AccessTools.all);

        private static readonly MethodInfo StashTabManager_SelectNext =
            typeof(GUI_StashTabManager).GetMethod(nameof(GUI_StashTabManager.SelectPrev), AccessTools.all);

        private static readonly MethodInfo ItemManagerStash_SelectPrevTab =
            typeof(GUI_ItemManager_Stash).GetMethod("SelectPrevTab", AccessTools.all);

        private static readonly MethodInfo ItemManagerStash_SelectNextTab =
            typeof(GUI_ItemManager_Stash).GetMethod("SelectNextTab", AccessTools.all);


        public static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        public static readonly Dictionary<string, TMP_SpriteAsset> spriteAssets =
            new Dictionary<string, TMP_SpriteAsset>();

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

            /*
            else if (Input.GetKeyDown(KeyCode.O))
            {
                if (ItemManagerStash == null) return;
                if (typeof(GUI_TabManager<int>).GetField("_activeInstances", AccessTools.all)
                        ?.GetValue(StashTabManager) is List<GUI_Tab<int>> activeInstances) MelonLogger.Msg(activeInstances.Count);
                ItemManagerStash_SelectPrevTab.Invoke(ItemManagerStash, null);
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                if (ItemManagerStash == null) return;
                if (typeof(GUI_TabManager<int>).GetField("_activeInstances", AccessTools.all)
                        ?.GetValue(StashTabManager) is List<GUI_Tab<int>> activeInstances) MelonLogger.Msg(activeInstances.Count);
                ItemManagerStash_SelectNextTab.Invoke(ItemManagerStash, null);
            }
             */
        }
    }
}
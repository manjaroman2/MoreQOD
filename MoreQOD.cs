using System.Collections.Generic;
using Claw.UserInterface.Screens;
using Death.App;
using Death.App.StateManagement;
using Death.TimesRealm;
using Death.TimesRealm.UserInterface;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreQOD
{
    public class MoreQOD : MelonMod
    {
        public static MoreQOD Instance;

        private readonly List<Feature> Features = new();
        public readonly Dictionary<string, Material> Materials = new();
        public GameManager dGameManager;
        public GameStateManager dGameStateManager;
        public ScreenManager FacadeLobbyScreenManager;

        public bool IsRun;
        public SpriteManager spriteManager;
        public StashSort StashSort;

        public override void OnInitializeMelon()
        {
            Instance = this;
            MelonLogger.Msg("OnInitializeMelon");
            spriteManager = new SpriteManager();

            addFeatures();

            HarmonyInstance.Patch(
                typeof(Facade_Lobby).GetMethod(nameof(Facade_Lobby.Init), AccessTools.all, null,
                    new[] { typeof(ILobbyGameState) }, null),
                postfix: new HarmonyMethod(typeof(MoreQOD).GetMethod(nameof(Facade_Lobby__Init__Postfix),
                    AccessTools.all)));
        }

        private void addFeatures()
        {
            Features.Add(new ItemMarkers(false));
            StashSort = new StashSort();
            Features.Add(StashSort);

            foreach (Feature feature in Features)
                if (feature is Hookable fHookable && feature.isEnabled())
                    fHookable.addHarmonyHooks();
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
                    break;
                case "Scene_LobbyGUI":
                    break;
                case "Scene_Bootstrap":
                    dGameManager = SceneManager.GetActiveScene().GetRootGameObjects()[0].GetComponent<GameManager>();
                    break;
                default:
                    LoggerInstance.Msg($"Scene {sceneName} with build index {buildIndex} has been loaded!");
                    break;
            }
        }

        private static void Facade_Lobby__Init__Postfix(ILobbyGameState state, Facade_Lobby __instance)
        {
            Instance.FacadeLobbyScreenManager =
                (ScreenManager)typeof(Facade_Lobby).GetField("_screenManager", AccessTools.all)?.GetValue(__instance);
        }

        public override void OnLateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.S))
                if (FacadeLobbyScreenManager != null)
                    if (FacadeLobbyScreenManager.CurrentScreen is Screen_Stash)
                        StashSort.sortSelectedPage();
        }
    }
}
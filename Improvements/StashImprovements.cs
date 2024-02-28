using Death.App;
using Death.Items;
using Death.Run.UserInterface.Items;
using HarmonyLib;
using MelonLoader;

namespace MoreQOD
{
    public static class StashImprovements
    {
        /*
        public static GUI_StashTabManager StashTabManager;
        public static GUI_ItemManager_Stash ItemManagerStash;
        
        [HarmonyPatch(typeof(GUI_ItemManager_Stash))]
        internal static class GUI_ItemManager_StashPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(GUI_ItemManager_Stash.Init), typeof(Profile))]
            private static void Init_Postfix(Profile profile, ref GUI_ItemManager_Stash __instance, ref GUI_StashTabManager ____stashTabManager)
            {
                StashTabManager = ____stashTabManager;
                ItemManagerStash = __instance;
            }
        
        }
        [HarmonyPatch(typeof(StashData))]
        internal static class StashData_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(MethodType.Constructor, typeof(int))]
            private static void StashData_Constructor_Patch(int defaultPageCount, ref StashData __instance)
            {
                MelonLogger.Msg("Adding 5 stash pages");
            
                __instance.AddPages(5);
            }
        
        }
         */
    }
}
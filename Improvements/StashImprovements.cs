using System.Collections.Generic;
using System.Reflection;
using Death.Run.UserInterface.Items;
using HarmonyLib;
using MelonLoader;

namespace MoreQOD
{
    public static class StashImprovements
    {
        // private static readonly MethodInfo StashTabManager_SelectPrev =
        //     typeof(GUI_StashTabManager).GetMethod(nameof(GUI_StashTabManager.SelectPrev), AccessTools.all);
        //
        // private static readonly MethodInfo StashTabManager_SelectNext =
        //     typeof(GUI_StashTabManager).GetMethod(nameof(GUI_StashTabManager.SelectPrev), AccessTools.all);
        //
        // private static readonly MethodInfo ItemManagerStash_SelectPrevTab =
        //     typeof(GUI_ItemManager_Stash).GetMethod("SelectPrevTab", AccessTools.all);
        //
        // private static readonly MethodInfo ItemManagerStash_SelectNextTab =
        //     typeof(GUI_ItemManager_Stash).GetMethod("SelectNextTab", AccessTools.all);
        //
        // private static GUI_ItemManager_Stash ItemManagerStash; 
        // private static GUI_StashTabManager StashTabManager; 
        // public static void NextPage()
        // {
        //     if (ItemManagerStash == null) return;
        //     if (typeof(GUI_TabManager<int>).GetField("_activeInstances", AccessTools.all)
        //             ?.GetValue(StashTabManager) is List<GUI_Tab<int>> activeInstances) MelonLogger.Msg(activeInstances.Count);
        //     ItemManagerStash_SelectPrevTab.Invoke(ItemManagerStash, null);
        // }
        //
        // public static void PreviousPage()
        // {
        //     if (ItemManagerStash == null) return;
        //     if (typeof(GUI_TabManager<int>).GetField("_activeInstances", AccessTools.all)
        //             ?.GetValue(StashTabManager) is List<GUI_Tab<int>> activeInstances) MelonLogger.Msg(activeInstances.Count);
        //     ItemManagerStash_SelectNextTab.Invoke(ItemManagerStash, null);
        // }
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
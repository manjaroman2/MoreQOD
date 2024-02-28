using System;
using System.Collections.Generic;
using System.Reflection;
using Claw.Core.Structures;
using Death;
using Death.Achievements;
using Death.App;
using Death.Audio;
using Death.Items;
using Death.Run.UserInterface.Items;
using Death.Shop;
using Death.TimesRealm.UserInterface;
using Death.Unlockables;
using Death.Utils.Collections;
using HarmonyLib;
using MelonLoader;
using TMPro;
using UnityEngine;

namespace MoreQOD
{
    public abstract class ShopImprovements
    {
        private static readonly MethodInfo _GetTreasureClassWeightsForTimeMethod = typeof(ShopGenerator)
            .GetMethod("GetTreasureClassWeightsForTime", AccessTools.all, null, new[] { typeof(float) }, null);

        private static GoldCostGenerator shopGoldCostGenerator;
        private static bool IsInShop;
        private static int availableRerolls { get; set; }
        private static CueReference screenShopSound;
        private static ShopGenerator shopGenerator;
        private static ItemGenerator itemGenerator;
        private static GUI_ItemSlot selectedSlotShop;

        private const int rerollsPerRun = 10;

        public static void Init()
        {
            shopGoldCostGenerator = new GoldCostGenerator(1, 1.5f, 0.01f);
        }

        public static bool Reroll()
        {
            if (!IsInShop) return false;
            if (Game.ActiveProfile.Gold <= shopGoldCostGenerator.cost || availableRerolls <= 0) return false;
            MelonLogger.Msg("Re-rolling Shop!");
            availableRerolls -= 1;
            Game.ActiveProfile.ReGenerateShop();
            Game.ActiveProfile.Gold -= shopGoldCostGenerator.cost;
            screenShopSound.Fire2D();
            shopGoldCostGenerator.update();
            return true;
        }

        public static bool Upgrade()
        {
            if (!IsInShop) return false;
            if (selectedSlotShop == null || Game.ActiveProfile.Gold <= 1) return false;
            Item upgradedItem = UpgradeItemRarity(selectedSlotShop.Slot.Item);
            if (upgradedItem == null) return false;
            selectedSlotShop.Slot.Set(upgradedItem);
            upgradedItem.PlayDropAudio();
            MelonLogger.Msg("Upgraded item!");
            Game.ActiveProfile.Gold -= 1;
            return true;
        }

        public static void OnRunExit()
        {
            availableRerolls = rerollsPerRun;
        }


        private class GoldCostGenerator
        {
            private readonly int _baseCost;
            private readonly float _exponent;
            private readonly float _multiplier;

            private readonly List<Action<int>> _callbacks;

            public GoldCostGenerator(int baseCost, float exponent, float multiplier)
            {
                _baseCost = baseCost;
                _exponent = exponent;
                _multiplier = multiplier;
                _callbacks = new List<Action<int>>();
            }

            public void hook(Action<int> callback)
            {
                _callbacks.Add(callback);
            }

            public void update()
            {
                float currentMin = Game.ActiveProfile.Progression.TimeSpentInRunMin;
                // f(x) = m*exp(x, _exp)+base
                cost = (int)Math.Floor(_multiplier * Math.Pow(currentMin, _exponent)) + _baseCost;

                foreach (Action<int> callback in _callbacks)
                {
                    callback(cost);
                }
            }

            public int cost { get; private set; }
        }

        private static Item UpgradeItemRarity(IReadOnlyItem item)
        {
            if (item.Rarity >= ItemRarity.Mythic) return null;
            float timeSpentInRunMin = Game.ActiveProfile.Progression.TimeSpentInRunMin;
            ItemGenerator.Context context = Game.ActiveProfile.GenerateItemContext();
            WeighedRandomSet<string> classWeightsForTime =
                (WeighedRandomSet<string>)_GetTreasureClassWeightsForTimeMethod.Invoke(shopGenerator,
                    new object[] { timeSpentInRunMin });
            WeighedRandomSet<ItemType> typeWeights = new WeighedRandomSet<ItemType> { { item.Type, 1.0f } };
            using (WeighedRandomSetPool<ItemRarity>.Get(out WeighedRandomSet<ItemRarity> rarityWeightedRandomSetPool))
            {
                rarityWeightedRandomSetPool.Add(item.Rarity + 1, 1.0f);
                ItemGenerator.Recipe recipe = new ItemGenerator.Recipe(
                    classWeightsForTime.PickRandom(),
                    Game.ActiveProfile.Progression.MaxItemTier, 0.0f,
                    rarityWeightedRandomSetPool,
                    typeWeights
                );
                return itemGenerator.Generate(recipe, context);
            }
        }

        private static void OnShopEntered(GUI_Shop shop)
        {
            IsInShop = true;
            shop.OnSelectEv += slot => { selectedSlotShop = slot; };
        }

        [HarmonyPatch(typeof(Screen_Shop))]
        internal static class PATCH_Screen_Shop
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnShow")]
            private static void PRE_OnShow(ref CueReference ____openCue)
            {
                screenShopSound = ____openCue;
            }
            // [HarmonyPostfix]
            // [HarmonyPatch("PostShow")]
            // private static void PostShow_Postfix(ref Screen_Shop __instance)
            // {
            //     
            // }
        }

        internal static class PATCH_GUI_ItemManager_Shop
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(GUI_ItemManager_Shop.PostOpen))]
            private static void POST_PostOpen(GUI_Shop ____shop)
            {
                OnShopEntered(____shop);
            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(GUI_ItemManager_Shop.OnClose), new Type[] { })]
            private static void POST_OnClose()
            {
                // MelonLogger.Msg("Is not shop");
                IsInShop = false;
            }


            [HarmonyPostfix]
            [HarmonyPatch(nameof(GUI_ItemManager_Shop.Init), typeof(Profile))]
            private static void POST_Init(ref GUI_ItemManager_Shop __instance, ref GUI_Gold ____gold)
            {
                TextMeshProUGUI goldGuiText =
                    AccessTools.Field(typeof(GUI_Gold), "_text").GetValue(____gold) as TextMeshProUGUI;

                Utils.FindSpriteAssets(new List<string>(new[]
                    { "_S_ButtonPrompts", "TextIcons_Items", "_S_ButtonPrompts_Small" }));

                GameObject ShopRerollCost = new GameObject("ShopRerollCost");
                ShopRerollCost.transform.SetParent(__instance.transform);
                TextMeshProUGUI shopRerollCostTMPro = ShopRerollCost.gameObject.AddComponent<TextMeshProUGUI>();
                Utils.CopyValues(goldGuiText, shopRerollCostTMPro);
                shopRerollCostTMPro.spriteAsset = MoreQOD.spriteAssets["_S_ButtonPrompts_Small"];
                shopRerollCostTMPro.rectTransform.anchorMax = new Vector2(0.25f, 0.19f);
                shopRerollCostTMPro.rectTransform.anchorMin = new Vector2(0.25f, 0.19f);

                if (MoreQOD.spriteAssets.TryGetValue("TextIcons_Items", out TMP_SpriteAsset asset))
                {
                    GameObject GoldIcon = new GameObject("GoldIcon");
                    GoldIcon.transform.SetParent(__instance.transform);
                    TextMeshProUGUI goldIconTMPro = GoldIcon.gameObject.AddComponent<TextMeshProUGUI>();
                    Utils.CopyValues(goldGuiText, goldIconTMPro);
                    goldIconTMPro.spriteAsset = asset;
                    goldIconTMPro.rectTransform.anchorMax = new Vector2(0.24f, 0.19f);
                    goldIconTMPro.rectTransform.anchorMin = new Vector2(0.24f, 0.19f);

                    goldIconTMPro.text = "<sprite=1>";

                    GoldIcon.gameObject.SetActive(true);
                }

                shopGoldCostGenerator.hook(cost =>
                {
                    shopRerollCostTMPro.text =
                        $"<color=#FF0000>-{cost}</color><sprite=44> {availableRerolls}/{rerollsPerRun}";
                });
                shopGoldCostGenerator.update();
            }
        }

        [HarmonyPatch(typeof(Profile))]
        internal static class PATCH_Profile
        {
            [HarmonyPostfix]
            [HarmonyPatch(MethodType.Constructor, typeof(AchievementManager), typeof(Unlocks))]
            private static void POST_Profile(ref Profile __instance, ref ShopGenerator ____shopGenerator)
            {
                MelonLogger.Msg("Profile Initialized!");
                shopGenerator = ____shopGenerator;
                itemGenerator = (ItemGenerator)typeof(ShopGenerator).GetField("_itemGenerator",
                    AccessTools.all)?.GetValue(shopGenerator);
                MelonLogger.Msg($"stash pages: {__instance.Stash.PageCount}");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using Claw.Core.CustomAttributes;
using Claw.Core.Structures;
using Claw.UserInterface.Selection;
using Death;
using Death.Achievements;
using Death.App;
using Death.App.Options.UserInterface;
using Death.Audio;
using Death.Items;
using Death.Run.UserInterface.Items;
using Death.Shop;
using Death.TimesRealm.UserInterface;
using Death.Unlockables;
using Death.Utils;
using Death.Utils.Collections;
using HarmonyLib;
using MelonLoader;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MoreQOD
{
    public static class ShopImprovements
    {
        private static readonly MethodInfo _GetTreasureClassWeightsForTimeMethod = typeof(ShopGenerator)
            .GetMethod("GetTreasureClassWeightsForTime", AccessTools.all, null, new[] { typeof(float) }, null);

        private static GameObject buttonPrefab;

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
            private readonly int baseCost;
            private readonly float exponent;
            private readonly float multiplier;

            private readonly List<Action<int>> callbacks;

            public GoldCostGenerator(int baseCost, float exponent, float multiplier)
            {
                this.baseCost = baseCost;
                this.exponent = exponent;
                this.multiplier = multiplier;
                callbacks = new List<Action<int>>();
            }

            public void hook(Action<int> callback)
            {
                callbacks.Add(callback);
            }

            public void update()
            {
                float currentMin = Game.ActiveProfile.Progression.TimeSpentInRunMin;
                // f(x) = m*exp(x, _exp)+base
                cost = (int)Math.Floor(multiplier * Math.Pow(currentMin, exponent)) + baseCost;

                foreach (Action<int> callback in callbacks)
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
            WeighedRandomSet<ItemType> typeWeights = new() { { item.Type, 1.0f } };
            using (WeighedRandomSetPool<ItemRarity>.Get(out WeighedRandomSet<ItemRarity> rarityWeightedRandomSetPool))
            {
                rarityWeightedRandomSetPool.Add(item.Rarity + 1, 1.0f);
                ItemGenerator.Recipe recipe = new(
                    classWeightsForTime.PickRandom(),
                    Game.ActiveProfile.Progression.MaxItemTier, 0.0f,
                    rarityWeightedRandomSetPool,
                    typeWeights
                );
                return itemGenerator.Generate(recipe, context);
            }
        }

        [RequireComponent(typeof(SelectableObject))]
        private class RerollButton : MonoBehaviour
        {
            private static readonly int Bool_IsHovered = Animator.StringToHash("IsHovered");
            private static readonly int Bool_IsDisabled = Animator.StringToHash("IsDisabled");
            private static readonly int Trigger_Select = Animator.StringToHash("OnSelect");
            private static readonly int Trigger_Appear = Animator.StringToHash("OnAppear");
            [SerializeField] [ReadOnly] private SelectableObject _selectable;
            [SerializeField] [ReadOnly] private Animator _animator;
            [SerializeField] private float _appearDelay;
            [SerializeField] private TextMeshProUGUI _countText;
            private bool _interactionEnabled = true;
            private bool _isDisabledDirty;
            private int _count;

            public UnityEvent<GameObject> OnClickEv => _selectable.OnClickEv;

            public void SetVisible(bool visible)
            {
                gameObject.SetActive(visible);
                if (!visible)
                    return;
                Invoker.Cancel(Appear);
                Invoker.InvokeDelayedUnscaled(Appear, _appearDelay);
                _isDisabledDirty = true;
            }

            public void SetInteractionEnabled(bool value)
            {
                _interactionEnabled = value;
                UpdateInteractionEnabled();
            }

            public void UpdateCount(int count)
            {
                _count = count;
                _countText.text = count.ToString();
                UpdateInteractionEnabled();
            }

            private void UpdateInteractionEnabled()
            {
                _selectable.Interactable = _interactionEnabled && _count > 0;
                _selectable.AllowMouseInteraction = _interactionEnabled && _count > 0;
                _isDisabledDirty = true;
            }

            private void Appear() => _animator.SetTrigger(Trigger_Appear);

            private void OnValidate()
            {
                if (_selectable == null)
                    _selectable = GetComponent<SelectableObject>();
                if (_animator != null)
                    return;
                _animator = GetComponent<Animator>();
            }

            private void Awake()
            {
                _selectable.OnSelectEv.AddListener(OnSelect);
                _selectable.OnDeselectEv.AddListener(OnDeselect);
                _selectable.OnClickEv.AddListener(OnClick);
            }

            private void Update()
            {
                if (!_isDisabledDirty)
                    return;
                _isDisabledDirty = false;
                _animator.SetBool(Bool_IsDisabled, !_selectable.Interactable);
            }

            private void OnSelect(GameObject go)
            {
                _animator.SetBool(Bool_IsHovered, true);
            }

            private void OnDeselect(GameObject go)
            {
                _animator.SetBool(Bool_IsHovered, false);
            }

            private void OnClick(GameObject go)
            {
                _animator.SetTrigger(Trigger_Select);
            }
        }

        [HarmonyPatch(typeof(GUI_Options))]
        internal static class PATCH_GUI_Options
        {

            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            private static void POST_Awake(ref GUI_Options __instance)
            {
                if (buttonPrefab == null)
                    buttonPrefab = __instance.transform.Find("Screen_Gameplay/Panel_Large").gameObject;
            }
            
        }


        [HarmonyPatch(typeof(Screen_Shop))]
        internal static class PATCH_Screen_Shop
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Screen_Shop.Init))]
            private static void POST_Init(Profile profile, ref Screen_Shop __instance)
            {
            }
            [HarmonyPrefix]
            [HarmonyPatch("OnShow")]
            private static void PRE_OnShow(ref CueReference ____openCue)
            {
                screenShopSound = ____openCue;
            }
        }

        [HarmonyPatch(typeof(GUI_Shop))]
        internal static class PATCH_GUI_Shop
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(GUI_Shop.Init))]
            private static void POST_Init(ref GUI_Shop __instance, ref GUI_ShopTabManager ____shopTabs)
            {
                MelonLogger.Msg("GUI_Shop.Init POST");
                
                
                
                // Bad reference 
                // GameObject prefab = ____shopTabs.transform.GetChild(1).gameObject;
                // MelonLogger.Msg(prefab);
                // foreach (Component component1 in prefab.GetComponents(typeof(Component)))
                // {
                //     MelonLogger.Msg(component1);
                // }
                // GameObject gameObject = Object.Instantiate(buttonPrefab);

                GameObject gameObject = new("Image");
                // GameObject gameObject = Object.Instantiate(prefab);
                gameObject.transform.SetParent(____shopTabs.transform);
                gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                gameObject.AddComponent<Image>().sprite = MoreQOD.spriteManager.RerollButton[0];
                RectTransform component = gameObject.GetComponent<RectTransform>();
                Vector2 position = new (0.3f, 0.9f);
                float scale = 0.25f; 
                component.anchorMin = position;
                component.anchorMax = position;
                component.pivot = new Vector2(0.5f, 0.5f);
                component.localScale = new Vector3(scale, scale, scale);

                
                

                // TextMeshProUGUI goldGuiText =
                //     AccessTools.Field(typeof(GUI_Gold), "_text").GetValue(____gold) as TextMeshProUGUI;
                //
                // Utils.FindSpriteAssets(new List<string>(new[]
                //     { "_S_ButtonPrompts", "TextIcons_Items", "_S_ButtonPrompts_Small" }));

                // GameObject ShopRerollCost = new("ShopRerollCost");
                // ShopRerollCost.transform.SetParent(__instance.transform);
                // TextMeshProUGUI shopRerollCostTMPro = ShopRerollCost.gameObject.AddComponent<TextMeshProUGUI>();
                // Utils.CopyValues(goldGuiText, shopRerollCostTMPro);
                // shopRerollCostTMPro.spriteAsset = MoreQOD.spriteManager.spriteAssets["_S_ButtonPrompts_Small"];
                // shopRerollCostTMPro.rectTransform.anchorMax = new Vector2(0.25f, 0.19f);
                // shopRerollCostTMPro.rectTransform.anchorMin = new Vector2(0.25f, 0.19f);
            }
        }

        [HarmonyPatch(typeof(GUI_ItemManager_Shop))]
        internal static class PATCH_GUI_ItemManager_Shop
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(GUI_ItemManager_Shop.PostOpen))]
            private static void POST_PostOpen(GUI_Shop ____shop)
            {
                IsInShop = true;
                ____shop.OnSelectEv += slot => { selectedSlotShop = slot; };
            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(GUI_ItemManager_Shop.OnClose), new Type[] { })]
            private static void POST_OnClose()
            {
                // MelonLogger.Msg("Is not shop");
                IsInShop = false;
            }


            [HarmonyPostfix]
            [HarmonyPatch(nameof(GUI_ItemManager_Shop.Init))]
            private static void POST_Init(ref GUI_ItemManager_Shop __instance, ref GUI_Gold ____gold)
            {
                MelonLogger.Msg("GUI_ItemManager_Shop.Init POST");
                
                /*

                TextMeshProUGUI goldGuiText =
                    AccessTools.Field(typeof(GUI_Gold), "_text").GetValue(____gold) as TextMeshProUGUI;

                Utils.FindSpriteAssets(new List<string>(new[]
                    { "_S_ButtonPrompts", "TextIcons_Items", "_S_ButtonPrompts_Small" }));

                GameObject ShopRerollCost = new("ShopRerollCost");
                ShopRerollCost.transform.SetParent(__instance.transform);
                TextMeshProUGUI shopRerollCostTMPro = ShopRerollCost.gameObject.AddComponent<TextMeshProUGUI>();
                Utils.CopyValues(goldGuiText, shopRerollCostTMPro);
                shopRerollCostTMPro.spriteAsset = MoreQOD.spriteManager.spriteAssets["_S_ButtonPrompts_Small"];
                shopRerollCostTMPro.rectTransform.anchorMax = new Vector2(0.25f, 0.19f);
                shopRerollCostTMPro.rectTransform.anchorMin = new Vector2(0.25f, 0.19f);

                if (MoreQOD.spriteManager.spriteAssets.TryGetValue("TextIcons_Items", out TMP_SpriteAsset asset))
                {
                    GameObject GoldIcon = new("GoldIcon");
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
                */
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
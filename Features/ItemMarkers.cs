using System;
using System.Collections.Generic;
using Death.Items;
using Death.Run.Behaviours.Objects;
using Death.Run.UserInterface.HUD.Minimap;
using Death.Run.UserInterface.Items;
using Death.UserInterface.Localization;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace MoreQOD
{
    public class ItemMarkers : Feature, Hookable
    {
        public ItemMarkers(bool enabled = true) : base(enabled)
        {
        }

        private readonly Dictionary<ItemPickUp, Vector2> DroppedItems = new();

        public void addHarmonyHooks()
        {
            MoreQOD.Instance.HarmonyInstance.Patch(typeof(GUI_FloatingItemName).GetMethod(
                    nameof(GUI_FloatingItemName.Init), AccessTools.all, null,
                    new[] { typeof(ItemPickUp) }, null),
                new HarmonyMethod(typeof(ItemMarkers).GetMethod(nameof(GUI_FloatingItemName__Init__Prefix))));

            MoreQOD.Instance.HarmonyInstance.Patch(typeof(ItemPickUp).GetMethod(
                    nameof(ItemPickUp.Init), AccessTools.all, null,
                    new[] { typeof(Vector2), typeof(Item) }, null),
                new HarmonyMethod(typeof(ItemMarkers).GetMethod(nameof(ItemPickUp__Init__Prefix))));

            MoreQOD.Instance.HarmonyInstance.Patch(typeof(ItemPickUp).GetMethod(
                    "OnDestroy", AccessTools.all, null,
                    new Type[] { }, null),
                new HarmonyMethod(typeof(ItemMarkers).GetMethod(nameof(ItemPickUp__OnDestroy__Prefix))));
        }

        private void OnItemDropped(ItemPickUp pickUp, string itemName, Vector2 pos)
        {
            MelonLogger.Msg($"Dropped item '{itemName}' at {pos}");
            if (!MoreQOD.Instance.IsRun) return;
            Sprite markerSprite;
            switch (pickUp.Item.Rarity)
            {
                case ItemRarity.Common:
                    markerSprite = MoreQOD.Instance.spriteManager.MinimapMarkerCommon;
                    break;
                case ItemRarity.Rare:
                    markerSprite = MoreQOD.Instance.spriteManager.MinimapMarkerRare;
                    break;
                case ItemRarity.Epic:
                    markerSprite = MoreQOD.Instance.spriteManager.MinimapMarkerEpic;
                    break;
                case ItemRarity.Mythic:
                    markerSprite = MoreQOD.Instance.spriteManager.MinimapMarkerMythic;
                    break;
                case ItemRarity.Immortal:
                    markerSprite = MoreQOD.Instance.spriteManager.MinimapMarkerImmortal;
                    break;
                case ItemRarity.Broken: // Broken = Simple 
                case ItemRarity._Count:
                default:
                    markerSprite = MoreQOD.Instance.spriteManager.MinimapMarkerSimple;
                    break;
            }

            if (markerSprite == null)
                MelonLogger.Error($"No marker sprite found for rarity {pickUp.Item.Rarity}");
            else
                Minimap.Get()
                    .AddMarker(pickUp.transform, markerSprite, true);
        }

        private void OnItemCollected(ItemPickUp pickUp)
        {
            if (MoreQOD.Instance.IsRun) Minimap.Get().RemoveMarker(pickUp.transform);
        }

        private void GUI_FloatingItemName__Init__Prefix(ItemPickUp pickUp, ref LocalizedItemName ____localizedName,
            ref GUI_FloatingItemName __instance)
        {
            ____localizedName.OnChangeEv.AddListener(s =>
            {
                if (DroppedItems.TryGetValue(pickUp, out Vector2 pos)) OnItemDropped(pickUp, s, pos);
            });
        }

        private void ItemPickUp__Init__Prefix(Vector2 pos, Item item, ref ItemPickUp __instance)
        {
            DroppedItems.Add(__instance, pos);
        }

        private void ItemPickUp__OnDestroy__Prefix(ref ItemPickUp __instance)
        {
            if (!DroppedItems.ContainsKey(__instance)) return;
            // MelonLogger.Msg($"Removed item at {MoreQOD.DroppedItems[__instance]}");
            DroppedItems.Remove(__instance);
            OnItemCollected(__instance);
        }

        // [HarmonyPatch(typeof(GUI_FloatingItemName))]
        // internal class GUI_FloatingItemNamePatch
        // {
        //     [HarmonyPrefix]
        //     [HarmonyPatch(nameof(GUI_FloatingItemName.Init), typeof(ItemPickUp))]
        //     private static void PRE_Init(ItemPickUp pickUp, ref LocalizedItemName ____localizedName,
        //         ref GUI_FloatingItemName __instance)
        //     {
        //         ____localizedName.OnChangeEv.AddListener(s =>
        //         {
        //             if (DroppedItems.TryGetValue(pickUp, out Vector2 pos)) OnItemDropped(pickUp, s, pos);
        //         });
        //     }
        // }

        // [HarmonyPatch(typeof(ItemPickUp))]
        // internal class ItemPickUpPatch
        // {
        //     [HarmonyPrefix]
        //     [HarmonyPatch(nameof(ItemPickUp.Init), typeof(Vector2), typeof(Item))]
        //     private static void PRE_Init(Vector2 pos, Item item, ref ItemPickUp __instance)
        //     {
        //         DroppedItems.Add(__instance, pos);
        //     }
        //
        //     [HarmonyPrefix]
        //     [HarmonyPatch("OnDestroy")]
        //     private static void PRE_OnDestroy(ref ItemPickUp __instance)
        //     {
        //         if (!DroppedItems.ContainsKey(__instance)) return;
        //         // MelonLogger.Msg($"Removed item at {MoreQOD.DroppedItems[__instance]}");
        //         DroppedItems.Remove(__instance);
        //         OnItemCollected(__instance);
        //     }
        // }
    }
}
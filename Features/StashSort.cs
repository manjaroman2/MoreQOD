using System;
using System.Collections.Generic;
using System.Linq;
using Death.App;
using Death.Items;
using Death.Run.UserInterface.Items;
using HarmonyLib;
using MelonLoader;

namespace MoreQOD
{
    public class StashSort : Feature, Hookable
    {
        private ItemController_Stash controller;

        public StashSort(bool enabled = true) : base(enabled)
        {
        }

        public void addHarmonyHooks()
        {
            MoreQOD.Instance.HarmonyInstance.Patch(
                typeof(GUI_ItemManager_Stash).GetMethod(nameof(GUI_ItemManager_Stash.Init), AccessTools.all, null,
                    new[] { typeof(Profile) }, null),
                postfix: new HarmonyMethod(typeof(StashSort).GetMethod(nameof(GUI_ItemManager_Stash__Init__Postfix),
                    AccessTools.all)));
        }

        private static ulong getRank(Item item)
        {
            // This method creates an absolute ordering (hopefully)  
            ulong rank = 0b_0000000000000000000000000000000000000000000000000000000000000000;

            ulong mask = 0b_1000000000000000000000000000000000000000000000000000000000000000;
            int bitsLeft = 64; 

            //  - 1 bit for uniqueness
            if (item.IsUnique) rank |= mask;
            mask >>= 1;
            bitsLeft -= 1; 

            //  - 6 bits for rarity
            rank |= mask >> ((int)ItemRarity._Count - (int)item.Rarity);
            mask >>= (int)ItemRarity._Count;
            bitsLeft -= (int)ItemRarity._Count; 

            //  - 11 bits for type
            rank |= mask >> (int)item.Type;
            mask >>= (int)ItemType._Count;
            bitsLeft -= (int)ItemType._Count; 

            //  - 46 bits for lowercase chars + [A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T] in subtype  


            bitsLeft -= 26;
            foreach (int c in item.SubtypeCode)
            {
                if (c >= 65 && c <= 65 + bitsLeft) rank ^= mask >> (c - 65);
                else if (c is > 96 and < 123) rank ^= mask >> (c - 97 + bitsLeft);
            }
            return rank;
        }

        // QuickSort 
        public static void SortArrayInPlace(ulong[] array, int leftIndex, int rightIndex)
        {
            int i = leftIndex;
            int j = rightIndex;
            ulong pivot = array[leftIndex];
            while (i <= j)
            {
                while (array[i] < pivot) i++;

                while (array[j] > pivot) j--;
                if (i <= j)
                {
                    (array[i], array[j]) = (array[j], array[i]);
                    i++;
                    j--;
                }
            }

            if (leftIndex < j)
                SortArrayInPlace(array, leftIndex, j);
            if (i < rightIndex)
                SortArrayInPlace(array, i, rightIndex);
        }

        private static void sortItemGrid(ItemGrid itemGrid)
        {
            Dictionary<ulong, List<Item>> ItemRank = new();

            foreach (Item item in itemGrid.GetItems())
            {
                ulong rank = getRank(item);
                if (!ItemRank.ContainsKey(rank)) ItemRank[rank] = new List<Item>();
                ItemRank[rank].Add(item);
            }

            ulong[] A = new List<ulong>(ItemRank.Keys).ToArray();
            // foreach ((ulong rank, List<Item> items) in ItemRank)
            //     MelonLogger.Msg(
            //         $"rank={rank} items={string.Join(", ", items.Select((item, idx) => $"{item.Type} {item.SubtypeCode}"))}");
            // MelonLogger.Msg(A + "");
            // MelonLogger.Msg(string.Join(", ", A));
            if (A.Length < 2) return;
            SortArrayInPlace(A, 0, A.Length - 1);
            Array.Reverse(A);

            itemGrid.Clear();
            int i = 0;
            foreach (ulong rank in A)
            foreach (Item item in ItemRank[rank])
            {
                int y = i / itemGrid.Width;
                int x = i % itemGrid.Width;
                itemGrid.Set(x, y, item);
                i++;
            }
        }

        public void sortSelectedPage()
        {
            if (controller == null) return;
            MelonLogger.Msg("Selected Page=" + controller.SelectedPage);
            sortItemGrid(controller.SelectedPage);
        }

        private static void GUI_ItemManager_Stash__Init__Postfix(Profile profile,
            ref ItemController_Stash ____controller)
        {
            // MelonLogger.Msg("GUI_ItemManager_Stash__Init_Prefix");
            MoreQOD.Instance.StashSort.controller = ____controller;
        }
    }
}
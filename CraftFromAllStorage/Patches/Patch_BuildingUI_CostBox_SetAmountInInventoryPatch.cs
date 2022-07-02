using HarmonyLib;
using System.Collections.Generic;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Patches
{
    /// <summary>
    /// This renders the amount we have when holding the hammer
    /// </summary>
    [HarmonyPatch(typeof(BuildingUI_CostBox), nameof(BuildingUI_CostBox.SetAmountInInventory), typeof(PlayerInventory), typeof(bool))]
    class Patch_BuildingUI_CostBox_SetAmountInInventoryPatch
    {
        static void Postfix(BuildingUI_CostBox __instance, PlayerInventory inventory, bool includeSecondaryInventory)
        {
            var isPlayerInventory = inventory is PlayerInventory;

            if (!inventory || !isPlayerInventory)
            {
                return;
            }

            if (!CraftFromStorageManager.HasUnlimitedResources())
            {
                var playerInventoryAndStorageAmount = 0;

                List<Item_Base> items = CraftFromStorageManager.getItemsFromCostBox(__instance);

                //Debug.Log($"BuildingUI_CostBox.SetAmountInInventory includeSecondaryInventory {includeSecondaryInventory} ---------------- {items.Count} items");

                foreach (var costBoxItem in items)
                {
                    playerInventoryAndStorageAmount += inventory.GetItemCount(costBoxItem); // This includes storages, because we patch PlayerInventory.GetItemCount
                    //Debug.Log($"{costBoxItem.name} player and storage amount {playerInventoryAndStorageAmount}");
                }

                __instance.SetAmount(playerInventoryAndStorageAmount);
            }
        }
    }
}
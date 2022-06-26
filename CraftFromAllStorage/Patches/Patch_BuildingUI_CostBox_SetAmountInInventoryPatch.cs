using HarmonyLib;
using System.Collections.Generic;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Patches
{
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
                //Debug.Log($"BuildingUI_CostBox.SetAmountInInventory includeSecondaryInventory {includeSecondaryInventory}");
                var playerInventoryAmount = 0;

                List<Item_Base> items = CraftFromStorageManager.getItemsFromCostBox(__instance);

                //var currentStorageInventory = InventoryManager.GetCurrentStorageInventory();

                int storageInventoryAmount = 0;
                foreach (var costBoxItem in items)
                {
                    playerInventoryAmount += inventory.GetItemCount(costBoxItem);
                    //Debug.Log($"player {inventory.name} {playerInventoryAmount}");

                    if (inventory.secondInventory != null)
                    {
                        storageInventoryAmount += inventory.secondInventory.GetItemCountWithoutDuplicates(costBoxItem.UniqueName);
                        //Debug.Log($"open storage {inventory.secondInventory.name} {storageInventoryAmount}");
                    }

                    foreach (Storage_Small storage in StorageManager.allStorages)
                    {
                        if (storage.IsExcludeFromCraftFromAllStorage())
                        {
                            continue;
                        }

                        Inventory container = storage.GetInventoryReference();

                        //var localPlayerIsWithinDistance = Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage);
                        if (storage.IsOpen || container == null /*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                        {
                            continue;
                        }

                        if (inventory == container || inventory.secondInventory == container)
                        {
                            Debug.Log($"{container.name} being skipped, it is player inventory or secondary inventory.");
                        }

                        //var amount = container.GetItemCount(costBoxItem);
                        var amount = container.GetItemCountWithoutDuplicates(costBoxItem.UniqueName);
                        //Debug.Log($"chest storage {container.name} {amount}");
                        storageInventoryAmount += amount;
                    }
                }

                __instance.SetAmount(playerInventoryAmount + storageInventoryAmount);
            }
        }
    }
}
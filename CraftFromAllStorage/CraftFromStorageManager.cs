using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

class CraftFromStorageManager
{
    static public bool HasUnlimitedResources()
    {
        return GameModeValueManager.GetCurrentGameModeValue().playerSpecificVariables.unlimitedResources;
    }

    /// <summary>
    /// costBox.items is protected on BuildingUI_CostBox, so we use harmony to get access
    /// </summary>
    /// <param name="costBox"></param>
    /// <returns></returns>
    static public List<Item_Base> getItemsFromCostBox(BuildingUI_CostBox costBox)
    {
        return Traverse.Create(costBox).Field("items").GetValue<List<Item_Base>>();
    }

    static public void RemoveCostMultiple(CostMultiple[] costMultipleArray, bool manipulateCostAmount = false)
    {
        if (!CraftFromStorageManager.HasUnlimitedResources())
        {
            ////Debug.Log("Getting player");
            ////var player = RAPI.GetLocalPlayer();

            //Debug.Log("Getting player inventory");
            Inventory playerInventory = InventoryManager.GetPlayerInventory();

            //Debug.Log("Getting storage inventory");
            //Inventory storageInventory = InventoryManager.GetCurrentStorageInventory();
            Inventory storageInventory = playerInventory.secondInventory;

            // Remove items from player inventory, then the open storage, then other chests.
            foreach (var costMultiple in costMultipleArray)
            {
                var remainingAmount = costMultiple.amount;

                if (remainingAmount <= 0)
                {
                    continue;
                }

                foreach (var item in costMultiple.items)
                {
                    Debug.Log($"Preparing to remove {remainingAmount} {item.UniqueName} from player inventory");
                    // Handle Player inventory
                    remainingAmount = RemoveItemFromInventory(item, playerInventory, remainingAmount);

                    if (remainingAmount <= 0)
                    {
                        Debug.Log($"no more {item.UniqueName} to be removed.");
                        continue;
                    }

                    Debug.Log($"Preparing to remove {remainingAmount} {item.UniqueName} from currently open storage");
                    // Handle Current Opened Storage
                    remainingAmount = RemoveItemFromInventory(item, storageInventory, remainingAmount);


                    if (remainingAmount <= 0)
                    {
                        Debug.Log($"no more {item.UniqueName} to be removed.");
                        continue;
                    }

                    Debug.Log($"Preparing to remove {remainingAmount} {item.UniqueName} from all other storages");
                    // Handle all other containers
                    foreach (Storage_Small storage in StorageManager.allStorages)
                    {
                        Inventory container = storage.GetInventoryReference();
                        ////var localPlayerWithinDistance = Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage);
                        if (container == playerInventory || container == storageInventory || storage.IsOpen || container == null /*|| !localPlayerWithinDistance*/)
                        {
                            continue;
                        }

                        remainingAmount = RemoveItemFromInventory(item, container, remainingAmount);

                        // We close the storage to sync changes to other players
                        storage.BroadcastCloseEvent();

                        if (remainingAmount <= 0)
                        {
                            Debug.Log($"no more {item.UniqueName} to be removed.");
                            break;
                        }
                    }
                }

                if (manipulateCostAmount)
                {
                    costMultiple.amount -= costMultiple.amount - remainingAmount;
                }
            }

            Debug.Log($"all resources where removed.");
        }
    }

    private static int RemoveItemFromInventory(Item_Base item, Inventory inventory, int remainingAmount)
    {
        // Handle when current storage is null
        if (inventory == null || item == null)
        {
            return remainingAmount;
        }

        var inventoryAmount = inventory.GetItemCount(item.UniqueName);
        Debug.Log($"inventory has {inventoryAmount} {item.UniqueName}");
        int amountToRemove = Math.Min(remainingAmount, inventoryAmount);
        if (amountToRemove > 0)
        {
            Debug.Log($"Preparing to remove {amountToRemove} {item.UniqueName}");
            inventory.RemoveItem(item.UniqueName, amountToRemove);
            Debug.Log($"{amountToRemove} {item.UniqueName} was removed");
            return remainingAmount - amountToRemove;
        }

        return remainingAmount;
    }
}

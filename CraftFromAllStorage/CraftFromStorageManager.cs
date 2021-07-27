using HarmonyLib;
using System;
using System.Collections.Generic;

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

    static public void RemoveCostMultiple(CostMultiple[] costMultipleArray)
    {
        if (!CraftFromStorageManager.HasUnlimitedResources())
        {
            Inventory playerInventory = InventoryManager.GetPlayerInventory();

            Inventory storageInventory = InventoryManager.GetCurrentStorageInventory();

            // Remove items from player inventory, then the open storage, then other chests.

            foreach (var costMultiple in costMultipleArray)
            {
                var remainingAmount = costMultiple.amount;

                foreach (var item in costMultiple.items)
                {
                    // Handle Player inventory
                    remainingAmount = RemoveItemFromInventory(item, playerInventory, remainingAmount);

                    if (remainingAmount <= 0)
                    {
                        continue;
                    }

                    // Handle Current Opened Storage
                    remainingAmount = RemoveItemFromInventory(item, storageInventory, remainingAmount);
                    

                    if (remainingAmount <= 0)
                    {
                        continue;
                    }

                    // Handle all other containers
                    foreach (Storage_Small storage in StorageManager.allStorages)
                    {
                        Inventory container = storage.GetInventoryReference();
                        if (container == playerInventory || container == storageInventory || storage.IsOpen || container == null /*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                        {
                            continue;
                        }

                        remainingAmount = RemoveItemFromInventory(item, container, remainingAmount);
                        // We close the storage to sync changes to other players
                        storage.BroadcastCloseEvent();

                        if (remainingAmount <= 0)
                        {
                            break;
                        }
                    }
                }
            }
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
        int amountToRemove = Math.Min(remainingAmount, inventoryAmount);
        if (amountToRemove > 0)
        {
            inventory.RemoveItem(item.UniqueName, amountToRemove);
            return remainingAmount - amountToRemove;
        }

        return remainingAmount;
    }
}

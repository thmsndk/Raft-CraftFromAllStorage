﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


[HarmonyPatch(typeof(CostMultiple), "HasEnoughInInventory")]
class HasEnoughInInventoryPatch
{
    static bool Postfix(bool __result, CostMultiple __instance, Inventory inventory)
    {
        // player inventory should already have been checked
        var enoughInPlayerInventory = __result;

        if (!CraftFromStorageManager.HasUnlimitedResources() && !enoughInPlayerInventory)
        {
            // verify if current open storage has enough items andd return early
            var currentStorageInventory = InventoryManager.GetCurrentStorageInventory();

            if (currentStorageInventory != null && currentStorageInventory == inventory)
            {
                // We potentially call this patch recursively with the above check, thus we bail out.
                //Debug.Log("current storage and inventory we are checking are the same, bail out.");
                return __result;
            }
            
            if (currentStorageInventory != null && __instance.HasEnoughInInventory(currentStorageInventory))
            {
                //Debug.Log($"current storage has enough: {inventory.GetInstanceID()} {currentStorageInventory?.GetInstanceID()}");
                return true;
            }

            // currentstorage does not have enough items, or none
            int num = 0;
            foreach (var costMultipleItems in __instance.items)
            {
                foreach (Storage_Small storage in StorageManager.allStorages)
                {
                    Inventory container = storage.GetInventoryReference();
                    if (storage.IsOpen || container == null /*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                        continue;

                    num += container.GetItemCountWithoutDuplicates(costMultipleItems.UniqueName);

                    if (num >= __instance.amount)
                    {
                        // bail out early so we don't check ALL storage
                        return true;
                    }
                }
            }

            return num >= __instance.amount;
        }
        return __result;
    }
}

[HarmonyPatch(typeof(BuildingUI_CostBox), "SetAmountInInventory", typeof(PlayerInventory), typeof(bool))]
//[HarmonyPatch(typeof(BuildingUI_CostBox), "SetAmountInInventory")]
class SetAmountInInventoryPatch
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
                    Inventory container = storage.GetInventoryReference();

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

//// TODO: there seems to be some wrong crafting with quick crafting of bolts... needs to investigate.
///*
// * Attempting to craft 2/6 nails crashes the client to desktop, possibly because I only have 3/2 scrap?
// */

[HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveCostMultiple), typeof(CostMultiple[]), typeof(bool))]
class RemoveCostMultiple
{
    static bool Prefix(CostMultiple[] costMultiple, bool manipulateCostAmount = false)
    {
        // RemoveCostMultipleIncludeSecondaryInventories calls this twice if a secondary inventory (chest) is opened.
        // So it is important to manipulate the cost amount to prevent resources getting removed multiple times
        CraftFromStorageManager.RemoveCostMultiple(costMultiple, manipulateCostAmount);
        return false; // Prevent the original RemoveCostMultiple from running.
    }
}

// Patches for Auto Recipe Redux support

/// <summary>
/// This needs to be overriden to indicate we have enough resources
/// </summary>
[HarmonyPatch(typeof(PlayerInventory), "GetItemCount", typeof(Item_Base))]
class GetItemCount
{
    static void Postfix(PlayerInventory __instance, ref int __result, Item_Base item)
    {
        var isPlayerInventory = __instance is PlayerInventory;
        if (!__instance || !isPlayerInventory)
        {
            return;
        }

        //// https://stackoverflow.com/a/615976/28145
        //System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        //MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
        //Debug.Log("GetItemCount: " + methodBase.);

        // Handle both fuel counts and item counts from AutoRecipeBehaviour.CalculatePreparation (colors resources red if missing)
        if (!Environment.StackTrace.Contains("at AutoRecipeBehaviour.OnIsRayed"))
        {
            return;
        }

        // Append item count of storages to original GetItemCount
        // Append item count of storages to original GetItemCount, Odds are you don't have a storage open when looking at the recipe.
        if (!CraftFromStorageManager.HasUnlimitedResources())
        {
            foreach (Storage_Small storage in StorageManager.allStorages)
            {
                Inventory container = storage.GetInventoryReference();
                if (storage.IsOpen || container == null /*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                    continue;

                __result += container.GetItemCountWithoutDuplicates(item.UniqueName);
            }
        }
    }
}

/// <summary>
/// Patch RemoveItem so fuel can be removed from storage as well as the players inventory.
/// this.cookingPot.localPlayer.Inventory.RemoveItem(this.cookingPot.Fuel.fuelItem.UniqueName, fuel);
/// </summary>
[HarmonyPatch(typeof(PlayerInventory), "RemoveItem", typeof(string), typeof(int))]
class RemoveItem
{
    static bool Prefix(PlayerInventory __instance, string uniqueItemName, int amount)
    {
        if (!Environment.StackTrace.Contains("at AutoRecipeBehaviour.OnIsRayed"))
        {
            return true;
        }

        var isPlayerInventory = __instance is PlayerInventory;
        if (!__instance || !isPlayerInventory)
        {
            return true;
        }

        // Append item count of storages to original GetItemCount
        if (!CraftFromStorageManager.HasUnlimitedResources())
        {
            // Get count in player inventory
            var playerInventoryCount = __instance.GetItemCount(uniqueItemName); // we have only patched the overload that uses the item, so this should not trigger our patch

            var amountToRemoveFromPlayerInventory = amount - Math.Min(playerInventoryCount, amount);
            var amountToRemove = amountToRemoveFromPlayerInventory;

            if (amountToRemove <= 0)
            {
                return true;
            }

            foreach (Storage_Small storage in StorageManager.allStorages)
            {
                Inventory container = storage.GetInventoryReference();
                if (storage.IsOpen || container == null || container == __instance /*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                    continue;

                var containerItemCount = container.GetItemCountWithoutDuplicates(uniqueItemName);
                var amountToRemoveFromContainer = Math.Min(containerItemCount, amountToRemove);

                container.RemoveItemUses(uniqueItemName, amountToRemoveFromContainer, false);
                amountToRemove -= amountToRemoveFromContainer;

                if (amountToRemove <= 0)
                {
                    // All items have been removed.
                    break;
                }
            }

            return amountToRemoveFromPlayerInventory > 0; // only run the original method if player inventory has something to remove.
        }

        return true;
    }
}

/// <summary>
/// Allow Inserting item to CookingTable_Slot to pull from player inventory as well as other storages.
/// </summary>
[HarmonyPatch(typeof(CookingTable_Slot), "InsertItem")]
class InsertItem
{
    static bool Prefix(CookingTable_Slot __instance,
                       Network_Player player,
                       ItemInstance itemInstance,
                       ref ItemInstance ___currentItem, // private field
                       ItemObjectEnabler ___objectEnabler, // private field
                       CookingTable ___cookingPot) // private field
    {
        ___currentItem = itemInstance.Clone();
        ___currentItem.Amount = 1;
        ___objectEnabler.ActivateModel(___currentItem.baseItem);
        if (player != null && player.IsLocalPlayer)
        {
            var remainingUses = itemInstance.Uses;
            var playerInventoryCount = player.Inventory.GetItemCount(itemInstance.UniqueName);

            var amountToRemoveFromPlayerInventory = Math.Min(playerInventoryCount, remainingUses);

            player.Inventory.RemoveItemUses(itemInstance.UniqueName, amountToRemoveFromPlayerInventory, false);
            remainingUses -= amountToRemoveFromPlayerInventory;

            if (remainingUses <= 0)
            {
                return false;
            }

            foreach (Storage_Small storage in StorageManager.allStorages)
            {
                Inventory container = storage.GetInventoryReference();
                if (storage.IsOpen || container == null /*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                {
                    continue;
                }

                var containerItemCount = container.GetItemCountWithoutDuplicates(itemInstance.UniqueName);
                var amountToRemoveFromContainer = Math.Min(containerItemCount, remainingUses);

                container.RemoveItemUses(itemInstance.UniqueName, amountToRemoveFromContainer, false);
                remainingUses -= amountToRemoveFromContainer;

                if (remainingUses <= 0)
                {
                    // All items have been removed.
                    break;
                }
            }

            ___cookingPot.displayText.HideDisplayTexts();
            player.Animator.SetAnimation(PlayerAnimation.Trigger_Plant, false);
        }

        return false;
    }
}

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Patches for Auto Recipe Redux support

/// <summary>
/// This needs to be overriden to indicate we have enough resources
/// </summary>
[HarmonyPatch(typeof(Inventory), nameof(Inventory.GetItemCount), typeof(Item_Base))]
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
[HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), typeof(string), typeof(int))]
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

            var amountToRemoveFromPlayerInventory = Math.Min(playerInventoryCount, amount);
            var remainingAmountToRemoveFromStorage = amount - amountToRemoveFromPlayerInventory;

            if (remainingAmountToRemoveFromStorage <= 0)
            {
                return true;
            }

            foreach (Storage_Small storage in StorageManager.allStorages)
            {
                Inventory container = storage.GetInventoryReference();
                if (storage.IsOpen || container == null || container == __instance /*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                    continue;

                var containerItemCount = container.GetItemCountWithoutDuplicates(uniqueItemName);
                var amountToRemoveFromContainer = Math.Min(containerItemCount, remainingAmountToRemoveFromStorage);

                container.RemoveItemUses(uniqueItemName, amountToRemoveFromContainer, false);
                remainingAmountToRemoveFromStorage -= amountToRemoveFromContainer;

                if (remainingAmountToRemoveFromStorage <= 0)
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
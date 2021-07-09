using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
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
            if (currentStorageInventory != null && __instance.HasEnoughInInventory(currentStorageInventory))
            {
                //Debug.Log($"current storage has enough: {inventory.GetInstanceID()} {currentStorageInventory?.GetInstanceID()}");
                return true;
            }

            Network_Player player = RAPI.GetLocalPlayer();

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
                }
            }

            return num >= __instance.amount;
        }
        return __result;
    }
}

[HarmonyPatch(typeof(BuildingUI_CostBox), "SetAmountInInventory")]
class SetAmountInInventoryPatch
{
    static void Postfix(PlayerInventory inventory, BuildingUI_CostBox __instance)
    {
        if (!CraftFromStorageManager.HasUnlimitedResources())
        {
            var playerInventoryAmount = __instance.GetAmount();

            List<Item_Base> items = CraftFromStorageManager.getItemsFromCostBox(__instance);

            var currentStorageInventory = InventoryManager.GetCurrentStorageInventory();
            
            int storageInventoryAmount = 0;
            foreach (var costBoxItem in items)
            {
                foreach (Storage_Small storage in StorageManager.allStorages)
                {
                    Inventory container = storage.GetInventoryReference();

                    bool isOpenByAnotherPlayer = (storage.IsOpen && currentStorageInventory != container);
                    if (isOpenByAnotherPlayer || container == null /*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                    {
                        continue;
                    }

                    storageInventoryAmount += container.GetItemCountWithoutDuplicates(costBoxItem.UniqueName);
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

[HarmonyPatch(typeof(PlayerInventory), "RemoveCostMultiple")]
class RemoveCost
{
    static bool Prefix(CostMultiple[] costMultiple)
    {
        CraftFromStorageManager.RemoveCostMultiple(costMultiple);
        return false; // Prevent the original RemoveCostMultiple from running.
    }
}

class InventoryManager
{
    static public Inventory GetPlayerInventory()
    {
        return RAPI.GetLocalPlayer().Inventory;
    }

    /// <summary>
    /// Gets the currently open storage inventory for the player
    /// </summary>
    /// <returns></returns>
    static public Inventory GetCurrentStorageInventory()
    {
        return RAPI.GetLocalPlayer().StorageManager.currentStorage?.GetInventoryReference();
    }
}

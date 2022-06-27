using HarmonyLib;
using thmsn.CraftFromAllStorage.Network;

namespace thmsn.CraftFromAllStorage.Patches
{
    /// <summary>
    /// 
    /// </summary>
    [HarmonyPatch(typeof(CostMultiple), nameof(CostMultiple.HasEnoughInInventory))]
    class Patch_CostMultiple_HasEnoughInInventory
    {
        static bool Postfix(bool __result, CostMultiple __instance, Inventory inventory)
        {
            var isPlayerInventory = inventory is PlayerInventory;
            if (!inventory || !isPlayerInventory)
            {
                return true;
            }

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
                        if (storage.IsExcludeFromCraftFromAllStorage())
                        {
                            continue;
                        }

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
}
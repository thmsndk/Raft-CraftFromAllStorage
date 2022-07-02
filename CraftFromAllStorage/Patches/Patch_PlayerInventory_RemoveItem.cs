using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Patches
{

    /* The flow of storage, OnIsRayed triggers a Message_Storage when you interact with the storage, 
     * if you are the host, it also calls this.storageManager.OpenStorage(this); causing your inventory to open.
     */

    /// <summary>
    /// Patch RemoveItem so fuel can be removed from storage as well as the players inventory.
    /// this.cookingPot.localPlayer.Inventory.RemoveItem(this.cookingPot.Fuel.fuelItem.UniqueName, fuel);
    /// </summary>
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), typeof(string), typeof(int))]
    class Patch_PlayerInventory_RemoveItem
    {
        static bool Prefix(Inventory __instance, string uniqueItemName, int amount)
        {
            var isPlayerInventory = __instance is PlayerInventory;
            if (!__instance || !isPlayerInventory)
            {
                Debug.Log("Inventory.RemoveItem called, running original method.");
                return true;
            }

            //if (!Environment.StackTrace.Contains("at AutoRecipeBehaviour.OnIsRayed"))
            //{
            //    return true;
            //}

            // Append item count of storages to original GetItemCount
            if (!CraftFromStorageManager.HasUnlimitedResources())
            {
                // Get count in player inventory
                var playerInventoryCount = __instance.GetItemCount(uniqueItemName); // we have only patched the overload that uses the item, so this should not trigger our patch
                var amountToRemoveFromPlayerInventory = Math.Min(playerInventoryCount, amount);
                var remainingAmountToRemoveFromStorage = amount - amountToRemoveFromPlayerInventory;
                Debug.Log($"{uniqueItemName} {playerInventoryCount} in inventory, gonna remove {amountToRemoveFromPlayerInventory}, need to remove {remainingAmountToRemoveFromStorage} from storage.");

                if (remainingAmountToRemoveFromStorage <= 0)
                {
                    Debug.Log($"{uniqueItemName} has {playerInventoryCount}, removing {amountToRemoveFromPlayerInventory}");
                    return true;
                }

                foreach (Storage_Small storage in StorageManager.allStorages)
                {
                    if (storage.IsExcludeFromCraftFromAllStorage())
                    {
                        continue;
                    }

                    Inventory container = storage.GetInventoryReference();
                    if (storage.IsOpen || container == null || container == __instance /*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                        continue;

                    var containerItemCount = container.GetItemCountWithoutDuplicates(uniqueItemName);

                    if (containerItemCount > 0)
                    {
                        // TODO: should we broadcast an open event? would be cool to visualize to other players when a storage opens?
                        var player = RAPI.GetLocalPlayer();
                        var network = ComponentManager<Raft_Network>.Value;
                        if (Raft_Network.IsHost)
                        {
                            Debug.Log("'We are the host");
                            network.RPC(new Message_Storage(Messages.StorageManager_Open, player.StorageManager, storage), Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
                            //player.storageManager.OpenStorage(this); // this sets second inventory, if it's a local player. also calls storage.Open
                            storage.Open(player);  // This feels important, we might need to introduce it everywhere
                        }
                        else
                        {
                            network.SendP2P(network.HostID, new Message_Storage(Messages.StorageManager_Open, player.StorageManager, storage), EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
                        }

                        var amountToRemoveFromContainer = Math.Min(containerItemCount, remainingAmountToRemoveFromStorage);
                        Debug.Log($"{container.name} {uniqueItemName} {containerItemCount}, removing {amountToRemoveFromContainer}");
                        container.RemoveItem(uniqueItemName, amountToRemoveFromContainer);
                        remainingAmountToRemoveFromStorage -= amountToRemoveFromContainer;

                        // We close the storage to sync changes to other players
                        storage.BroadcastCloseEvent();
                        storage.Close(player); // This feels important, we might need to introduce it everywhere
                    }

                    if (remainingAmountToRemoveFromStorage <= 0)
                    {
                        // All items have been removed.
                        break;
                    }
                }

                var runOriginalMethod = amountToRemoveFromPlayerInventory > 0; // only run the original method if player inventory has something to remove.;
                if (runOriginalMethod)
                {
                    Debug.Log($"running original method to remove {amountToRemoveFromPlayerInventory} from player inventory.");
                }else
                {
                    Debug.Log("Not running the original method");
                }
                return runOriginalMethod;
            }

            Debug.Log($"running original method");

            return true;
        }
    }
}
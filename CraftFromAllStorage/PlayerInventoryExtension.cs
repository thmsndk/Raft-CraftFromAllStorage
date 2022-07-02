using Steamworks;
using System;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;

namespace thmsn.CraftFromAllStorage
{
    public static class PlayerInventoryExtension
    {
        static public bool RemoveItemFromInventoryAndStoragesOnRaft(this PlayerInventory playerInventory, string uniqueItemName, int amount)
        {
            if (!CraftFromStorageManager.HasUnlimitedResources())
            {
                // Get count in player inventory
                var playerInventoryCount = playerInventory.GetItemCount(uniqueItemName); // we have only patched the overload that uses the item, so this should not trigger our patch
                var amountToRemoveFromPlayerInventory = Math.Min(playerInventoryCount, amount);
                var remainingAmountToRemoveFromStorage = amount - amountToRemoveFromPlayerInventory;
                Debug.Log($"{uniqueItemName} {playerInventoryCount} in inventory, gonna remove {amountToRemoveFromPlayerInventory}, need to remove {remainingAmountToRemoveFromStorage} from storage.");

                if (remainingAmountToRemoveFromStorage <= 0)
                {
                    Debug.Log($"{uniqueItemName} has {playerInventoryCount}, removing {amountToRemoveFromPlayerInventory}");
                    return true;
                }

                // TODO: remove from currently open storage. as a priorty, we don't need to broadcast that because it is currently open.
                Inventory storageInventory = playerInventory.secondInventory;

                if (storageInventory != null)
                {
                    var containerItemCount = storageInventory.GetItemCountWithoutDuplicates(uniqueItemName);
                    var amountToRemoveFromContainer = Math.Min(containerItemCount, remainingAmountToRemoveFromStorage);
                    Debug.Log($"{storageInventory.name} {uniqueItemName} {containerItemCount}, removing {amountToRemoveFromContainer}");
                    storageInventory.RemoveItem(uniqueItemName, amountToRemoveFromContainer);
                    remainingAmountToRemoveFromStorage -= amountToRemoveFromContainer;
                }

                foreach (Storage_Small storage in StorageManager.allStorages)
                {
                    if (storage.IsExcludeFromCraftFromAllStorage())
                    {
                        continue;
                    }

                    Inventory container = storage.GetInventoryReference();
                    if (storage.IsOpen || container == null || container == playerInventory || container == storageInventory/*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                        continue;

                    var containerItemCount = container.GetItemCountWithoutDuplicates(uniqueItemName);

                    if (containerItemCount > 0)
                    {
                        // TODO: should we broadcast an open event? would be cool to visualize to other players when a storage opens?
                        //var player = RAPI.GetLocalPlayer();
                        //var network = ComponentManager<Raft_Network>.Value;
                        //if (Raft_Network.IsHost)
                        //{
                        //    Debug.Log("'We are the host");
                        //    network.RPC(new Message_Storage(Messages.StorageManager_Open, player.StorageManager, storage), Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
                        //    //player.storageManager.OpenStorage(this); // this sets second inventory, if it's a local player. also calls storage.Open
                        //    storage.Open(player);  // This feels important, we might need to introduce it everywhere
                        //}
                        //else
                        //{
                        //    Debug.Log("Sending StorageManager_Open to host.");
                        //    network.SendP2P(network.HostID, new Message_Storage(Messages.StorageManager_Open, player.StorageManager, storage), EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
                        //}

                        var amountToRemoveFromContainer = Math.Min(containerItemCount, remainingAmountToRemoveFromStorage);
                        Debug.Log($"{container.name} {uniqueItemName} {containerItemCount}, removing {amountToRemoveFromContainer}");
                        container.RemoveItem(uniqueItemName, amountToRemoveFromContainer);
                        remainingAmountToRemoveFromStorage -= amountToRemoveFromContainer;

                        // We close the storage to sync changes to other players, we only need to do that if it is not is a secondary open storage, should trigger a close event natually
                        storage.BroadcastCloseEvent();
                        //storage.Close(player); // This feels important, we might need to introduce it everywhere
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
                }
                else
                {
                    Debug.Log("Not running the original method");
                }
                return runOriginalMethod;
            }

            return true;
        }
    }
}
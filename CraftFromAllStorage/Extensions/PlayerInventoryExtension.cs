using FMODUnity;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Extensions
{
    public class ItemNameAndAmount
    {
        public string UniqueName { get; set; }
        public int Amount { get; set; }

        public ItemNameAndAmount(string uniqueName, int amount)
        {
            this.UniqueName = uniqueName;
            this.Amount = amount;
        }
    }

    public static class PlayerInventoryExtension
    {

        /// <summary>
        /// Removes items from Player Inventory > Open Storage > Storages on raft
        /// </summary>
        /// <param name="playerInventory"></param>
        /// <param name="itemsAndAmountToRemove"></param>
        /// <param name="removeFromPlayerInventory"></param>
        static public void RemoveItemFromPlayerInventoryAndStoragesOnRaft(this PlayerInventory playerInventory, IEnumerable<ItemNameAndAmount> itemsAndAmountToRemove, bool removeFromPlayerInventory = true)
        {
            if (!CraftFromStorageManager.HasUnlimitedResources())
            {
                var player = RAPI.GetLocalPlayer();
                Inventory storageInventory = playerInventory.secondInventory;

                // Handle player inventory
                foreach (var item in itemsAndAmountToRemove)
                {
                    var playerInventoryCount = playerInventory.GetItemCount(item.UniqueName); // we have only patched the overload that uses the item, so this should only get the itemcount for the player inventory.
                    var amountToRemoveFromPlayerInventory = Math.Min(playerInventoryCount, item.Amount);
                    item.Amount -= amountToRemoveFromPlayerInventory;

                    if (amountToRemoveFromPlayerInventory > 0)
                    {
                        Debug.Log($"{playerInventory.name} {item.UniqueName} {playerInventoryCount}, removing {amountToRemoveFromPlayerInventory}");

                        if (removeFromPlayerInventory)
                        {
                            CraftFromStorageManager.RemoveItem(playerInventory, item.UniqueName, amountToRemoveFromPlayerInventory);
                        }
                    }

                    if (item.Amount <= 0)
                    {
                        continue;
                    }

                    // Handle open storage
                    if (storageInventory != null)
                    {
                        var containerItemCount = storageInventory.GetItemCountWithoutDuplicates(item.UniqueName);
                        var amountToRemoveFromContainer = Math.Min(containerItemCount, item.Amount);
                        if (amountToRemoveFromContainer > 0)
                        {
                            Debug.Log($"open storage {storageInventory.name} {item.UniqueName} {containerItemCount}, removing {amountToRemoveFromContainer}");

                            // We have only patched RemoveItem if it is a player inventory, so this should be fine.
                            storageInventory.RemoveItem(item.UniqueName, amountToRemoveFromContainer);
                            item.Amount -= amountToRemoveFromPlayerInventory;
                        }
                    }
                }

                // bail out, we removed everything from inventory or open storage
                var allItemsHasBeenRemoved = itemsAndAmountToRemove.All(x => x.Amount <= 0);
                if (allItemsHasBeenRemoved)
                {
                    return;
                }

                // Handle other storages.
                // Both Player Inventory and currently open storage should sync to other players when the player closes their inventory.
                foreach (Storage_Small storage in StorageManager.allStorages)
                {
                    if (storage.IsExcludeFromCraftFromAllStorage())
                    {
                        continue;
                    }

                    // TODO: max distance validation for immersion, e.g. crafting on the other end of an island. needs to support extrasettings
                    ////var localPlayerWithinDistance = Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage);

                    Inventory remoteStorageInventory = storage.GetInventoryReference();
                    if (storage.IsOpen || remoteStorageInventory == null || remoteStorageInventory == playerInventory || remoteStorageInventory == storageInventory/*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                    {
                        continue;
                    }

                    var wasOpened = false;
                    foreach (var item in itemsAndAmountToRemove)
                    {
                        if (item.Amount <= 0)
                        {
                            continue;
                        }

                        var containerItemCount = remoteStorageInventory.GetItemCountWithoutDuplicates(item.UniqueName);

                        if (containerItemCount > 0)
                        {
                            if (!wasOpened)
                            {
                                storage.SendOpenAnimationNetworkMessage(); // cause the box to open on other clients

                                // then it sets IsOpen, the currentPlayer on the storage, plays an open sound, animates the open
                                // and if the player is the local player, it does some things with  the ui, it also sets PlayerItemManager.IsBusy = true;
                                storage.AnimateAsOpen();
                                // 0.50 second delay, hopefully enough to simulate the close event
                                storage.StartCoroutine(storage.AnimateAsClosedWithDelay());


                                wasOpened = true;
                            }

                            var amountToRemoveFromContainer = Math.Min(containerItemCount, item.Amount);
                            Debug.Log($"{remoteStorageInventory.name} {item.UniqueName} {containerItemCount}, removing {amountToRemoveFromContainer}");

                            // We have only patched RemoveItem if it is a player inventory, so this should be fine.
                            remoteStorageInventory.RemoveItem(item.UniqueName, amountToRemoveFromContainer);
                            item.Amount -= amountToRemoveFromContainer;
                        }

                        if (item.Amount <= 0)
                        {
                            // All items have been removed.
                            continue;
                        }
                    }

                    if (wasOpened)
                    {
                        storage.BroadcastCloseEvent(); // cause a sync of storage contents
                        storage.SendCloseAnimationNetworkMessage(); // cause the box to close on other clients
                    }

                    // bail out, no reason to check any more storages.
                    allItemsHasBeenRemoved = itemsAndAmountToRemove.All(x => x.Amount <= 0);
                    if (allItemsHasBeenRemoved)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Removes items from Player Inventory > Open Storage > Storages on raft
        /// </summary>
        /// <param name="playerInventory"></param>
        /// <param name="uniqueItemName"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        static public void RemoveItemFromInventoryAndStoragesOnRaft(this PlayerInventory playerInventory, string uniqueItemName, int amount, bool removeFromPlayerInventory = true)
        {
            if (amount <= 0)
            {
                return;
            }

            var itemsAndAmountToRemove = new List<ItemNameAndAmount>() { new ItemNameAndAmount(uniqueItemName, amount) };

            playerInventory.RemoveItemFromPlayerInventoryAndStoragesOnRaft(itemsAndAmountToRemove, removeFromPlayerInventory);
        }
    }
}
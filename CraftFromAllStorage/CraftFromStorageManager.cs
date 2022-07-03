using HarmonyLib;
using System;
using System.Collections.Generic;
using thmsn.CraftFromAllStorage.Extensions;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;

namespace thmsn.CraftFromAllStorage
{
    class CraftFromStorageManager
    {
        // TODO: could move to RAPI extension
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
            if (!HasUnlimitedResources())
            {
                //Debug.Log("Getting player inventory");
                var playerInventory = InventoryManager.GetPlayerInventory();

                var itemsAndAmountToRemove = new List<ItemNameAndAmount>();

                // Remove items from player inventory, then the open storage, then other chests.
                foreach (var costMultiple in costMultipleArray)
                {
                    if (costMultiple.amount <= 0)
                    {
                        continue;
                    }

                    // Usually there is only 1 item in .items. for example Plank
                    foreach (var item in costMultiple.items)
                    {
                        itemsAndAmountToRemove.Add(new ItemNameAndAmount(item.UniqueName, costMultiple.amount));
                    }

                    if (manipulateCostAmount)
                    {
                        costMultiple.amount -= costMultiple.amount;
                    }
                }

                playerInventory.RemoveItemFromPlayerInventoryAndStoragesOnRaft(itemsAndAmountToRemove);
            }
        }

        static public void RemoveItem(PlayerInventory inventory, string uniqueItemName, int amount)
        {
            // copied from Inventory.RemoveItem

            if (amount == 0)
            {
                return;
            }

            var itemByName = ItemManager.GetItemByName(uniqueItemName);
            if (itemByName == null)
            {
                return;
            }

            Slot slot = null;

            foreach (Slot allSlot in inventory.allSlots)
            {
                if (amount > 0 && !allSlot.IsEmpty && allSlot.itemInstance.UniqueIndex == itemByName.UniqueIndex)
                {
                    slot = allSlot;
                    if (allSlot.itemInstance.Amount >= amount)
                    {
                        allSlot.RemoveItem(amount);
                        break;
                    }
                    amount -= allSlot.itemInstance.Amount;
                    allSlot.RemoveItem(allSlot.itemInstance.Amount);
                }
            }

            var player = RAPI.GetLocalPlayer();
            // TODO: we might have gotten to a slot that is not selected, and it would not be refreshed
            if (!player.Inventory.hotbar.IsSelectedHotSlot(slot))
            {
                return;
            }

            player.Inventory.hotbar.ReselectCurrentSlot();
        }

        public void RemoveItemUses(
            PlayerInventory inventory,
            string uniqueItemName,
            int usesToRemove,
            bool addItemAfterUseToInventory = true)
        {
            // copied from Inventory.RemoveItemUses
            if (usesToRemove == 0)
            {
                return;
            }

            var itemByName = ItemManager.GetItemByName(uniqueItemName);

            if (itemByName == null)
            {
                return;
            }

            Slot slot = null;
            foreach (Slot allSlot in inventory.allSlots)
            {
                if (usesToRemove > 0 && !allSlot.IsEmpty && allSlot.itemInstance.UniqueIndex == itemByName.UniqueIndex)
                {
                    slot = allSlot;
                    if (allSlot.itemInstance.UsesInStack >= usesToRemove)
                    {
                        allSlot.IncrementUses(-usesToRemove, addItemAfterUseToInventory);
                        break;
                    }
                    usesToRemove -= allSlot.itemInstance.UsesInStack;
                    allSlot.IncrementUses(-allSlot.itemInstance.UsesInStack, addItemAfterUseToInventory);
                }
            }

            var player = RAPI.GetLocalPlayer();
            // TODO: we might have gotten to a slot that is not selected, and it would not be refreshed
            if (!player.Inventory.hotbar.IsSelectedHotSlot(slot))
            {
                return;
            }

            player.Inventory.hotbar.ReselectCurrentSlot();
        }

    }
}
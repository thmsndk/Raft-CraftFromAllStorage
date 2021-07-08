using HarmonyLib;
using System;
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
                Debug.Log($"current storage has enough: {inventory.GetInstanceID()} {currentStorageInventory?.GetInstanceID()}");
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

                    num += container.GetItemCount(costMultipleItems.UniqueName);
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
            foreach (var recipeItem in items)
            {
                foreach (Storage_Small storage in StorageManager.allStorages)
                {
                    Inventory container = storage.GetInventoryReference();

                    bool isOpenByAnotherPlayer = (storage.IsOpen && currentStorageInventory != container);
                    if (isOpenByAnotherPlayer || container == null /*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                        continue;

                    storageInventoryAmount += container.GetItemCount(recipeItem.UniqueName);
                }
            }

            __instance.SetAmount(playerInventoryAmount + storageInventoryAmount);
        }
    }
}

[HarmonyPatch(typeof(CraftingMenu), "CraftItem")]
class CraftItemPatch
{
    static void Prefix(SelectedRecipeBox ___selectedRecipeBox)
    {
        Item_Base itemBase = ___selectedRecipeBox.selectedRecipeItem;

        if (itemBase != null)
        {
            CostMultiple[] newCost = itemBase.settings_recipe.NewCost;
            CraftFromStorageManager.RemoveCostMultiple(newCost);
        }
    }
}

[HarmonyPatch(typeof(BuildingUI_Costbox_Sub_Crafting), "OnQuickCraft")]
class OnQuickCraftPatch
{
    static void Prefix(Item_Base ___item)
    {
        if (___item != null)
        {
            CostMultiple[] newCost = ___item.settings_recipe.NewCost;
            CraftFromStorageManager.RemoveCostMultiple(newCost);
        }
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

                        //storage.Close();

                        //var eventRef = Traverse.Create(ComponentManager<SoundManager>.Value).Field("eventRef_UI_MoveItem").GetValue<string>();
                        //var msg = new Message_SoundManager_PlayOneShot(Messages.SoundManager_PlayOneShot, ComponentManager<Semih_Network>.Value.NetworkIDManager, ComponentManager<SoundManager>.Value.ObjectIndex, eventRef, storage.transform.position);
                        //msg.Broadcast();
                        //FMODUnity.RuntimeManager.PlayOneShot(eventRef, msg.Position);

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
        if (inventory == null)
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

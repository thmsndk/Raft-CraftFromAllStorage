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

        if (!CraftFromStorageManager.isUnlimitedResources() && !enoughInPlayerInventory)
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

            //return CraftFromStorageManager.enoughInStorageInventory(__result, __instance, inventory);

        }
        return __result;
    }
}

[HarmonyPatch(typeof(BuildingUI_CostBox), "SetAmountInInventory")]
class SetAmountInInventoryPatch
{
    static void Postfix(PlayerInventory inventory, BuildingUI_CostBox __instance)
    {
        if (!CraftFromStorageManager.isUnlimitedResources())
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

                    if ((storage.IsOpen && currentStorageInventory != container) || container == null /*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
                        continue;

                    storageInventoryAmount += container.GetItemCount(recipeItem.UniqueName);
                }
            }

            __instance.SetAmount(playerInventoryAmount + storageInventoryAmount);
            //__instance.SetAmount(__instance.GetAmount() + InventoryManager.getItemCountInInventory(__instance, null));
        }
    }
}

[HarmonyPatch(typeof(CraftingMenu), "CraftItem")]
class CraftItemPatch
{
    static void Prefix(SelectedRecipeBox ___selectedRecipeBox)
    {
        Debug.Log("HarmonyPatch CraftItem");
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
        Debug.Log("HarmonyPatch OnQuickCraft");
        if (___item != null)
        {
            CostMultiple[] newCost = ___item.settings_recipe.NewCost;
            CraftFromStorageManager.RemoveCostMultiple(newCost);
        }
    }
}

class InventoryManager
{
    static public Inventory getPlayerInventory()
    {
        return RAPI.GetLocalPlayer().Inventory;
    }

    static public Inventory GetCurrentStorageInventory()
    {
        return RAPI.GetLocalPlayer().StorageManager.currentStorage?.GetInventoryReference();
    }

    static public bool isStorageInventoryOpened()
    {
        return InventoryManager.GetCurrentStorageInventory() != null;
    }

    static public bool isInventorySameAsOpenedStorageInventory(Inventory inventory)
    {
        return InventoryManager.GetCurrentStorageInventory() == inventory;
    }

    static public int getItemCountInInventory(BuildingUI_CostBox costBox, Inventory inventory)
    {
        Inventory actualInventory = inventory != null ? inventory : InventoryManager.GetCurrentStorageInventory();
        List<Item_Base> items = CraftFromStorageManager.getItemsFromCostBox(costBox);
        int itemCount = 0;

        if (actualInventory != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null)
                {
                    itemCount += actualInventory.GetItemCount(items[i].UniqueName);
                }
            }
        }

        return itemCount;
    }

    static public int getItemCountInInventory(CostMultiple costMultiple, Inventory inventory)
    {
        Inventory actualInventory = inventory != null ? inventory : InventoryManager.GetCurrentStorageInventory();
        Item_Base[] items = costMultiple.items;
        int itemCount = 0;

        if (actualInventory != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null)
                {
                    itemCount += actualInventory.GetItemCount(items[i].UniqueName);
                }
            }
        }

        return itemCount;
    }
}

class CraftFromStorageManager
{
    static public bool isUnlimitedResources()
    {
        return GameModeValueManager.GetCurrentGameModeValue().playerSpecificVariables.unlimitedResources;
    }


    static public bool enoughInStorageInventory(bool enouthInPlayerInventory, CostMultiple costMultiple, Inventory inventoryToCheck)
    {
        bool enough = enouthInPlayerInventory;

        if (!CraftFromStorageManager.isUnlimitedResources())
        {
            if (enough == false && InventoryManager.isStorageInventoryOpened() && !InventoryManager.isInventorySameAsOpenedStorageInventory(inventoryToCheck))
            {
                enough = costMultiple.HasEnoughInInventory(InventoryManager.GetCurrentStorageInventory());
            }
        }

        return enough;
    }

    static public List<Item_Base> getItemsFromCostBox(BuildingUI_CostBox costBox)
    {
        return Traverse.Create(costBox).Field("items").GetValue<List<Item_Base>>();
    }

    static public void RemoveCostMultiple(CostMultiple[] costMultipleArray)
    {
        if (!CraftFromStorageManager.isUnlimitedResources())
        {
            Inventory storageInventory = InventoryManager.GetCurrentStorageInventory();
            Inventory playerInventory = InventoryManager.getPlayerInventory();

            if (storageInventory != null)
            {
                for (int i = 0; i < (int)costMultipleArray.Length; i++)
                {
                    CostMultiple costMultiple = costMultipleArray[i];
                    int num = costMultiple.amount - InventoryManager.getItemCountInInventory(costMultiple, playerInventory);

                    for (int j = 0; j < (int)costMultiple.items.Length; j++)
                    {
                        int itemCount = storageInventory.GetItemCount(costMultiple.items[j].UniqueName);
                        if (itemCount < num)
                        {
                            storageInventory.RemoveItem(costMultiple.items[j].UniqueName, itemCount);
                            num -= itemCount;
                        }
                        else
                        {
                            storageInventory.RemoveItem(costMultiple.items[j].UniqueName, num);
                            num = 0;
                        }
                        if (num <= 0)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}

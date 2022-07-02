//using HarmonyLib;
//using System;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using thmsn.CraftFromAllStorage.Network;

//namespace thmsn.CraftFromAllStorage.Patches
//{
//    /// <summary>
//    /// Allow Inserting item to Electric, purifier, smelter to pull from player inventory as well as other storages.
//    /// </summary>
//    [HarmonyPatch(typeof(Block_CookingStand), nameof(Block_CookingStand.InsertItem))]
//    class Block_CookingStand_InsertItem
//    {
//        static bool Prefix(CookingTable_Slot __instance,
//                           Network_Player player,
//                           ItemInstance itemInstance,
//                           ref ItemInstance ___currentItem, // private field
//                           ItemObjectEnabler ___objectEnabler, // private field
//                           CookingTable ___cookingPot) // private field
//        {

//            ___currentItem = itemInstance.Clone();
//            ___currentItem.Amount = 1;
//            ___objectEnabler.ActivateModel(___currentItem.baseItem);
//            if (player != null && player.IsLocalPlayer)
//            {
//                var remainingUses = itemInstance.Uses;
//                var playerInventoryCount = player.Inventory.GetItemCount(itemInstance.UniqueName);

//                var amountToRemoveFromPlayerInventory = Math.Min(playerInventoryCount, remainingUses);

//                player.Inventory.RemoveItemUses(itemInstance.UniqueName, amountToRemoveFromPlayerInventory, false);
//                remainingUses -= amountToRemoveFromPlayerInventory;

//                if (remainingUses <= 0)
//                {
//                    return false;
//                }

//                foreach (Storage_Small storage in StorageManager.allStorages)
//                {
//                    if (storage.IsExcludeFromCraftFromAllStorage())
//                    {
//                        continue;
//                    }

//                    Inventory container = storage.GetInventoryReference();
//                    if (storage.IsOpen || container == null /*|| !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage)*/)
//                    {
//                        continue;
//                    }

//                    var containerItemCount = container.GetItemCountWithoutDuplicates(itemInstance.UniqueName);
//                    var amountToRemoveFromContainer = Math.Min(containerItemCount, remainingUses);

//                    container.RemoveItemUses(itemInstance.UniqueName, amountToRemoveFromContainer, false);
//                    remainingUses -= amountToRemoveFromContainer;

//                    if (remainingUses <= 0)
//                    {
//                        // All items have been removed.
//                        break;
//                    }
//                }

//                ___cookingPot.displayText.HideDisplayTexts();
//                player.Animator.SetAnimation(PlayerAnimation.Trigger_Plant, false);
//            }

//            return false;
//        }
//    }
//}
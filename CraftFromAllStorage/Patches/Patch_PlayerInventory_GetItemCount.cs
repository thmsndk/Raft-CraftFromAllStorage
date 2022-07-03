using HarmonyLib;
using thmsn.CraftFromAllStorage.Extensions;
using thmsn.CraftFromAllStorage.Network;

namespace thmsn.CraftFromAllStorage.Patches
{
    /// <summary>
    /// This needs to be overriden to indicate we have enough resources
    /// Handle both fuel counts and item counts from AutoRecipeBehaviour.CalculatePreparation (colors resources red if missing)
    /// </summary>
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.GetItemCount), typeof(Item_Base))] // TODO: Extract out, it is no longer recipe redux responsible, it is also responsible for Block_CookingStand_Smelter
    class Patch_PlayerInventory_GetItemCount
    {
        static void Postfix(Inventory __instance, ref int __result, Item_Base item)
        {
            var isPlayerInventory = __instance is PlayerInventory;
            if (!__instance || !isPlayerInventory || !item)
            {
                return;
            }

            var playerInventory = __instance as PlayerInventory;

            if (playerInventory.secondInventory != null)
            {
                __result += playerInventory.secondInventory.GetItemCountWithoutDuplicates(item.UniqueName);
            }

            if (!CraftFromStorageManager.HasUnlimitedResources())
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

                    __result += container.GetItemCountWithoutDuplicates(item.UniqueName);
                }
            }
        }
    }
}
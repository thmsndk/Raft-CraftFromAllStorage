using HarmonyLib;
using thmsn.CraftFromAllStorage.Network;

namespace thmsn.CraftFromAllStorage.Patches
{
    // Patches for Auto Recipe Redux support

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
            if (!__instance || !isPlayerInventory)
            {
                return;
            }

            //// https://stackoverflow.com/a/615976/28145
            //System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            //MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
            //Debug.Log("GetItemCount: " + methodBase.);

            // this might be a peformance hog, and bailing out if it is not a player inventory should be enough, could patch AutoRecipeBehaviour.OnIsRayed specifically perhaps
            //Debug.Log($"{__instance.GetType().FullName} Inventory.GetItemCount:" + Environment.StackTrace);
            //if (!Environment.StackTrace.Contains("at AutoRecipeBehaviour.OnIsRayed"))
            //{
            //    return;
            //}

            // Append item count of storages to original GetItemCount
            // Append item count of storages to original GetItemCount, Odds are you don't have a storage open when looking at the recipe.
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
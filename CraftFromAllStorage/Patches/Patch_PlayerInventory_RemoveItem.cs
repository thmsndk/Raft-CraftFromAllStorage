using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using thmsn.CraftFromAllStorage.Extensions;
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
                return true;
            }

            var playerInventory = __instance as PlayerInventory;

            if (playerInventory != null)
            {
                return playerInventory.RemoveItemFromInventoryAndStoragesOnRaft(uniqueItemName, amount);
            }

            return true;
        }
    }
}
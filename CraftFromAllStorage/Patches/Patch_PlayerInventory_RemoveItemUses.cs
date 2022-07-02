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

    /// <summary>
    /// Auto Recipe Redux triggers CookingTable_Slot.OnInsertItem, 
    /// this uses CookingTable_Slot.OnSlotInsertItem that calls CookingTable_Slot.InsertItem 
    /// that calls RemoveItemUses in player inventory.
    /// 
    /// </summary>
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItemUses), typeof(string), typeof(int), typeof(bool))]
    class Patch_PlayerInventory_RemoveItemUses
    {
        static bool Prefix(Inventory __instance, string uniqueItemName, int usesToRemove, bool addItemAfterUseToInventory = true)
        {
            var isPlayerInventory = __instance is PlayerInventory;
            if (!__instance || !isPlayerInventory)
            {
                return true;
            }

            var playerInventory = __instance as PlayerInventory;

            if (playerInventory != null)
            {
                return playerInventory.RemoveItemFromInventoryAndStoragesOnRaft(uniqueItemName, usesToRemove);
            }

            return true;
        }
    }
}
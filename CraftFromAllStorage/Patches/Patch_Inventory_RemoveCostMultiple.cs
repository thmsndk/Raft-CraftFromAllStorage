using HarmonyLib;

namespace thmsn.CraftFromAllStorage.Patches
{
    //// TODO: there seems to be some wrong crafting with quick crafting of bolts... needs to investigate.
    ///*
    // * Attempting to craft 2/6 nails crashes the client to desktop, possibly because I only have 3/2 scrap?
    // */

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveCostMultiple), typeof(CostMultiple[]), typeof(bool))]
    class Patch_Inventory_RemoveCostMultiple
    {
        static bool Prefix(CostMultiple[] costMultiple, bool manipulateCostAmount = false)
        {
            // RemoveCostMultipleIncludeSecondaryInventories calls this twice if a secondary inventory (chest) is opened.
            // So it is important to manipulate the cost amount to prevent resources getting removed multiple times
            CraftFromStorageManager.RemoveCostMultiple(costMultiple, manipulateCostAmount);
            return false; // Prevent the original RemoveCostMultiple from running.
        }
    }
}
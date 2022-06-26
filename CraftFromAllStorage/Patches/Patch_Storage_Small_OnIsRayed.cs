using HarmonyLib;
using thmsn.CraftFromAllStorage.Network;

namespace thmsn.CraftFromAllStorage.Patches
{
    /// <summary>
    /// Allow toggling the excluded flag on and off for a storage.
    /// </summary>
    [HarmonyPatch(typeof(Storage_Small), nameof(Storage_Small.OnIsRayed))]
    class Patch_Storage_Small_OnIsRayed
    {
        private static void Postfix(Storage_Small __instance, ref CanvasHelper ___canvas, Raft_Network ___network)
        {
            var outOfUseDistanceRange = !Helper.LocalPlayerIsWithinDistance(__instance.transform.position, Player.UseDistance + 0.5f);
            if (__instance.IsOpen && !PlayerItemManager.IsBusy && !___canvas.CanOpenMenu && outOfUseDistanceRange)
            {
                return;
            }

            var displayTextManager = ___canvas.displayTextManager;

            var additionalData = __instance.GetAdditionalData();
            if (!additionalData.excludeFromCraftFromAllStorage)
            {
                displayTextManager.ShowText("Press to EXCLUDE from CFAS", MyInput.Keybinds["RMB"].MainKey, 2, 0, false);
            }
            else
            {
                displayTextManager.ShowText("Press to INCLUDE in CFAS", MyInput.Keybinds["RMB"].MainKey, 2, 0, false);
            }

            // TODO: change to a "HOLD" effect to toggle it, perhaps filtered nets has something?
            if (MyInput.GetButtonDown("RMB"))
            {
                additionalData.excludeFromCraftFromAllStorage = !additionalData.excludeFromCraftFromAllStorage; // Toggle bool
                                                                                                                //Debug.Log($"excludeFromCraftFromAllStorage: {additionalData.excludeFromCraftFromAllStorage}");
                __instance.SendAdditionalDataNetworkMessage(___network.NetworkIDManager, additionalData);
            }
        }
    }
}
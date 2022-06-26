using HarmonyLib;
using thmsn.CraftFromAllStorage.Network;



// https://api.raftmodding.com/client-code-examples/adding-private-variables
namespace thmsn.CraftFromAllStorage
{
    // Allow toggling the excluded flag on and off for a storage.
    [HarmonyPatch(typeof(Storage_Small), nameof(Storage_Small.OnIsRayed))]
    class Storage_SmallPatchOnIsRayed
    {
        public static int CHANNEL_ID = 15007;
        public static Messages MESSAGE_TYPE = (Messages)1300;

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

                // Broadcast the additional data to other players
                var message = new Message_Storage_Small_AdditionalData(MESSAGE_TYPE, ___network.NetworkIDManager, __instance.GetAdditionalData(), __instance);
                RAPI.SendNetworkMessage(message, channel: CHANNEL_ID);
            }
        }
    }
}
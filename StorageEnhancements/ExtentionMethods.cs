using HarmonyLib;
using Steamworks;

static class ExtentionMethods
{
    public static bool BelongsToPlayer(this Slot slot) => Traverse.Create(slot).Field("inventory").GetValue<Inventory>() is PlayerInventory;
    public static void Close(this Storage_Small box)
    {
        box.Close(RAPI.GetLocalPlayer());
        new Message_Storage_Close(Messages.StorageManager_Close, RAPI.GetLocalPlayer().StorageManager, box).Broadcast();
    }
    public static void Broadcast(this Message message) => ComponentManager<Semih_Network>.Value.RPC(message, Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
    public static void Send(this Message message, CSteamID steamID) => ComponentManager<Semih_Network>.Value.SendP2P(steamID, message, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
}
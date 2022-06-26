using HarmonyLib;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

static class ExtentionMethods
{
    public static void BroadcastCloseEvent(this Storage_Small box)
    {
        var player = RAPI.GetLocalPlayer();
        var message = new Message_Storage_Close(Messages.StorageManager_Close, player.StorageManager, box);
        if (Raft_Network.IsHost)
        {
            // TODO. if we are sending multiple messages, we could potentially batch them together? e.g. removing items from multiple storages.
            message.Broadcast(NetworkChannel.Channel_Game);
        }
        else
        {
            message.Send(player.Network.HostID, NetworkChannel.Channel_Game);
        }
    }

    public static void Broadcast(this Message message, NetworkChannel channel/* = (NetworkChannel)MessageType.ChannelID*/) => ComponentManager<Raft_Network>.Value.RPC(message, Target.Other, EP2PSend.k_EP2PSendReliable, channel);
    public static void Send(this Message message, CSteamID steamID, NetworkChannel channel) => ComponentManager<Raft_Network>.Value.SendP2P(steamID, message, EP2PSend.k_EP2PSendReliable, channel);


}

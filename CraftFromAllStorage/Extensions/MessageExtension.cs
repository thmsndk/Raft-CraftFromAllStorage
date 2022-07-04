using HarmonyLib;
using Steamworks;
using System.Collections.Generic;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Extensions
{
    static class MessageExtension
    {
        public static void SendOrBroadcast(this Message message, NetworkChannel channel = (NetworkChannel)Channel.DEFAULT)
        {
            var player = RAPI.GetLocalPlayer();

            if (Raft_Network.IsHost)
            {
                message.Broadcast(channel);
            }
            else
            {
                message.Send(player.Network.HostID, channel);
            }
        }

        public static void Broadcast(this Message message, NetworkChannel channel = (NetworkChannel)Channel.DEFAULT) => ComponentManager<Raft_Network>.Value.RPC(message, Target.Other, EP2PSend.k_EP2PSendReliable, channel);
        public static void Send(this Message message, CSteamID steamID, NetworkChannel channel) => ComponentManager<Raft_Network>.Value.SendP2P(steamID, message, EP2PSend.k_EP2PSendReliable, channel);


    }
}
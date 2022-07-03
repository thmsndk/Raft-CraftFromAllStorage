using HarmonyLib;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Extensions
{
    static class MessageExtension
    {
        public static void BroadcastOpenEvent(this Storage_Small box)
        {
            // storage small has this when interacting with it
            //if (Raft_Network.IsHost)
            //{
            //    this.network.RPC((Message)new Message_Storage(Messages.StorageManager_Open,
            //    (MonoBehaviour_Network)this.storageManager, this),
            //    Target.Other,
            //    EP2PSend.k_EP2PSendReliable,
            //    NetworkChannel.Channel_Game);
            //    this.storageManager.OpenStorage(this);
            //}
            //else
            //    this.network.SendP2P(this.network.HostID,
            //    (Message)new Message_Storage(Messages.StorageManager_Open,
            //    (MonoBehaviour_Network)this.storageManager, this),
            //    EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);

            var player = RAPI.GetLocalPlayer();
            var message = new Message_Storage(Messages.StorageManager_Open, player.StorageManager, box);
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

        public static void BroadcastCloseEvent(this Storage_Small box)
        {
            // storage manager has this on update
            //if (Raft_Network.IsHost)
            //{
            //    this.playerNetwork.Network.RPC((Message)messageStorageClose, Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
            //    this.CloseStorage(this.currentStorage);
            //}
            //else
            //    this.playerNetwork.SendP2P((Message)messageStorageClose, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);

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
}
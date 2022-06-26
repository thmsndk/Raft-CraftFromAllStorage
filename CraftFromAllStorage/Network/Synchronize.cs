using HarmonyLib;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Network
{
    public static class Synchronize
    {
        public static void Storage_Small_AdditionalData()
        {
            if (Raft_Network.IsHost)
            {
                //var channel = (NetworkChannel)Storage_SmallPatchOnIsRayed.CHANNEL_ID;

                foreach (Storage_Small storage in StorageManager.allStorages)
                {
                    var data = storage.GetAdditionalData();
                    var network = Traverse.Create(storage).Field("network").GetValue<Raft_Network>();

                    if (data != null && network != null)
                    {
                        Debug.Log($"Sending data for {storage.name} excludeFromCraftFromAllStorage: {data.excludeFromCraftFromAllStorage}");
                        storage.SendAdditionalDataNetworkMessage(network.NetworkIDManager, data); // TODO: Unsure how to send to a specific player. solve that later
                    }
                }
            }
        }
    }
}

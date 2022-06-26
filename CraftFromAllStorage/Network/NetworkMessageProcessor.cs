using HarmonyLib;
using System.Collections.Generic;
using thmsn.CraftFromAllStorage.Patches;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Network
{
    public static class NetworkMessageProcessor
    {
        public static List<Message> waiting = new List<Message>();

        public static void ListenAndProcess()
        {
            NetworkMessage netMessage = RAPI.ListenForNetworkMessagesOnChannel(Message_Storage_Small_AdditionalData.CHANNEL_ID);

            if (netMessage != null)
            {
                Message message = netMessage.message;

                if (CraftFromAllStorageMod.worldLoaded)
                {
                    ProcessMessage(message);
                }
                else
                {
                    waiting.Add(message);
                }
            }
        }

        public static void ProcessMessage(Message message)
        {
            if (message.Type == Message_Storage_Small_AdditionalData.MESSAGE_TYPE)
            {
                var msg = message as Message_Storage_Small_AdditionalData;

                if (msg != null)
                {
                    var storageManager = RAPI.GetLocalPlayer()?.StorageManager; //  TODO: this is null, storage manager has not initialized yet. and technically the game is not done loading
                    // TODO: probably need to patch OnWorldReceivedLate
                    // [HarmonyPatch(typeof(GameManager), "OnWorldRecievedLate")]

                    if (storageManager != null)
                    {
                        var storage = storageManager.GetStorageByObjectIndex(msg.storageObjectIndex);

                        if (storage != null)
                        {
                            var data = storage.GetAdditionalData();

                            if (data != null)
                            {
                                // TODO: notification of toggled state
                                data.SetData(msg.data);

                                if (data.excludeFromCraftFromAllStorage)
                                {
                                    Debug.Log("A storage is now excluded from Craft From All Storage");
                                }
                                else
                                {
                                    Debug.Log("A storage is now included in Craft From All Storage");
                                }
                            }
                        }
                        else
                        {
                            Debug.Log($"storage with storageObjectIndex {msg.storageObjectIndex} was not found");
                        }
                    }
                    else
                    {
                        Debug.Log("could not find a storage manager");
                    }
                }
                else
                {
                    Debug.Log("msg was not a Message_Storage_Small_AdditionalData");
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameManager), "OnWorldRecievedLate")]
    class Patch_OnWorldRecievedLate
    {
        static void Postfix()
        {
            CraftFromAllStorageMod.worldLoaded = true;

            if (NetworkMessageProcessor.waiting.Count > 0)
            {
                foreach (var message in NetworkMessageProcessor.waiting)
                {
                    NetworkMessageProcessor.ProcessMessage(message);
                }

                NetworkMessageProcessor.waiting.Clear();
            }
        }
    }
}

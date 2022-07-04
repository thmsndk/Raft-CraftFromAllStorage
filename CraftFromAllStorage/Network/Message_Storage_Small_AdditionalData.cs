using System;
using thmsn.CraftFromAllStorage.Extensions;
using UnityEngine;

// https://api.raftmodding.com/client-code-examples/adding-private-variables
namespace thmsn.CraftFromAllStorage.Network
{
    [Serializable]
    public class Message_Storage_Small_AdditionalData : Message_Storage
    {
        
        public const Messages MESSAGE_TYPE = (Messages)NetworkMessage.INITIALIZE_STORAGE;

        public Message_Storage_Small_AdditionalData(NetworkIDManager behaviour, Storage_SmallAdditionalData data, Storage_Small storage) : base(MESSAGE_TYPE, behaviour, storage)
        {
            this.data = data;
        }

        public Storage_SmallAdditionalData data;

        public static void HandleAdditionalData(Message_Storage_Small_AdditionalData message)
        {
            if (message != null)
            {
                var storageManager = RAPI.GetLocalPlayer()?.StorageManager;

                if (storageManager != null)
                {
                    var storage = storageManager.GetStorageByObjectIndex(message.storageObjectIndex);

                    if (storage != null)
                    {
                        var data = storage.GetAdditionalData();

                        if (data != null)
                        {
                            // TODO: notification of toggled state
                            data.SetData(message.data);

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
                        Debug.Log($"storage with storageObjectIndex {message.storageObjectIndex} was not found");
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
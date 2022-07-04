using System;
using thmsn.CraftFromAllStorage.Extensions;
using UnityEngine;

// https://api.raftmodding.com/client-code-examples/adding-private-variables
namespace thmsn.CraftFromAllStorage.Network
{
    [Serializable]
    public class Message_Storage_Small_AnimateClose : Message_Storage
    {
        public const Messages MESSAGE_TYPE = (Messages)NetworkMessage.CLOSE_ANIMATION;
        public Message_Storage_Small_AnimateClose(NetworkIDManager behaviour, Storage_Small storage) : base(MESSAGE_TYPE, behaviour, storage)
        {

        }

        internal static void HandleNetworkMessage(Message_Storage_Small_AnimateClose message)
        {
            if (message != null)
            {
                var storageManager = RAPI.GetLocalPlayer()?.StorageManager;

                if (storageManager != null)
                {
                    var storage = storageManager.GetStorageByObjectIndex(message.storageObjectIndex);

                    if (storage != null)
                    {
                        storage.AnimateAsClosed();
                    }
                    else
                    {
                        Debug.LogWarning($"Message_Storage_Small_AnimateClose storage with storageObjectIndex {message.storageObjectIndex} was not found");
                    }
                }
            }
        }
    }


}
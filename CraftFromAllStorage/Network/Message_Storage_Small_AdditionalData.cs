using System;

// https://api.raftmodding.com/client-code-examples/adding-private-variables
namespace thmsn.CraftFromAllStorage.Network
{
    [Serializable]
    public class Message_Storage_Small_AdditionalData : Message_Storage
    {
        public static int CHANNEL_ID = 15007;
        public static Messages MESSAGE_TYPE = (Messages)1300;

        public Message_Storage_Small_AdditionalData(NetworkIDManager behaviour, Storage_SmallAdditionalData data, Storage_Small storage) : base(MESSAGE_TYPE, behaviour, storage)
        {
            this.data = data;
        }

        public Storage_SmallAdditionalData data;

        public void SendNetworkMessage()
        {
            RAPI.SendNetworkMessage(this, channel: CHANNEL_ID);
        }
    }

    public static class Storage_SmallExtension
    {
        public static void SendAdditionalDataNetworkMessage(this Storage_Small storage, NetworkIDManager behaviour, Storage_SmallAdditionalData data)
        {
            new Message_Storage_Small_AdditionalData(behaviour, data, storage).SendNetworkMessage();
        }
    }
}
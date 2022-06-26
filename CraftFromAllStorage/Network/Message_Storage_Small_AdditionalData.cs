using System;

// https://api.raftmodding.com/client-code-examples/adding-private-variables
namespace thmsn.CraftFromAllStorage.Network
{
    [Serializable]
    public class Message_Storage_Small_AdditionalData : Message_Storage
    {
        public Message_Storage_Small_AdditionalData(Messages type, MonoBehaviour_Network behaviour, Storage_SmallAdditionalData data, Storage_Small storage) : base(type, behaviour, storage)
        {
            this.data = data;
        }

        public Storage_SmallAdditionalData data;
    }
}
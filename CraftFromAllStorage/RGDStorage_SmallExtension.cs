using System;
using System.Runtime.CompilerServices;
using thmsn.CraftFromAllStorage.Network;



// https://api.raftmodding.com/client-code-examples/adding-private-variables
namespace thmsn.CraftFromAllStorage
{
    /// <summary>
    /// Allows persistance / saving of private variables
    /// To save private variables you need to patch RDG_%RaftClassName% class and method that is invoked inside switch in SaveAndLoad class in RestoreRGDGame(RGD_Game game) method.
    /// </summary>
    public static class RGDStorage_SmallExtension
    {
        public static ConditionalWeakTable<RGD_Storage, Storage_SmallAdditionalData> RGD_data =
                new ConditionalWeakTable<RGD_Storage, Storage_SmallAdditionalData>();

        public static void AddData(this RGD_Storage RGD_Storage, Storage_SmallAdditionalData value)
        {
            try
            {
                RGD_data.Add(RGD_Storage, value);
            }
            catch (Exception) { }
        }
    }
}
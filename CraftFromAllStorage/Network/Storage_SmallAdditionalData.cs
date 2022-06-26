using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



// https://api.raftmodding.com/client-code-examples/adding-private-variables
namespace thmsn.CraftFromAllStorage.Network
{
    [Serializable]
    public class Storage_SmallAdditionalData
    {
        public bool excludeFromCraftFromAllStorage;
        public Storage_SmallAdditionalData()
        {
            excludeFromCraftFromAllStorage = false;
        }

        /// <summary>
        /// Used to set data when updates are sent over network.
        /// </summary>
        /// <param name="data"></param>
        public void SetData(Storage_SmallAdditionalData data)
        {
            excludeFromCraftFromAllStorage = data.excludeFromCraftFromAllStorage;
        }
    }
}
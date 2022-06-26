using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

    public static class Storage_SmallAdditionalDataExtension
    {
        private static readonly ConditionalWeakTable<Storage_Small, Storage_SmallAdditionalData> data =
        new ConditionalWeakTable<Storage_Small, Storage_SmallAdditionalData>();

        public static ConditionalWeakTable<RGD_Storage, Storage_SmallAdditionalData> RGD_data =
                new ConditionalWeakTable<RGD_Storage, Storage_SmallAdditionalData>();

        public static Storage_SmallAdditionalData GetAdditionalData(this Storage_Small storage)
        {
            return data.GetOrCreateValue(storage);
        }

        public static void AddData(this Storage_Small storage, Storage_SmallAdditionalData value)
        {
            try
            {
                data.Add(storage, value);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void AddData(this RGD_Storage RGD_Storage, Storage_SmallAdditionalData value)
        {
            try
            {
                RGD_data.Add(RGD_Storage, value);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static bool IsExcludeFromCraftFromAllStorage(this Storage_Small box)
        {
            var data = box.GetAdditionalData();

            return data.excludeFromCraftFromAllStorage;
        }
    }
}
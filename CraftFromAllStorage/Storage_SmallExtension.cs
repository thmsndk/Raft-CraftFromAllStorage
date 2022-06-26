using System;
using System.Runtime.CompilerServices;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;



// https://api.raftmodding.com/client-code-examples/adding-private-variables
namespace thmsn.CraftFromAllStorage
{
    public static class Storage_SmallExtension
    {
        private static readonly ConditionalWeakTable<Storage_Small, Storage_SmallAdditionalData> data =
        new ConditionalWeakTable<Storage_Small, Storage_SmallAdditionalData>();

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

        public static bool IsExcludeFromCraftFromAllStorage(this Storage_Small box)
        {
            var data = box.GetAdditionalData();

            return data.excludeFromCraftFromAllStorage;
        }
    }
}
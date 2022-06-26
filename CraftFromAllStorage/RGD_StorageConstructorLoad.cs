using HarmonyLib;
using System;
using System.Runtime.Serialization;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;



// https://api.raftmodding.com/client-code-examples/adding-private-variables
namespace thmsn.CraftFromAllStorage
{
    [HarmonyPatch(typeof(RGD_Storage), MethodType.Constructor, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) })]
    class RGD_StorageConstructorLoad // Constructor for loading
    {
        private static void Prefix(RGD_Storage __instance, ref SerializationInfo info)
        {
            try
            {
                // Loads from Json that we will create in GetObjectData
                var json = info.GetString("AdditionalData");
                //Debug.Log($"RGD_Storage.Constructor loading json {json}");
                __instance.AddData(JsonUtility.FromJson<Storage_SmallAdditionalData>(json));
            }
            catch (Exception) { }
        }
    }
}
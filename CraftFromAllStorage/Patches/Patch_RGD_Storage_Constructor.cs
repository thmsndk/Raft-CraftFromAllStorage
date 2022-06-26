using HarmonyLib;
using System;
using System.Runtime.Serialization;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Patches
{
    [HarmonyPatch(typeof(RGD_Storage), MethodType.Constructor, typeof(RGDType), typeof(Block), typeof(uint), typeof(Inventory), typeof(bool))]
    class Patch_RGD_Storage_ConstructorSave // Constructor for saving
    {
        private static void Prefix(RGD_Storage __instance, ref Block block, ref uint storageObjectIndex, ref Inventory inventory, bool isOpen)
        {
            var smallStorage = block as Storage_Small;
            if (smallStorage != null)
            {
                var data = smallStorage.GetAdditionalData();
                //Debug.Log($"RGD_Storage.Constructor adding RGD data excludeFromCraftFromAllStorage: {data.excludeFromCraftFromAllStorage}");
                __instance.AddData(data);
            }
        }
    }

    [HarmonyPatch(typeof(RGD_Storage), MethodType.Constructor, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) })]
    class Patch_RGD_Storage_Constructor_Load // Constructor for loading
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
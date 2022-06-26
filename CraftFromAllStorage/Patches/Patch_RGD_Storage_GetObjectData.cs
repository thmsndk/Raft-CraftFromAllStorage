using HarmonyLib;
using System.Runtime.Serialization;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Patches
{
    [HarmonyPatch(typeof(RGD_Storage), "GetObjectData")]
    class Patch_RGD_Storage_GetObjectData // Interfering with the serialization flow and adding our custom data to the save
    {
        private static void Postfix(RGD_Storage __instance, ref SerializationInfo info)
        {
            Storage_SmallAdditionalData value;
            if (Storage_SmallAdditionalDataExtension.RGD_data.TryGetValue(__instance, out value))
            {
                var json = JsonUtility.ToJson(value);
                //Debug.Log($"RGD_Storage.GetObjectData saving json {json}");
                // We need to use json because worlds loads before mod compiles
                info.AddValue("AdditionalData", json);
            }
        }
    }
}
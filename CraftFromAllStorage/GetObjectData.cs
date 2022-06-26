using HarmonyLib;
using System.Runtime.Serialization;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;



// https://api.raftmodding.com/client-code-examples/adding-private-variables
namespace thmsn.CraftFromAllStorage
{
    [HarmonyPatch(typeof(RGD_Storage), "GetObjectData")]
    class GetObjectData // Interfering with the serialization flow and adding our custom data to the save
    {
        private static void Postfix(RGD_Storage __instance, ref SerializationInfo info)
        {
            Storage_SmallAdditionalData value;
            if (RGDStorage_SmallExtension.RGD_data.TryGetValue(__instance, out value))
            {
                var json = JsonUtility.ToJson(value);
                //Debug.Log($"RGD_Storage.GetObjectData saving json {json}");
                // We need to use json because worlds loads before mod compiles
                info.AddValue("AdditionalData", json);
            }
        }
    }
}
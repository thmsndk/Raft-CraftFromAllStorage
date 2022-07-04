using HarmonyLib;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Patches
{
    /*
     * TODO: figure out if there is a better place to patch.
     Fynikoto — Today at 22:04
    Perhaps RGD_DATA.RestoreBlock is even better to patch
    Ohhhhh wait. Perhaps you can override this method directly instead of Patching the parent class
    SaveAndLoad.RestoreBlock calls rgdBlock.RestoreBlock.
    rgdBlock is rgd_storage in your case and RestoreBlock is inherited from RGD_Block
    So there is an inherited Method RestoreBlock on RGD_Storage. perhaps you can patch that
    */

    /// <summary>
    /// Attach additional data from the RGD_Storage to Storage_Small
    /// </summary>
    [HarmonyPatch(typeof(SaveAndLoad), "RestoreBlock", typeof(BlockCreator), typeof(RGD_Block))]
    class Patch_Storage_Small_RestoreBlock
    {
        private static void Postfix(SaveAndLoad __instance, RGD_Block rgdBlock, Block __result)
        {
            var rgdStorage = rgdBlock as RGD_Storage;
            if (rgdStorage != null)
            {
                var storage = __result.GetComponent<Storage_Small>();

                if (storage == null)
                {
                    Debug.LogWarning($"Could not find Storage_Small component for block with object index {__result.ObjectIndex}");
                    return;
                }

                Storage_SmallAdditionalData value;
                if (Storage_SmallAdditionalDataExtension.RGD_data.TryGetValue(rgdStorage, out value))
                {
                    //Debug.Log($"{storage.name} has additonal data {value.excludeFromCraftFromAllStorage}");
                    storage.AddData(value);
                }
            }
        }
    }
}
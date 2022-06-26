using HarmonyLib;
using thmsn.CraftFromAllStorage.Network;



// https://api.raftmodding.com/client-code-examples/adding-private-variables
namespace thmsn.CraftFromAllStorage
{
    /*
     Fynikoto — Today at 22:04
    Perhaps RGD_DATA.RestoreBlock is even better to patch
    Ohhhhh wait. Perhaps you can override this method directly instead of Patching the parent class
    SaveAndLoad.RestoreBlock calls rgdBlock.RestoreBlock.
    rgdBlock is rgd_storage in your case and RestoreBlock is inherited from RGD_Block
    So there is an inherited Method RestoreBlock on RGD_Storage. perhaps you can patch that
    */
    // Attach the loaded data, inventory is restored in a different way, so we have to patch SaveAndLoad
    [HarmonyPatch(typeof(SaveAndLoad), "RestoreBlock", typeof(BlockCreator), typeof(RGD_Block))]
    class Storage_SmallRestoreBlockPatch
    {
        private static void Postfix(SaveAndLoad __instance, RGD_Block rgdBlock, Block __result)
        {
            var rgdStorage = rgdBlock as RGD_Storage;
            if (rgdStorage != null)
            {
                var storage = __result.GetComponent<Storage_Small>();

                Storage_SmallAdditionalData value;
                if (RGDStorage_SmallExtension.RGD_data.TryGetValue(rgdStorage, out value))
                {
                    //Debug.Log($"{storage.name} has additonal data {value.excludeFromCraftFromAllStorage}");
                    storage.AddData(value);
                }
            }
        }
    }
}
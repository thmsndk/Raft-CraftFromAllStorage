using HarmonyLib;



// https://api.raftmodding.com/client-code-examples/adding-private-variables
namespace thmsn.CraftFromAllStorage
{
    [HarmonyPatch(typeof(RGD_Storage), MethodType.Constructor, typeof(RGDType), typeof(Block), typeof(uint), typeof(Inventory), typeof(bool))]
    class RGD_StorageConstructorSave // Constructor for saving
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
}
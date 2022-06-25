using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// https://api.raftmodding.com/client-code-examples/adding-private-variables

[Serializable]
public class Storage_SmallAdditionalData
{
    public bool excludeFromCraftFromAllStorage;
    public Storage_SmallAdditionalData()
    {
        excludeFromCraftFromAllStorage = false;
    }
}

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

// Allow toggling the excluded flag on and off for a storage.
[HarmonyPatch(typeof(Storage_Small), nameof(Storage_Small.OnIsRayed))]
class Storage_SmallPatchOnIsRayed
{
    private static void Postfix(Storage_Small __instance, ref CanvasHelper ___canvas)
    {
        var displayTextManager = ___canvas.displayTextManager;

        var additionalData = __instance.GetAdditionalData();
        if (!additionalData.excludeFromCraftFromAllStorage)
        {
            displayTextManager.ShowText("Press to EXCLUDE from CFAS", MyInput.Keybinds["RMB"].MainKey, 2, 0, false);
        }
        else
        {
            displayTextManager.ShowText("Press to INCLUDE in CFAS", MyInput.Keybinds["RMB"].MainKey, 2, 0, false);
        }
        // TODO: change to a "HOLD" effect to toggle it, perhaps filtered nets has something?
        if (MyInput.GetButtonDown("RMB"))
        {
            additionalData.excludeFromCraftFromAllStorage = !additionalData.excludeFromCraftFromAllStorage; // Toggle bool
            //Debug.Log($"excludeFromCraftFromAllStorage: {additionalData.excludeFromCraftFromAllStorage}");
        }
    }
}

/// <summary>
/// Allows persistance / saving of private variables
/// To save private variables you need to patch RDG_%RaftClassName% class and method that is invoked inside switch in SaveAndLoad class in RestoreRGDGame(RGD_Game game) method.
/// </summary>
public static class RGDStorage_SmallExtension
{
    public static ConditionalWeakTable<RGD_Storage, Storage_SmallAdditionalData> RGD_data =
            new ConditionalWeakTable<RGD_Storage, Storage_SmallAdditionalData>();

    public static void AddData(this RGD_Storage RGD_Storage, Storage_SmallAdditionalData value)
    {
        try
        {
            RGD_data.Add(RGD_Storage, value);
        }
        catch (Exception) { }
    }
}

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

//[HarmonyPatch(typeof(RGD_Storage), nameof(RGD_Storage.RestoreInventory))]
//class SteeringWheelRestoreWheelPatch
//{
//    private static void Prefix(RGD_Storage __instance, Inventory inventory)
//    {
//        Storage_SmallAdditionalData value;
//        if (RGDStorage_SmallExtension.RGD_data.TryGetValue(rgdStorage, out value))
//        {
//            __instance.AddData(value);
//        }
//    }
//}
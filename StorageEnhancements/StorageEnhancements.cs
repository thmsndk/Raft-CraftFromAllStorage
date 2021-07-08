using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/*
 * Ideas
 * Render a sprite above / in front of a container like the island loot mod? - perhaps when you focus it (OnRayed?
 * An Item Sign that renders what is inside a storage chest?
 * All In One inventory that opens by default when you press tab? - should render all storages on your raft.
 * Craft From storage, should craft from all chests, not only your local one.
 * - if you have a storage open, prefer taking items from that one. else take it from the other storages.
 */


public class StorageEnhancements : Mod
{
    private const string harmonyId = "com.thmsn.storage-enhancements";
    Harmony harmony;

    public void Start()
    {
        harmony = new Harmony(harmonyId);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        //this.gameObject.AddComponent<ShowStorageIcons>();
        Debug.Log("StorageEnhancements Mod has been loaded!");

    }

    public void OnModUnload()
    {
        Debug.Log("StorageEnhancements Mod has been unloaded!");
        harmony.UnpatchAll(harmonyId);
        Destroy(gameObject);
    }

    public static string depositAllItems()
    {
        Network_Player player = RAPI.GetLocalPlayer();
        if (player == null)
            return "Must be used in world";
        List<Slot> playerItems = player.Inventory.allSlots;
        List<Slot> items = new List<Slot>();
        //if ((int)slot.slotType % 2 == 1 || (!ignoreHotbar && slot.slotType == SlotType.Hotbar))
        //{
        //    items.Add(slot);
        //}
        foreach (Storage_Small storage in StorageManager.allStorages)
        {
            Inventory container = storage.GetInventoryReference();
            if (storage.IsOpen || container == null || !Helper.LocalPlayerIsWithinDistance(storage.transform.position, player.StorageManager.maxDistanceToStorage))
                continue;
            var edited = false;
            foreach (Slot slot in items)
                if (!slot.IsEmpty && container.GetItemCount(slot.GetItemBase()) > 0 /*&& slot.itemInstance.exclusiveString != favoriteString*/)
                {
                    var p = slot.itemInstance.Amount;
                    container.AddItem(slot.itemInstance, false);
                    if (slot.itemInstance.Amount != p)
                        edited = true;
                    if (slot.itemInstance.Amount == 0)
                        slot.SetItem(null);
                }
            if (edited)
            {
                var eventRef = Traverse.Create(ComponentManager<SoundManager>.Value).Field("eventRef_UI_MoveItem").GetValue<string>();
                storage.Close();
                var msg = new Message_SoundManager_PlayOneShot(Messages.SoundManager_PlayOneShot, ComponentManager<Semih_Network>.Value.NetworkIDManager, ComponentManager<SoundManager>.Value.ObjectIndex, eventRef, storage.transform.position);
                msg.Broadcast();
                FMODUnity.RuntimeManager.PlayOneShot(eventRef, msg.Position);
            }
        }
        return "Items deposited";
    }
}

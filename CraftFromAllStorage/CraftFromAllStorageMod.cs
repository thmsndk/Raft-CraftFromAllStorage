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
 * Hold modifiers to increase how many are crafted when pressing the craft button.
 */


public class StorageEnhancements : Mod
{
    private const string harmonyId = "com.thmsn.craft-from-all-storage";
    Harmony harmony;

    public void Start()
    {
        harmony = new Harmony(harmonyId);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        Debug.Log("Craft From All Sotrage Mod has been loaded!");

    }

    public void OnModUnload()
    {
        Debug.Log("Craft From All Sotrage Mod has been unloaded!");
        harmony.UnpatchAll(harmonyId);
        Destroy(gameObject);
    }
}

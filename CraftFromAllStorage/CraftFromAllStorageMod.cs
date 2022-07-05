using HarmonyLib;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using thmsn.CraftFromAllStorage.Network;
using UnityEngine;

//To set the time to 12 o'clock btw (if you ever need it): csrun ComponentManager<Network_Host>.Value.skyController.timeOfDay.hour = 12f;

/*
 * Thanks
 *  Azzmurr - Creating the original Craft From Storage mod that this mod is derived from.
 *  Aidanamite - for all their efforts and help on the mod discord, making this mod a reality.
 *  Fynikoto - For debugging MoreStorages duplicate issues, storage syncing testing, feature suggestions and guidance.
 *  DeadByte42 - For the inital Raft v1 PR.
 *  Entoarox - For helping with storage syncing
 */

namespace thmsn.CraftFromAllStorage
{
    public class CraftFromAllStorageMod : Mod
    {
        public static string ModNamePrefix = "<color=#d16e17>[Craft From All Storage]</color>";
        private const string harmonyId = "com.thmsn.craft-from-all-storage";
        Harmony harmony;

        public static bool worldLoaded = false;

        public void Start()
        {
            harmony = new Harmony(harmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Debug.Log($"{ModNamePrefix} {CraftFromAllStorageMod.modInstance.version} has been loaded!");

        }

        public void OnModUnload()
        {
            Debug.Log($"{ModNamePrefix} {CraftFromAllStorageMod.modInstance.version} has been unloaded!");
            harmony.UnpatchAll(harmonyId);
        }

        public void FixedUpdate()
        {
            NetworkMessageProcessor.ListenAndProcess();
        }

        public override void WorldEvent_OnPlayerConnected(CSteamID steamId, RGD_Settings_Character characterSettings)
        {
            Synchronize.Storage_Small_AdditionalData();
        }

        public override void WorldEvent_WorldLoaded()
        {
            worldLoaded = true;
        }
    }
}
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
        public static List<Message> waiting = new List<Message>();

        public void Start()
        {
            harmony = new Harmony(harmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Debug.Log(ModNamePrefix + " has been loaded!");

        }

        public void OnModUnload()
        {
            Debug.Log(ModNamePrefix + " has been unloaded!");
            harmony.UnpatchAll(harmonyId);
        }

        public void FixedUpdate()
        {
            NetworkMessage netMessage = RAPI.ListenForNetworkMessagesOnChannel(Storage_SmallPatchOnIsRayed.CHANNEL_ID);

            if (netMessage != null)
            {
                Message message = netMessage.message;
                if (worldLoaded)
                {
                    ProcessMessage(message);
                }
                else
                {
                    waiting.Add(message);
                }
            }
        }

        public static void ProcessMessage(Message message)
        {
            if (message.Type == Storage_SmallPatchOnIsRayed.MESSAGE_TYPE)
            {
                var msg = message as Message_Storage_Small_AdditionalData;

                if (msg != null)
                {
                    var storageManager = RAPI.GetLocalPlayer()?.StorageManager; //  TODO: this is null, storage manager has not initialized yet. and technically the game is not done loading
                    // TODO: probably need to patch OnWorldReceivedLate
                    // [HarmonyPatch(typeof(GameManager), "OnWorldRecievedLate")]

                    if (storageManager != null)
                    {
                        var storage = storageManager.GetStorageByObjectIndex(msg.storageObjectIndex);

                        if (storage != null)
                        {
                            var data = storage.GetAdditionalData();

                            if (data != null)
                            {
                                // TODO: notification of toggled state
                                data.SetData(msg.data);

                                if (data.excludeFromCraftFromAllStorage)
                                {
                                    Debug.Log("A storage is now excluded from Craft From All Storage");
                                }
                                else
                                {
                                    Debug.Log("A storage is now included in Craft From All Storage");
                                }
                            }
                        }
                        else
                        {
                            Debug.Log($"storage with storageObjectIndex {msg.storageObjectIndex} was not found");
                        }
                    }
                    else
                    {
                        Debug.Log("could not find a storage manager");
                    }
                }
                else
                {
                    Debug.Log("msg was not a Message_Storage_Small_AdditionalData");
                }
            }
        }

        public override void WorldEvent_OnPlayerConnected(CSteamID steamId, RGD_Settings_Character characterSettings)
        {
            if (Raft_Network.IsHost)
            {
                //var channel = (NetworkChannel)Storage_SmallPatchOnIsRayed.CHANNEL_ID;

                foreach (Storage_Small storage in StorageManager.allStorages)
                {
                    var data = storage.GetAdditionalData();
                    var network = Traverse.Create(storage).Field("network").GetValue<Raft_Network>();

                    if (data != null && network != null)
                    {
                        Debug.Log($"Sending data for {storage.name} excludeFromCraftFromAllStorage: {data.excludeFromCraftFromAllStorage}");
                        // Broadcast the additional data to other players
                        var message = new Message_Storage_Small_AdditionalData(Storage_SmallPatchOnIsRayed.MESSAGE_TYPE, network.NetworkIDManager, data, storage);
                        RAPI.SendNetworkMessage(message, channel: Storage_SmallPatchOnIsRayed.CHANNEL_ID); // TODO: Unsure how to send to a specific player.
                    }
                }
            }
        }

        public override void WorldEvent_WorldLoaded()
        {
            //if (Raft_Network.IsHost)
            //{
            //}
            worldLoaded = true;
        }
    }

    [HarmonyPatch(typeof(GameManager), "OnWorldRecievedLate")]
    class Patch_RemoteWorldLoaded
    {
        static void Postfix()
        {
            CraftFromAllStorageMod.worldLoaded = true;

            if (CraftFromAllStorageMod.waiting.Count > 0)
            {
                foreach (var message in CraftFromAllStorageMod.waiting)
                {
                    CraftFromAllStorageMod.ProcessMessage(message);
                }

                CraftFromAllStorageMod.waiting.Clear();
            }
        }
    }
}
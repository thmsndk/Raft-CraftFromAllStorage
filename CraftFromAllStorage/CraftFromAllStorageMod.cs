using HarmonyLib;
using Steamworks;
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
                if (message.Type == Storage_SmallPatchOnIsRayed.MESSAGE_TYPE)
                {
                    var msg = message as Message_Storage_Small_AdditionalData;
                    var storageManager = RAPI.GetLocalPlayer()?.StorageManager;

                    var storage = storageManager.GetStorageByObjectIndex(msg.storageObjectIndex);
                    var data = storage.GetAdditionalData();
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
        }

        public override void WorldEvent_WorldLoaded()
        {
            // TODO: request additional data from all storages from the host?

            //if (!Semih_Network.IsHost)
            //    Message_Storage_RequestLocks.Message.Send(ComponentManager<Semih_Network>.Value.HostID);
            //else if (ExtraSettingsAPI_Loaded)
            //    ChestLocks.CopyFrom(ExtraSettingsAPI_GetDataValue("locks", "data").Bytes());
            //string str = "World loaded with locks:";
            //foreach (var lockData in ChestLocks)
            //    str += "\n - " + lockData.Key + " locked by " + lockData.Value;
            //Debug.Log(str);
            //CanUnload = false;
        }

        public override void WorldEvent_OnPlayerConnected(CSteamID steamid, RGD_Settings_Character characterSettings)
        {
            base.WorldEvent_OnPlayerConnected(steamid, characterSettings);

            var network = ComponentManager<Raft_Network>.Value;
            foreach (Storage_Small storage in StorageManager.allStorages)
            {
                var data = storage.GetAdditionalData();

                if (data != null)
                {
                    // Broadcast the additional data to other players
                    var message = new Message_Storage_Small_AdditionalData(Storage_SmallPatchOnIsRayed.MESSAGE_TYPE, network.NetworkIDManager, data, storage);
                    RAPI.SendNetworkMessage(message, channel: Storage_SmallPatchOnIsRayed.CHANNEL_ID, fallbackSteamID: steamid); // TODO: Unsure if fallbackSteamID limits it to a specific player.
                }
            }
        }
    }
}
using HarmonyLib;
using System.Collections.Generic;
using thmsn.CraftFromAllStorage.Patches;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Network
{
    public static class NetworkMessageProcessor
    {
        public static List<Message> waiting = new List<Message>();

        public static void ListenAndProcess()
        {
            global::NetworkMessage netMessage = RAPI.ListenForNetworkMessagesOnChannel(Channel.DEFAULT);

            if (netMessage != null)
            {
                Message message = netMessage.message;

                if (CraftFromAllStorageMod.worldLoaded)
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
            switch (message.Type)
            {
                case Message_Storage_Small_AdditionalData.MESSAGE_TYPE:
                    Message_Storage_Small_AdditionalData.HandleAdditionalData(message as Message_Storage_Small_AdditionalData);
                    break;
                case Message_Storage_Small_AnimateOpen.MESSAGE_TYPE:
                    Message_Storage_Small_AnimateOpen.HandleNetworkMessage(message as Message_Storage_Small_AnimateOpen);
                    break;
                case Message_Storage_Small_AnimateClose.MESSAGE_TYPE:
                    Message_Storage_Small_AnimateClose.HandleNetworkMessage(message as Message_Storage_Small_AnimateClose);
                    break;
            }

            if (message.Type == Message_Storage_Small_AdditionalData.MESSAGE_TYPE)
            {
                
            }
        }

        
    }

    [HarmonyPatch(typeof(GameManager), "OnWorldRecievedLate")]
    class Patch_OnWorldRecievedLate
    {
        static void Postfix()
        {
            CraftFromAllStorageMod.worldLoaded = true;

            if (NetworkMessageProcessor.waiting.Count > 0)
            {
                foreach (var message in NetworkMessageProcessor.waiting)
                {
                    NetworkMessageProcessor.ProcessMessage(message);
                }

                NetworkMessageProcessor.waiting.Clear();
            }
        }
    }
}

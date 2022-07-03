using FMODUnity;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Extensions
{
    static class StorageSmallExtension
    {
        static public void AnimateAsOpen(this Storage_Small storage, Network_Player player)
        {
            var traverse = Traverse.Create(storage);
            //storage.IsOpen = true; // can't do this because it's private
            //traverse.Field("isOpen").SetValue(true);
            //storage.currentPlayer = player; // can't do this cause it's private
            //var eventRef_open = traverse.Field("eventRef_open").GetValue<string>();
            //RuntimeManager.PlayOneShotSafe(eventRef_open, storage.transform.position); // can't do this because it's private
            if (storage.anim != null)
            {
                //storage.anim.SetBool("IsOpen", storage.IsOpen);
                storage.anim.SetBool("IsOpen", true);
            }
            else
            {
                Debug.Log(".anim is null??");
            }
            //PlayerItemManager.IsBusy = true; /// unsure what this does 
            //storage.canCloseWithUsebutton = false; // unsure if this is needed.
            //this.CancelInvoke("AllowCloseWithUseButton");
            //this.Invoke("AllowCloseWithUseButton", 0.15f);
            //storage.UpdateStorageFillRenderers();
            //Traverse.Create(storage).Method("UpdateStorageFillRenderers");
        }

        static public void AnimateAsClosed(this Storage_Small storage, Network_Player player)
        {
            //storage.IsOpen = true; // can't do this because it's private
            //Traverse.Create(storage).Field("isOpen").SetValue(false);
            //storage.currentPlayer = player; // can't do this cause it's private
            //RuntimeManager.PlayOneShotSafe(storage.eventRef_close_squeal, storage.transform.position);
            //if (!storage.playCloseSoundEndOfAnimation)
            //{
            //    RuntimeManager.PlayOneShotSafe(storage.eventRef_close, storage.transform.position);
            //}

            if (storage.anim != null)
            {
                storage.anim.SetBool("IsOpen", false);
            }
            //PlayerItemManager.IsBusy = true; /// unsure what this does 
            //storage.canCloseWithUsebutton = false; // unsure if this is needed.
            //this.CancelInvoke("AllowCloseWithUseButton");
            //this.Invoke("AllowCloseWithUseButton", 0.15f);
        }
        static public IEnumerator AnimateAsClosedWithDelay(this Storage_Small storage, Network_Player player, float delay = 0.5f)
        {
            yield return new WaitForSeconds(delay);

            storage.AnimateAsClosed(player);
        }
    }
}

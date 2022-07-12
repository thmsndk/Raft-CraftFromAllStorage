using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace thmsn.CraftFromAllStorage.Patches
{
    [HarmonyPatch(typeof(Tank), nameof(Tank_BiofuelExtractor.OnIsRayed))]
    class Patch_Tank_BiofuelExtractor_OnIsRayed
    {
        private static void Postfix(Tank __instance, ref DisplayTextManager ___displayText, ref List<Item_Base> ___acceptableTypes, Network_Player ___localPlayer)
        {
            if (__instance is Tank_BiofuelExtractor)
            {
                ItemInstance selectedHotbarItem = ___localPlayer.Inventory.GetSelectedHotbarItem();
                if (selectedHotbarItem != null && ___acceptableTypes.Contains(selectedHotbarItem.baseItem) && __instance.defaultFuelToAdd != selectedHotbarItem.baseItem)
                {
                    __instance.defaultFuelToAdd = selectedHotbarItem.baseItem;
                }

                var keybind = "Rotate"; // TODO: make a changeable keybind?
                var currentAcceptableTypeIndex = ___acceptableTypes.IndexOf(__instance.defaultFuelToAdd);
                var nextIndex = currentAcceptableTypeIndex + 1;
                if (___acceptableTypes.Count == nextIndex)
                {
                    nextIndex = 0;
                }
                
                var nextAcceptableType = ___acceptableTypes.ElementAt(nextIndex);

                //___displayText.ShowText($"Change default to {nextAcceptableType.settings_Inventory.DisplayName}", MyInput.Keybinds[keybind].MainKey, 2, 0, false);
                ___displayText.ShowText($"DEFAULT: {__instance.defaultFuelToAdd.settings_Inventory.DisplayName}", MyInput.Keybinds[keybind].MainKey, 1, 0, false);

                if (MyInput.GetButtonDown(keybind))
                {
                    __instance.defaultFuelToAdd = nextAcceptableType;
                }
            }
        }
    }
}

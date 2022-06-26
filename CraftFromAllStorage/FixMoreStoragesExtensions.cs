using System.Collections.Generic;
using UnityEngine;

namespace thmsn.CraftFromAllStorage
{
    public static class FixMoreStoragesExtensions
    {
        /// <summary>
        /// MoreStorages adds two containers that seem to contain duplicate slots
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="uniqueItemName"></param>
        /// <returns></returns>
        public static int GetItemCountWithoutDuplicates(this Inventory inventory, string uniqueItemName)
        {
            if (GameModeValueManager.GetCurrentGameModeValue().playerSpecificVariables.unlimitedResources)
            {
                return int.MaxValue;
            }
            var visitedItemInstances = new HashSet<Slot>();
            int num = 0;
            //var index = 0;
            foreach (Slot slot in inventory.allSlots)
            {
                var slotNotVisisted = !visitedItemInstances.Contains(slot);
                if (!slot.IsEmpty && slot.itemInstance.UniqueName == uniqueItemName && slotNotVisisted)
                {
                    num += slot.itemInstance.Amount;
                    visitedItemInstances.Add(slot);
                    //Debug.Log($"{inventory.name} {slot.itemInstance.UniqueIndex} {slot.itemInstance.Amount}");
                }

                //if (!slot.IsEmpty && slot.itemInstance.UniqueName == uniqueItemName)
                //{
                //    Debug.LogWarning($"{inventory.name} index {index} itemInstance.UniqueIndex {slot.itemInstance.UniqueIndex} {slot.itemInstance.Amount}");
                //}

                //index++;
            }

            return num;
        }

    }
}
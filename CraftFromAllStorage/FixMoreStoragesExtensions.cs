using System.Collections.Generic;
using UnityEngine;

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
        
        foreach (Slot slot in inventory.allSlots)
        {
            if (!slot.IsEmpty && slot.itemInstance.UniqueName == uniqueItemName && !visitedItemInstances.Contains(slot))
            {
                num += slot.itemInstance.Amount;
                visitedItemInstances.Add(slot);
                //Debug.Log($"{inventory.name} {slot.itemInstance.UniqueIndex} {slot.itemInstance.Amount}");
            }
        }

        return num;
    }

}
class InventoryManager
{
    static public Inventory GetPlayerInventory()
    {
        return RAPI.GetLocalPlayer()?.Inventory;
    }

    /// <summary>
    /// Gets the currently open storage inventory for the player
    /// </summary>
    /// <returns></returns>
    static public Inventory GetCurrentStorageInventory()
    {
        return RAPI.GetLocalPlayer()?.StorageManager?.currentStorage?.GetInventoryReference();
    }
}

// Patches for Auto Recipe Redux support
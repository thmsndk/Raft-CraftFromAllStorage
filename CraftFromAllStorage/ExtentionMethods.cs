using HarmonyLib;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

static class ExtentionMethods
{
    //static Dictionary<Slot, Storage_Small> refs = new Dictionary<Slot, Storage_Small>();

    //public static Sprite GetReadable(this Sprite source) => Sprite.Create(source.texture.GetReadable(), source.rect, source.pivot, source.pixelsPerUnit);

    //public static Texture2D GetReadable(this Texture2D source)
    //{
    //    RenderTexture temp = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
    //    Graphics.Blit(source, temp);
    //    RenderTexture prev = RenderTexture.active;
    //    RenderTexture.active = temp;
    //    Texture2D texture = new Texture2D(source.width, source.height);
    //    texture.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
    //    texture.Apply();
    //    RenderTexture.active = prev;
    //    RenderTexture.ReleaseTemporary(temp);
    //    return texture;
    //}
    //public static Sprite CreateEmpty(this Sprite sprite)
    //{
    //    return Sprite.Create(new Texture2D(sprite.texture.width, sprite.texture.height, TextureFormat.RGBA32, false), sprite.rect, sprite.pivot, sprite.pixelsPerUnit);
    //}
    //public static Item_Base Clone(this Item_Base source, int uniqueIndex, string uniqueName)
    //{
    //    Item_Base item = ScriptableObject.CreateInstance<Item_Base>();
    //    item.Initialize(uniqueIndex, uniqueName, source.MaxUses);
    //    item.settings_buildable = source.settings_buildable.Clone();
    //    item.settings_consumeable = source.settings_consumeable.Clone();
    //    item.settings_cookable = source.settings_cookable.Clone();
    //    item.settings_equipment = source.settings_equipment.Clone();
    //    item.settings_Inventory = source.settings_Inventory.Clone();
    //    item.settings_recipe = source.settings_recipe.Clone();
    //    item.settings_usable = source.settings_usable.Clone();
    //    return item;
    //}

    //public static void SetRecipe(this ItemInstance_Recipe item, CostMultiple[] cost, int amountToCraft = 1)
    //{
    //    Traverse.Create(item).Field("amountToCraft").SetValue(amountToCraft);
    //    item.NewCost = cost;
    //}

    //public static int Missing(this Fuel fuel) => fuel.MaxFuel - fuel.GetFuelCount();

    //public static void ForceStart(this CookingTable table, SO_CookingTable_Recipe recipe)
    //{
    //    if (recipe == null)
    //        return;
    //    table.CurrentRecipe = recipe;
    //    table.CookTimer = 0f;
    //    table.Portions = 0;
    //    table.StartCoroutine(Traverse.Create(table).Method("SetAnimationState", new object[] { true, Traverse.Create(table).Field("startAnimDelay").GetValue<float>() }).GetValue<IEnumerator>());
    //    if (table is CookingTable_Pot)
    //        ((CookingTable_Pot)table).Fuel.StartBurning();
    //}

    //public static void Clear(this CookingTable table)
    //{
    //    table.Portions = 0;
    //    table.CurrentRecipe = null;
    //    Traverse.Create(table).Field("gooModel").GetValue<GameObject>().SetActive(false);
    //    Traverse.Create(table).Field("finishedModel").GetValue<GameObject>().SetActive(false);
    //}

    //public static void Clear(this PickupChanneling pickup)
    //{
    //    while (pickup.ItemsRemaining != 0)
    //        pickup.RemoveYield(null);
    //}

    //public static Storage_Small GetStorage(this Slot slot)
    //{
    //    if (refs.ContainsKey(slot))
    //    {
    //        if (refs[slot] == null)
    //        {
    //            var inv = Traverse.Create(slot).Field("inventory").GetValue<Inventory>();
    //            foreach (var storage in StorageManager.allStorages)
    //                if (storage.GetInventoryReference() == inv)
    //                {
    //                    refs[slot] = storage;
    //                    return storage;
    //                }
    //        }
    //        return refs[slot];
    //    }
    //    else
    //    {
    //        var inv = Traverse.Create(slot).Field("inventory").GetValue<Inventory>();
    //        foreach (var storage in StorageManager.allStorages)
    //            if (storage.GetInventoryReference() == inv)
    //            {
    //                refs.Add(slot, storage);
    //                return storage;
    //            }
    //    }
    //    return null;
    //}

    public static void BroadcastCloseEvent(this Storage_Small box)
    {
        var player = RAPI.GetLocalPlayer();
        var message = new Message_Storage_Close(Messages.StorageManager_Close, player.StorageManager, box);
        if (Semih_Network.IsHost)
        {
            // TODO. if we are sending multiple messages, we could potentially batch them together? e.g. removing items from multiple storages.
            message.Broadcast(NetworkChannel.Channel_Game);
        }
        else
        {
            message.Send(player.Network.HostID, NetworkChannel.Channel_Game);
        }
    }

    //public static bool Contains(this List<ItemInstance> items, Item_Base item)
    //{
    //    foreach (var i in items)
    //        if (i.Valid && i.baseItem == item)
    //            return true;
    //    return false;
    //}

    //public static int GetCount(this List<ItemInstance> items, Item_Base item)
    //{
    //    var c = 0;
    //    foreach (var i in items)
    //        if (i.baseItem == item)
    //            c += i.Amount;
    //    return c;
    //}

    //public static int GetCount(this List<ItemInstance> items, CostMultiple item)
    //{
    //    var c = 0;
    //    foreach (var i in items)
    //        if (item.ContainsItem(i.baseItem))
    //            c += i.Amount;
    //    return c;
    //}

    //public static void Remove(this List<ItemInstance> items, Item_Base item, int Count)
    //{
    //    for (int i = items.Count - 1; i >= 0; i--)
    //        if (items[i].baseItem == item)
    //        {
    //            if (items[i].Amount > Count)
    //            {
    //                items[i].Amount -= Count;
    //                return;
    //            }
    //            else
    //            {
    //                Count -= items[i].Amount;
    //                items.RemoveAt(i);
    //            }
    //        }
    //}

    //public static CookingTable_Recipe_UI FindRecipe(this CookingTable table, List<ItemInstance> items)
    //{
    //    CookingTable_Recipe_UI nearest = null;
    //    var min = float.MaxValue;
    //    foreach (var recipe in BenevolentSprites.recipes)
    //        if (recipe && recipe.Recipe)
    //        {
    //            var dist = (recipe.transform.position - table.transform.position).magnitude;
    //            if (dist < min && recipe.Recipe.RecipeCost.CanCraftFrom(items))
    //            {
    //                min = dist;
    //                nearest = recipe;
    //            }
    //        }
    //    if (min > 2)
    //        return null;
    //    return nearest;
    //}

    //public static bool CanCraftFrom(this CostMultiple[] costs, List<ItemInstance> items)
    //{
    //    foreach (CostMultiple cost in costs)
    //        if (items.GetCount(cost) < cost.amount)
    //            return false;
    //    return true;
    //}

    //public static List<ItemInstance> TakeItems(this Inventory inventory, CostMultiple[] costs)
    //{
    //    var items = new List<ItemInstance>();
    //    foreach (CostMultiple cost in costs)
    //    {
    //        var c = cost.amount;
    //        foreach (var slot in inventory.allSlots)
    //            if (slot.HasValidItemInstance() && cost.ContainsItem(slot.itemInstance.baseItem))
    //            {
    //                if (slot.itemInstance.Amount <= c)
    //                {
    //                    c -= slot.itemInstance.Amount;
    //                    items.Add(slot.itemInstance.Clone());
    //                    slot.SetItem(null);
    //                }
    //                else
    //                {
    //                    items.Add(new Cost(slot.itemInstance.baseItem, c));
    //                    slot.itemInstance.Amount -= c;
    //                    c = 0;
    //                }
    //                if (c == 0)
    //                    break;
    //            }
    //    }
    //    return items;
    //}

    //public static void Remove(this List<ItemInstance> items, CostMultiple[] costs)
    //{
    //    foreach (CostMultiple cost in costs)
    //    {
    //        var c = cost.amount;
    //        for (int i = items.Count - 1; i >= 0; i--)
    //            if (items[i].Valid && cost.ContainsItem(items[i].baseItem))
    //            {
    //                if (items[i].Amount <= c)
    //                {
    //                    c -= items[i].Amount;
    //                    items.RemoveAt(i);
    //                }
    //                else
    //                {
    //                    items[i].Amount -= c;
    //                    c = 0;
    //                }
    //                if (c == 0)
    //                    break;
    //            }
    //    }
    //}

    //public static List<ItemInstance> GetAllItems(this Inventory inventory)
    //{
    //    var items = new List<ItemInstance>();
    //    foreach (var slot in inventory.allSlots)
    //        if (slot.HasValidItemInstance())
    //            items.Add(slot.itemInstance);
    //    return items;
    //}

    //public static List<ItemInstance> GetAllItems(this PickupItem pickup)
    //{
    //    var items = new List<ItemInstance>();
    //    if (pickup.yieldHandler)
    //        items.AddAll(pickup.yieldHandler.Yield);
    //    if (pickup.dropper)
    //        items.AddAll(pickup.dropper.GetRandomItems());
    //    if (pickup.itemInstance != null && pickup.itemInstance.Valid)
    //        items.Add(pickup.itemInstance);
    //    if (pickup.specificPickups != null)
    //    {
    //        var tempObj = new GameObject();
    //        Patch_InventoryOverrides.ignore = tempObj;
    //        tempObj.SetActive(false);
    //        var tempInv = tempObj.AddComponent<PlayerInventory>();
    //        tempInv.hotslotCount = 1;
    //        tempInv.hotbar = tempObj.AddComponent<Hotbar>();
    //        Traverse.Create(tempInv).Field("inventoryPickup").SetValue(tempObj.AddComponent<InventoryPickup>());
    //        var tempTxt = new GameObject().AddComponent<Text>();
    //        for (int i = 0; i < 20; i++)
    //        {
    //            var s = new GameObject().AddComponent<Slot>();
    //            s.rectTransform = s.gameObject.AddComponent<RectTransform>();
    //            s.active = true;
    //            s.textComponent = tempTxt;
    //            tempInv.allSlots.Add(s);
    //        }
    //        foreach (var specificPickup in pickup.specificPickups)
    //        {
    //            specificPickup.PickupSpecific(tempInv);
    //            items.AddRange(tempInv.GetAllItems());
    //            foreach (var slot in tempInv.allSlots)
    //                slot.SetItem(null);
    //        }
    //        foreach (var slot in tempInv.allSlots)
    //            GameObject.Destroy(slot);
    //        GameObject.Destroy(tempTxt.gameObject);
    //        GameObject.Destroy(tempObj);
    //    }
    //    return items;
    //}

    //public static void AddAll(this List<ItemInstance> items, IEnumerable<Cost> newItems)
    //{
    //    if (newItems != null)
    //        foreach (var cost in newItems)
    //            if (cost != null)
    //                items.Add(new ItemInstance(cost.item, cost.amount, cost.item.MaxUses));
    //}
    //public static void AddAll(this List<ItemInstance> items, IEnumerable<Item_Base> newItems)
    //{
    //    if (newItems != null)
    //        foreach (var item in newItems)
    //            if (item != null)
    //                items.Add(new ItemInstance(item, 1, item.MaxUses));
    //}
    //public static void AddAll(this List<ItemInstance> items, IEnumerable<ItemInstance> newItems)
    //{
    //    if (newItems != null)
    //        foreach (var item in newItems)
    //            if (item != null)
    //                items.Add(item);
    //}
    //public static void Add(this List<ItemInstance> items, Cost cost)
    //{
    //    if (cost != null)
    //        items.Add(new ItemInstance(cost.item, cost.amount, cost.item.MaxUses));
    //}
    //public static void Add(this List<ItemInstance> items, Item_Base item)
    //{
    //    if (item)
    //        items.Add(new ItemInstance(item, 1, item.MaxUses));
    //}
    //public static void Remove(this PickupItem_Networked pickup)
    //{
    //    if (pickup.stopTrackUseRPC)
    //    {
    //        PickupObjectManager.RemovePickupItemNetwork(pickup, new CSteamID(0));
    //    }
    //    else
    //    {
    //        PickupObjectManager.RemovePickupItem(pickup, new CSteamID(0));
    //    }
    //}
    //public static Item_Base FindItem(this Inventory inventory, CookingStand stand, CookingSlot slot, out List<CookingSlot> slots)
    //{
    //    slots = null;
    //    foreach (var s in inventory.allSlots)
    //        if (s.HasValidItemInstance() && slot.CanCookItem(s.itemInstance.baseItem))
    //        {
    //            slots = stand.FindSlots(s.itemInstance.baseItem, slot);
    //            if (slots != null)
    //                return s.itemInstance.baseItem;
    //        }
    //    return null;
    //}

    //public static List<CookingSlot> FindSlots(this CookingStand stand, Item_Base item, CookingSlot containsSlot)
    //{
    //    if (item == null)
    //        return null;
    //    List<CookingSlot> list = new List<CookingSlot>();
    //    for (int i = 0; i < stand.cookingSlots.Length; i++)
    //    {
    //        CookingSlot cookingSlot = stand.cookingSlots[i];
    //        if (!cookingSlot.IsBusy && cookingSlot.CanCookItem(item))
    //        {
    //            list.Add(cookingSlot);
    //            if (list.Count >= item.settings_cookable.CookingSlotsRequired)
    //            {
    //                if (list.Contains(containsSlot))
    //                    return list;
    //                else
    //                    list.RemoveAt(0);
    //            }
    //        }
    //    }
    //    return null;
    //}

    //public static bool IsRepelled(this Transform t)
    //{
    //    foreach (var g in SpriteRepellent.repellents)
    //        if (UnityEngine.Vector3.Distance(t.position, g.transform.position) < 1.5)
    //            return true;
    //    return false;
    //}
    //public static bool IsRepelled(this Transform t, Vector3 off)
    //{
    //    foreach (var g in SpriteRepellent.repellents)
    //        if (UnityEngine.Vector3.Distance(t.TransformPoint(off), g.transform.position) < 1.5)
    //            return true;
    //    return false;
    //}

    //public static string String(this byte[] bytes, int length = -1, int offset = 0)
    //{
    //    string str = "";
    //    if (length == -1)
    //        length = (bytes.Length - offset) / 2;
    //    while (str.Length < length)
    //    {
    //        str += System.BitConverter.ToChar(bytes, offset + str.Length * 2);
    //    }
    //    return str;

    //}
    //public static string String(this List<byte> bytes) => bytes.ToArray().String();
    //public static byte[] Bytes(this string str)
    //{
    //    var data = new List<byte>();
    //    foreach (char chr in str)
    //        data.AddRange(System.BitConverter.GetBytes(chr));
    //    return data.ToArray();
    //}
    //public static int Integer(this byte[] bytes, int offset = 0) => System.BitConverter.ToInt32(bytes, offset);
    //public static uint UInteger(this byte[] bytes, int offset = 0) => System.BitConverter.ToUInt32(bytes, offset);
    //public static float Float(this byte[] bytes, int offset = 0) => System.BitConverter.ToSingle(bytes, offset);
    //public static Vector3 Vector3(this byte[] bytes, int offset = 0) => new Vector3(bytes.Float(offset), bytes.Float(offset + 4), bytes.Float(offset + 8));
    //public static ItemInstance Item(this byte[] bytes, out int size, int offset = 0)
    //{
    //    var stringSize = bytes.Integer(offset + 12);
    //    size = stringSize / 2 + 16;
    //    return new ItemInstance(ItemManager.GetItemByIndex(bytes.Integer(offset)), bytes.Integer(offset + 4), bytes.Integer(offset + 8), bytes.String(stringSize, offset + 16));
    //}
    //public static List<ItemInstance> Items(this byte[] bytes, out int size, int offset = 0)
    //{
    //    var items = new List<ItemInstance>();
    //    var count = bytes.Integer(offset);
    //    size = 4;
    //    while (items.Count < count)
    //    {
    //        items.Add(bytes.Item(out int inc, offset + size));
    //        size += inc;
    //    }
    //    return items;
    //}
    //public static CookingSlotTargets CookingSlotTargets(this byte[] bytes, int offset = 0) => new CookingSlotTargets(bytes.UInteger(offset), bytes.Integer(offset + 4), bytes.Integer(offset + 8));
    //public static byte[] Bytes(this int value) => System.BitConverter.GetBytes(value);
    //public static byte[] Bytes(this uint value) => System.BitConverter.GetBytes(value);
    //public static byte[] Bytes(this float value) => System.BitConverter.GetBytes(value);
    //public static byte[] Bytes(this Vector3 value)
    //{
    //    var data = new byte[12];
    //    value.x.Bytes().CopyTo(data, 0);
    //    value.y.Bytes().CopyTo(data, 4);
    //    value.z.Bytes().CopyTo(data, 8);
    //    return data;
    //}
    //public static byte[] Bytes(this ItemInstance value)
    //{
    //    var data = new List<byte>();
    //    data.AddRange(value.baseItem.UniqueIndex.Bytes());
    //    data.AddRange(value.Amount.Bytes());
    //    data.AddRange(value.Uses.Bytes());
    //    data.AddRange(value.exclusiveString.Length.Bytes());
    //    data.AddRange(value.exclusiveString.Bytes());
    //    return data.ToArray();
    //}
    //public static byte[] Bytes(this IEnumerable<ItemInstance> values)
    //{
    //    var data = new List<byte>();
    //    int count = 0;
    //    foreach (var item in values)
    //    {
    //        data.AddRange(item.Bytes());
    //        count++;
    //    }
    //    data.InsertRange(0, count.Bytes());
    //    return data.ToArray();
    //}
    //public static byte[] Bytes(this CookingSlotTargets value)
    //{
    //    var data = new byte[12];
    //    value.ObjectIndex.Bytes().CopyTo(data, 0);
    //    value.slotInd.Bytes().CopyTo(data, 4);
    //    value.slotCount.Bytes().CopyTo(data, 8);
    //    return data;
    //}

    public static void Broadcast(this Message message, NetworkChannel channel/* = (NetworkChannel)MessageType.ChannelID*/) => ComponentManager<Semih_Network>.Value.RPC(message, Target.Other, EP2PSend.k_EP2PSendReliable, channel);
    public static void Send(this Message message, CSteamID steamID, NetworkChannel channel) => ComponentManager<Semih_Network>.Value.SendP2P(steamID, message, EP2PSend.k_EP2PSendReliable, channel);
}

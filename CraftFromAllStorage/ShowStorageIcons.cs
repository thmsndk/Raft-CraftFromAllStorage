//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//// TODO: when holding the hammer, requirements are rendered, could probably use that to render contents of the current storage you are looking at.
//class ShowStorageIcons : MonoBehaviour
//{
//    static float ICON_TRANSPARENCY = .75f;

//    static GameObject firePrefab;
//    public static GameObject FirePrefab
//    {
//        get
//        {
//            if (firePrefab == null)
//                foreach (var p in Resources.FindObjectsOfTypeAll<ParticleSystemRenderer>())
//                    if (p.name == "Fire_v2")
//                    { firePrefab = p.gameObject; break; }
//            return firePrefab;
//        }
//    }

//    public void Start()
//    {

//        Color iconColor = new Color(1, 1, 1, ICON_TRANSPARENCY);

//        Debug.Log("Finding all storages");


//        var storages = StorageManager.allStorages;

//        foreach (var storage in storages)
//        {
//            /*
//             * if (GameModeValueManager.GetCurrentGameModeValue().playerSpecificVariables.unlimitedResources)
//          {
//           return int.MaxValue;
//          }
//             */
//            var inventory = storage.GetInventoryReference();
//            var slots = inventory.allSlots;
//            foreach (var slot in slots)
//            {
//                if (slot.IsEmpty)
//                {
//                    continue;
//                }

//                var buffItemIcon = GameObject.Instantiate(FirePrefab, GameManager.Singleton.lockedPivot, false);//.AddComponent<ItemIcon>();
//                buffItemIcon.gameObject.transform.SetParent(storage.transform);

//                buffItemIcon.objectItem = item;

//                buffItemIcon.image = buffItemIcon.GetComponentInChildren<Image>();
//                buffItemIcon.image.color = iconColor;
//                buffItemIcon.image.sprite = itemInfos[i].sprite;

//                buffItemIcon.outline = Instantiate(buffItemIcon.image, buffItemIcon.image.transform.parent);
//                buffItemIcon.outline.transform.SetAsFirstSibling();
//                buffItemIcon.outline.color = Color.black;
//                buffItemIcon.outline.transform.localScale *= 1.15f;


//            }

//            // TODO: attach behaviours to storages , allowing us to render icons above them, would also allow it to check the inventory itself.
//        }
//    }

//    //public void DisplayIcons(int i, bool all = false)
//    //{
//    //    if (all)
//    //    {
//    //        for (int x = 0; x < itemInfos.Length; x++)
//    //        {
//    //            for (int o = 0; o < maxItemsPerKind; o++)
//    //            {
//    //                if (AllIcons2d[x, o] != null)
//    //                {
//    //                    //RConsole.Log(o.ToString());
//    //                    if (buttons[i].isActive)
//    //                    {
//    //                        if (AllIcons2d[x, o].objectItem.gameObject.activeSelf)
//    //                        {
//    //                            AllIcons2d[x, o].image.enabled = true;
//    //                            AllIcons2d[x, o].outline.enabled = true;
//    //                        }
//    //                    }
//    //                    else
//    //                    {
//    //                        AllIcons2d[x, o].image.enabled = false;
//    //                        AllIcons2d[x, o].outline.enabled = false;
//    //                    }

//    //                }
//    //            }
//    //        }
//    //    }
//    //    else
//    //    {
//    //        for (int o = 0; o < maxItemsPerKind; o++)
//    //        {
//    //            if (AllIcons2d[i, o] != null)
//    //            {
//    //                if (buttons[i].isActive)
//    //                {
//    //                    if (AllIcons2d[i, o].objectItem.gameObject.activeSelf)
//    //                    {
//    //                        AllIcons2d[i, o].image.enabled = true;
//    //                        AllIcons2d[i, o].outline.enabled = true;
//    //                    }
//    //                }
//    //                else
//    //                {
//    //                    AllIcons2d[i, o].image.enabled = false;
//    //                    AllIcons2d[i, o].outline.enabled = false;
//    //                }
//    //            }
//    //        }
//    //    }
//    //}

//    //public void RefreshWallhackPosition()
//    //{
//    //    if (currentIsland == null)
//    //        return;

//    //    ItemIcon buffIcon; ;
//    //    bool showIcon = false;

//    //    for (int i = 0; i < itemInfos.Length; i++)
//    //    {
//    //        if (buttons[i].isActive)
//    //        {
//    //            for (int o = 0; o < maxItemsPerKind; o++)
//    //            {
//    //                if (AllIcons2d[i, o] != null)
//    //                {
//    //                    buffIcon = AllIcons2d[i, o];
//    //                    showIcon = buffIcon.objectItem.gameObject.activeSelf;

//    //                    if (showIcon)
//    //                    {
//    //                        if (Vector3.Dot(Camera.main.transform.forward, (buffIcon.objectItem.transform.position - Camera.main.transform.position).normalized) <= 0)
//    //                        {
//    //                            showIcon = false;
//    //                        }
//    //                    }

//    //                    buffIcon.image.enabled = showIcon;
//    //                    buffIcon.outline.enabled = showIcon;

//    //                    buffIcon.transform.position = Camera.main.WorldToScreenPoint(buffIcon.objectItem.transform.position);
//    //                }
//    //            }
//    //        }
//    //    }
//    //}

//    //public void CreateIcons()
//    //{

//    //    if (itemIconParent != null)
//    //    {
//    //        Destroy(itemIconParent.gameObject);
//    //    }
//    //    if (currentIsland == null)
//    //        return;

//    //    itemIconParent = new GameObject().transform;
//    //    itemIconParent.parent = newCanvas.transform;
//    //    itemIconParent.SetAsFirstSibling();
//    //    itemIconParent.gameObject.SetActive(buttonWallhack.isActive);
//    //    iconDict.Clear();

//    //    int[] itemCounter = new int[itemInfos.Length];

//    //    Color iconColor = new Color(1, 1, 1, ICON_TRANSPARENCY);

//    //    ItemIcon buffItemIcon;

//    //    for (int h = 0; h < currentIsland.landmarkItems.Length; h++)
//    //    {
//    //        PickupItem item = currentIsland.landmarkItems[h].connectedBehaviourID.GetComponent<PickupItem>();

//    //        if (item != null)
//    //        {
//    //            for (int i = 0; i < itemInfos.Length; i++)
//    //            {
//    //                if (item.name.Contains(itemInfos[i].unityName) && item.gameObject.activeSelf)
//    //                {
//    //                    buffItemIcon = Instantiate(iconImageObject, itemIconParent).AddComponent<ItemIcon>();

//    //                    buffItemIcon.objectItem = item;

//    //                    buffItemIcon.image = buffItemIcon.GetComponentInChildren<Image>();
//    //                    buffItemIcon.image.color = iconColor;
//    //                    buffItemIcon.image.sprite = itemInfos[i].sprite;

//    //                    buffItemIcon.outline = Instantiate(buffItemIcon.image, buffItemIcon.image.transform.parent);
//    //                    buffItemIcon.outline.transform.SetAsFirstSibling();
//    //                    buffItemIcon.outline.color = Color.black;
//    //                    buffItemIcon.outline.transform.localScale *= 1.15f;

//    //                    itemCounter[i]++;
//    //                    AllIcons2d[i, itemCounter[i]] = buffItemIcon;
//    //                    iconDict.Add(item, buffItemIcon);
//    //                }
//    //            }
//    //        }
//    //    }
//    //}
//}

////public class ItemInfo
////{
////    public readonly string unityName;
////    public readonly string uniqueName;
////    public readonly string displayName;
////    public Sprite sprite;
////    public ItemInfo(string unityName, string uniqueName, string displayName)
////    {
////        this.unityName = unityName;
////        this.uniqueName = uniqueName;
////        this.displayName = displayName;
////    }
////}

////class ItemIcon : MonoBehaviour
////{
////    public Image image;
////    public Image outline;

////    public PickupItem objectItem;
////}

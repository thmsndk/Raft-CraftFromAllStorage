using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/*
 * Ideas
 * Render a sprite above / in front of a container like the island loot mod? - perhaps when you focus it (OnRayed?
 * An Item Sign that renders what is inside a storage chest?
 * All In One inventory that opens by default when you press tab? - should render all storages on your raft.
 * Craft From storage, should craft from all chests, not only your local one.
 * - if you have a storage open, prefer taking items from that one. else take it from the other storages.
 * Hold modifiers to increase how many are crafted when pressing the craft button.
 */

/*
 on exit to menu while crafting window is open 
NullReferenceException: Object reference not set to an instance of an object
InventoryManager.GetCurrentStorageInventory () (at <cb793ae125e749f3af162aa6454d7e2d>:0)
SetAmountInInventoryPatch.Postfix (PlayerInventory inventory, BuildingUI_CostBox __instance) (at <cb793ae125e749f3af162aa6454d7e2d>:0)
(wrapper dynamic-method) BuildingUI_CostBox.DMD<DMD<SetAmountInInventory_Patch1>?-1584877824::SetAmountInInventory_Patch1>(BuildingUI_CostBox,PlayerInventory)
CostCollection.ShowCost (CostMultiple[] cost) (at <1625233a19c64a01aa83bf62cbe0e622>:0)
BuildMenu.Update () (at <1625233a19c64a01aa83bf62cbe0e622>:0)
 
 */

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

    public override void WorldEvent_WorldLoaded()
    {
        var recipeBox = ComponentManager<CraftingMenu>.Value.selectedRecipeBox;
        var craftModifierKeys = recipeBox.gameObject.AddComponent<CraftModifierKeys>();
        craftModifierKeys.SelectedRecipeBox = recipeBox;



        base.WorldEvent_WorldLoaded();
    }

    public void OnModUnload()
    {
        Debug.Log(ModNamePrefix + " has been unloaded!");
        harmony.UnpatchAll(harmonyId);
        Destroy(gameObject);
    }
}

// Perhaps we can use the transpiler to rewrite CraftItem, to loop amountToCraft vs stacksize, alternatively we just need to add a postfix that does that -1

public class CraftModifierKeys : MonoBehaviour
{


    public SelectedRecipeBox SelectedRecipeBox { get; set; }
    private Text craftButtonText;
    private string originalCraftButtonText;
    private int multiplier = 10;
    private Dictionary<string, int> originalNewCost = new Dictionary<string, int>();
    private Item_Base originalItem;
    private bool leftShiftModifier = false;

    public static int amountToCraft = 1;
    
    private void Start()
    {
        Debug.Log("CraftModifierKeys started");
        craftButtonText = SelectedRecipeBox.craftButton.GetComponentInChildren<Text>();
        originalCraftButtonText = craftButtonText.text;
    }

    private void Update()
    {
        if (SelectedRecipeBox.ItemToCraft == null)
        {
            // TODO: Reset things.
            return;
        }

        // We got a new selected recipe, or the first one.
        if (originalItem != SelectedRecipeBox.ItemToCraft)
        {
            Debug.Log(CraftFromAllStorageMod.ModNamePrefix + " A new recipe was selected, restoring original cost.");

            if (originalItem != null)
            {
                RestoreOriginalCostMultiple();
            }

            if (SelectedRecipeBox.ItemToCraft != null)
            {
                originalItem = SelectedRecipeBox.ItemToCraft;
                CacheOriginalCostMultiple();
            }
        }

        // TODO: we need to clone the original cost to be able to restore the original costmultiple.
        if (Input.GetKeyDown(KeyCode.LeftShift) && !leftShiftModifier)
        {
            leftShiftModifier = true;

            //Debug.Log("Left shift pressed");
            craftButtonText.text = originalCraftButtonText + " x " + multiplier;

            // TODO: this causes 10 hammers to be added to the slot, this is not what we want, it should add a hammer 10 times. depending on stack size
            //Traverse.Create(SelectedRecipeBox.ItemToCraft.settings_recipe).Field("amountToCraft").SetValue(SelectedRecipeBox.ItemToCraft.settings_recipe.AmountToCraft * multiplier);

            if (SelectedRecipeBox.ItemToCraft != null)
            {
                ModifyCostMultiple(SelectedRecipeBox.ItemToCraft.settings_recipe.NewCost, multiplier);
                amountToCraft = SelectedRecipeBox.ItemToCraft.settings_recipe.AmountToCraft * multiplier;
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            leftShiftModifier = false;

            craftButtonText.text = originalCraftButtonText;
            //Traverse.Create(SelectedRecipeBox.ItemToCraft.settings_recipe).Field("amountToCraft").SetValue(SelectedRecipeBox.ItemToCraft.settings_recipe.AmountToCraft / multiplier);

            amountToCraft = SelectedRecipeBox.ItemToCraft.settings_recipe.AmountToCraft;

            RestoreOriginalCostMultiple();
        }

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            //Debug.Log("Left alt pressed");
        }
    }

    private void ModifyCostMultiple(CostMultiple[] newCost, int multiplier)
    {
        foreach (var costMultiple in newCost)
        {
            costMultiple.amount *= multiplier;
        }
    }

    private void RestoreOriginalCostMultiple()
    {
        foreach (var costMultiple in SelectedRecipeBox.ItemToCraft.settings_recipe.NewCost)
        {
            foreach (var item in costMultiple.items)
            {
                if (originalNewCost.TryGetValue(item.UniqueName, out var amount))
                {
                    costMultiple.amount = amount;
                }
            }
        }
    }

    private void CacheOriginalCostMultiple()
    {
        originalNewCost.Clear();

        foreach (var costMultiple in SelectedRecipeBox.ItemToCraft.settings_recipe.NewCost)
        {
            foreach (var item in costMultiple.items)
            {
                originalNewCost.Add(item.UniqueName, costMultiple.amount);
            }
        }
    }
}
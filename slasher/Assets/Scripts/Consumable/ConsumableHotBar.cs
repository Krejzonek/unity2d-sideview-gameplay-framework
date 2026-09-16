using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableHotbar : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private Button foodSlot;
    [SerializeField] private Button potionSlot;

    [Header("Selection Panel")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Transform itemListContent;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private GameObject ingredientButtonPrefab;

    [Header("Filter Buttons")]
    [SerializeField] private Button allButton;
    [SerializeField] private Button foodButton;
    [SerializeField] private Button potionButton;
    

    [Header("Player")]
    [SerializeField] private ConsumableManager consumableManager;
    [SerializeField] private ConsumableInventory consumableInventory;
    [SerializeField] private IngredientInventory ingredientInventory;

    [Header("Selected Items")]
    [SerializeField] private ConsumableData foodItem;
    [SerializeField] private ConsumableData potionItem;


    private int selectedSlot = -1;

    private ButtonScript buttonScript;

    private void Start()
    {
        foodSlot.onClick.AddListener(UseFood);
        potionSlot.onClick.AddListener(UsePotion);

        allButton.onClick.AddListener(ShowAllItems);
        foodButton.onClick.AddListener(() => ShowItems(ConsumableType.Food));
        potionButton.onClick.AddListener(() => ShowItems(ConsumableType.Potion));

        selectionPanel.SetActive(false);

        GameObject cameraObject = GameObject.FindGameObjectWithTag("MainCamera");

        if (cameraObject != null)
        {
            buttonScript = cameraObject.GetComponent<ButtonScript>();
        }
    }

    public void SelectFoodSlot()
    {
        selectedSlot = 0;

        selectionPanel.SetActive(true);

        ShowAllItems();
    }

    public void SelectPotionSlot()
    {
        selectedSlot = 1;

        selectionPanel.SetActive(true);

        ShowAllItems();
    }

    private void ShowAllItems()
    {
        ClearList();

        if (ingredientInventory == null)
            return;

        List<IngredientInventory.IngredientStack> items = ingredientInventory.GetIngredients();

        foreach (IngredientInventory.IngredientStack stack in items)
        {
            if (stack == null)
                continue;

            if (stack.ingredient == null)
                continue;

            if (stack.amount <= 0)
                continue;

            CreateIngredientButton(stack.ingredient, stack.amount);
        }
    }

    private void ShowItems(ConsumableType type)
    {
        ClearList();

        if (consumableInventory == null)
            return;

        List<ConsumableInventory.ConsumableStack> items = consumableInventory.GetConsumables();

        foreach (ConsumableInventory.ConsumableStack stack in items)
        {
            if (stack == null)
                continue;

            if (stack.item == null)
                continue;

            if (stack.amount <= 0)
                continue;

            if (stack.item.consumableType != type)
                continue;

            if (selectedSlot == 0 && stack.item.consumableType != ConsumableType.Food)
                continue;

            if (selectedSlot == 1 && stack.item.consumableType != ConsumableType.Potion)
                continue;

            CreateItemButton(stack.item);
        }
    }

    private void CreateItemButton(ConsumableData item)
    {
        GameObject buttonObject = Instantiate(itemButtonPrefab, itemListContent);

        ConsumableItemButton itemButton = buttonObject.GetComponent<ConsumableItemButton>();

        if (itemButton == null)
            return;

        itemButton.Setup(item, this);
    }

    private void ClearList()
    {
        foreach (Transform child in itemListContent)
        {
            Destroy(child.gameObject);
        }
    }

    public void SelectItem(ConsumableData item)
    {
        if (item == null)
            return;

        if (consumableInventory == null)
            return;

        if (!consumableInventory.HasConsumable(item, 1))
            return;

        if (selectedSlot == 0)
        {
            if (item.consumableType != ConsumableType.Food)
                return;

            foodItem = item;
        }
        else if (selectedSlot == 1)
        {
            if (item.consumableType != ConsumableType.Potion)
                return;

            potionItem = item;
        }

        CloseSelection();
    }

    public void UseFood()
    {
        if (foodItem == null)
            return;

        if (consumableManager == null)
            return;

        if (consumableInventory == null)
            return;

        if (!consumableInventory.HasConsumable(foodItem, 1))
        {
            foodItem = null;
            return;
        }

        consumableInventory.RemoveConsumable(foodItem, 1);

        consumableManager.UseConsumable(foodItem);

        if (consumableInventory.GetAmount(foodItem) <= 0)
        {
            foodItem = null;
        }
    }

    public void UsePotion()
    {
        if (potionItem == null)
            return;

        if (consumableManager == null)
            return;

        if (consumableInventory == null)
            return;

        if (!consumableInventory.HasConsumable(potionItem, 1))
        {
            potionItem = null;
            return;
        }

        consumableInventory.RemoveConsumable(potionItem, 1);

        consumableManager.UseConsumable(potionItem);

        if (consumableInventory.GetAmount(potionItem) <= 0)
        {
            potionItem = null;
        }
    }

    private void CloseSelection()
    {
        selectionPanel.SetActive(false);

        if (buttonScript != null)
        {
            buttonScript.consumablePanelVisible = false;
        }

        selectedSlot = -1;
    }

    public ConsumableData GetFood()
    {
        return foodItem;
    }

    public ConsumableData GetPotion()
    {
        return potionItem;
    }
    private void CreateIngredientButton(IngredientData ingredient, int amount)
    {
        GameObject buttonObject = Instantiate(ingredientButtonPrefab, itemListContent);

        IngredientItemButton itemButton = buttonObject.GetComponent<IngredientItemButton>();

        if (itemButton == null)
            return;

        itemButton.Setup(ingredient, amount);
    }
    public void SelectItemFromDescription(ConsumableData item)
    {
        if (item == null)
            return;

        if (consumableInventory == null)
            return;

        if (!consumableInventory.HasConsumable(item, 1))
            return;

        if (selectedSlot == 0)
        {
            if (item.consumableType != ConsumableType.Food)
                return;

            foodItem = item;
        }
        else if (selectedSlot == 1)
        {
            if (item.consumableType != ConsumableType.Potion)
                return;

            potionItem = item;
        }

        CloseSelection();
    }
    public void ShowConsumableDescription(ConsumableData item)
    {
        if (ItemDescriptionPanel.Instance == null)
            return;

        ItemDescriptionPanel.Instance.ShowConsumable(item, this);
    }
}

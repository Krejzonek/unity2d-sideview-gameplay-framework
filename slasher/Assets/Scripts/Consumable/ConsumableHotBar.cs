
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

    [Header("Consumable System")]
    [SerializeField] private ConsumableManager consumableManager;

    [Header("Selected Items")]
    [SerializeField] private ConsumableData foodItem;
    [SerializeField] private ConsumableData potionItem;

    private ConsumableInventory consumableInventory;
    private IngredientInventory ingredientInventory;
    private int selectedSlot = -1;
    private int activeFilter;
    private int lastUseFrame = -1;
    private ButtonScript buttonScript;

    private void Start()
    {
        if (consumableManager == null)
            consumableManager = FindFirstObjectByType<ConsumableManager>();

        if (consumableManager != null)
        {
            consumableInventory = consumableManager.GetConsumableInventory();
            ingredientInventory = consumableManager.GetIngredientInventory();
        }

        if (foodSlot != null)
            foodSlot.onClick.AddListener(UseFood);

        if (potionSlot != null)
            potionSlot.onClick.AddListener(UsePotion);

        if (allButton != null)
            allButton.onClick.AddListener(ShowAllItems);

        if (foodButton != null)
            foodButton.onClick.AddListener(() => ShowItems(ConsumableType.Food));

        if (potionButton != null)
            potionButton.onClick.AddListener(() => ShowItems(ConsumableType.Potion));

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        GameObject cameraObject = GameObject.FindGameObjectWithTag("MainCamera");

        if (cameraObject != null)
            buttonScript = cameraObject.GetComponent<ButtonScript>();
    }

    public void SelectFoodSlot()
    {
        selectedSlot = 0;

        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        ShowAllItems();
    }

    public void SelectPotionSlot()
    {
        selectedSlot = 1;

        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        ShowAllItems();
    }

    public void ShowAllItems()
    {
        activeFilter = 0;
        RefreshContent();
    }

    private void ShowItems(ConsumableType type)
    {
        activeFilter = type == ConsumableType.Food ? 1 : 2;
        RefreshContent();
    }

    public void RefreshContent()
    {
        ClearList();

        if (selectionPanel == null || !selectionPanel.activeSelf)
            return;

        if (activeFilter == 0)
        {
            if (ingredientInventory == null)
                return;

            List<IngredientInventory.IngredientStack> items = ingredientInventory.GetIngredients();

            foreach (IngredientInventory.IngredientStack stack in items)
            {
                if (stack == null || stack.ingredient == null || stack.amount <= 0)
                    continue;

                CreateIngredientButton(stack.ingredient, stack.amount);
            }

            return;
        }

        if (consumableInventory == null)
            return;

        List<ConsumableInventory.ConsumableStack> consumables = consumableInventory.GetConsumables();

        foreach (ConsumableInventory.ConsumableStack stack in consumables)
        {
            if (stack == null || stack.item == null || stack.amount <= 0)
                continue;

            if (activeFilter == 1 && stack.item.consumableType != ConsumableType.Food)
                continue;

            if (activeFilter == 2 && stack.item.consumableType != ConsumableType.Potion)
                continue;

            if (selectedSlot == 0 && stack.item.consumableType != ConsumableType.Food)
                continue;

            if (selectedSlot == 1 && stack.item.consumableType != ConsumableType.Potion)
                continue;

            CreateItemButton(stack.item, stack.amount);
        }
    }

    private void CreateItemButton(ConsumableData item, int amount)
    {
        GameObject buttonObject = Instantiate(itemButtonPrefab, itemListContent);
        ConsumableItemButton itemButton = buttonObject.GetComponent<ConsumableItemButton>();

        if (itemButton != null)
            itemButton.Setup(item, this, amount);
        else
            Destroy(buttonObject);
    }

    private void CreateIngredientButton(IngredientData ingredient, int amount)
    {
        GameObject buttonObject = Instantiate(ingredientButtonPrefab, itemListContent);
        IngredientItemButton itemButton = buttonObject.GetComponent<IngredientItemButton>();

        if (itemButton != null)
            itemButton.Setup(ingredient, amount);
        else
            Destroy(buttonObject);
    }

    private void ClearList()
    {
        if (itemListContent == null)
            return;

        foreach (Transform child in itemListContent)
            Destroy(child.gameObject);
    }

    public void SelectItem(ConsumableData item)
    {
        if (item == null || consumableInventory == null)
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
        else
            return;

        CloseSelection();
    }

    public void UseFood()
    {
        if (foodItem == null || lastUseFrame == Time.frameCount)
            return;

        if (consumableManager == null || consumableInventory == null || TeamManager.Instance == null)
            return;

        Player player = TeamManager.Instance.CurrentPlayer;

        if (player == null)
            return;

        if (!consumableInventory.HasConsumable(foodItem, 1))
        {
            foodItem = null;
            RefreshContent();
            return;
        }

        if (!consumableInventory.RemoveConsumable(foodItem, 1))
            return;

        lastUseFrame = Time.frameCount;
        consumableManager.UseConsumable(foodItem, player);

        if (consumableInventory.GetAmount(foodItem) <= 0)
            foodItem = null;

        RefreshContent();
    }

    public void UsePotion()
    {
        if (potionItem == null || lastUseFrame == Time.frameCount)
            return;

        if (consumableManager == null || consumableInventory == null || TeamManager.Instance == null)
            return;

        Player player = TeamManager.Instance.CurrentPlayer;

        if (player == null)
            return;

        if (!consumableInventory.HasConsumable(potionItem, 1))
        {
            potionItem = null;
            RefreshContent();
            return;
        }

        if (!consumableInventory.RemoveConsumable(potionItem, 1))
            return;

        lastUseFrame = Time.frameCount;
        consumableManager.UseConsumable(potionItem, player);

        if (consumableInventory.GetAmount(potionItem) <= 0)
            potionItem = null;

        RefreshContent();
    }

    private void CloseSelection()
    {
        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        if (buttonScript != null)
            buttonScript.consumablePanelVisible = false;

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

    public void SelectItemFromDescription(ConsumableData item)
    {
        SelectItem(item);
    }

    public void ShowConsumableDescription(ConsumableData item)
    {
        if (ItemDescriptionPanel.Instance != null)
            ItemDescriptionPanel.Instance.ShowConsumable(item, this);
    }
}
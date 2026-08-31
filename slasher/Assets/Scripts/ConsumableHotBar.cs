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

    [Header("Filter Buttons")]
    [SerializeField] private Button allButton;
    [SerializeField] private Button foodButton;
    [SerializeField] private Button potionButton;

    [Header("Items")]
    [SerializeField] private List<ConsumableData> consumables = new();

    [Header("Player")]
    [SerializeField] private ConsumableManager consumableManager;

    private int selectedSlot = -1;

    [SerializeField] private ConsumableData foodItem;
    [SerializeField] private ConsumableData potionItem;

    private ButtonScript buttonScript;

    private void Start()
    {
        foodSlot.onClick.AddListener(UseFood);
        potionSlot.onClick.AddListener(UsePotion);

        allButton.onClick.AddListener(ShowAllItems);
        foodButton.onClick.AddListener(() => ShowItems(ConsumableType.Food));
        potionButton.onClick.AddListener(() => ShowItems(ConsumableType.Potion));

        selectionPanel.SetActive(false);

        buttonScript = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ButtonScript>();
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

        foreach (ConsumableData item in consumables)
        {
            if (item == null)
                continue;

            if (selectedSlot == 0 && item.consumableType != ConsumableType.Food)
                continue;

            if (selectedSlot == 1 && item.consumableType != ConsumableType.Potion)
                continue;

            CreateItemButton(item);
        }
    }

    private void ShowItems(ConsumableType type)
    {
        ClearList();

        foreach (ConsumableData item in consumables)
        {
            if (item == null)
                continue;

            if (item.consumableType != type)
                continue;

            if (selectedSlot == 0 && item.consumableType != ConsumableType.Food)
                continue;

            if (selectedSlot == 1 && item.consumableType != ConsumableType.Potion)
                continue;

            CreateItemButton(item);
        }
    }

    private void CreateItemButton(ConsumableData item)
    {
        GameObject button = Instantiate(itemButtonPrefab, itemListContent);

        ConsumableItemButton itemButton = button.GetComponent<ConsumableItemButton>();

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

        if (selectedSlot == 0 && item.consumableType != ConsumableType.Food)
            return;

        if (selectedSlot == 1 && item.consumableType != ConsumableType.Potion)
            return;

        if (selectedSlot == 0)
        {
            foodItem = item;
        }
        else if (selectedSlot == 1)
        {
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
        consumableManager.UseConsumable(foodItem);
        
    }

    public void UsePotion()
    {
        if (potionItem == null)
            return;

        if (consumableManager == null)
            return;

        consumableManager.UseConsumable(potionItem);
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
}
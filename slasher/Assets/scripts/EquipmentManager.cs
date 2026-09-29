using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private GameObject itemSelectionPanel;
    [SerializeField] private Transform itemListContent;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private EquipmentSlot[] equipmentSlots;

    [SerializeField] private List<ItemData> inventory = new();

    private EquipmentSlot selectedSlot;
    private Player currentPlayer;

    private void Awake()
    {
        FindUI();
    }

    private void Start()
    {
        UpdateCurrentPlayer();
        RefreshEquipmentUI();
    }

    private void Update()
    {
        Player player = TeamManager.Instance != null ? TeamManager.Instance.CurrentPlayer : null;

        if (player != currentPlayer)
        {
            UpdateCurrentPlayer();
            RefreshEquipmentUI();
        }
    }

    private void UpdateCurrentPlayer()
    {
        currentPlayer = TeamManager.Instance != null ? TeamManager.Instance.CurrentPlayer : null;
    }

    private void FindUI()
    {
        if (itemSelectionPanel == null)
        {
            GameObject panel = GameObject.Find("ItemSelectionPanel");

            if (panel != null)
                itemSelectionPanel = panel;
        }

        if (itemListContent == null && itemSelectionPanel != null)
        {
            Transform content = itemSelectionPanel.transform.Find("ItemListContent");

            if (content != null)
                itemListContent = content;
        }
    }

    public void OpenItemList(EquipmentSlot slot)
    {
        if (itemSelectionPanel == null)
            FindUI();

        if (itemSelectionPanel == null || itemListContent == null)
            return;

        UpdateCurrentPlayer();

        if (currentPlayer == null)
            return;

        selectedSlot = slot;
        itemSelectionPanel.SetActive(true);

        RefreshItemList();
    }

    public void RefreshItemList()
    {
        if (itemListContent == null)
            return;

        foreach (Transform child in itemListContent)
            Destroy(child.gameObject);

        if (selectedSlot == null)
            return;

        foreach (ItemData item in inventory)
        {
            if (item == null)
                continue;

            if (!selectedSlot.CanEquip(item))
                continue;

            GameObject button = Instantiate(itemButtonPrefab, itemListContent);

            ItemButton itemButton = button.GetComponent<ItemButton>();

            if (itemButton != null)
                itemButton.Setup(item);
        }
    }

    public void CloseItemList()
    {
        if (itemSelectionPanel == null)
            FindUI();

        if (itemSelectionPanel != null)
            itemSelectionPanel.SetActive(false);

        selectedSlot = null;
    }

    public void SelectItem(ItemData item)
    {
        if (selectedSlot == null)
            return;

        if (!selectedSlot.CanEquip(item))
            return;

        selectedSlot.Equip(item);

        if (itemSelectionPanel != null)
            itemSelectionPanel.SetActive(false);

        selectedSlot = null;
    }

    public ItemData GetEquippedItem(EquipmentType type)
    {
        UpdateCurrentPlayer();

        if (currentPlayer == null || currentPlayer.GetCharacterData() == null)
            return null;

        foreach (ItemData item in currentPlayer.GetCharacterData().equippedItems)
        {
            if (item != null && item.equipmentType == type)
                return item;
        }

        return null;
    }

    public void EquipItem(EquipmentSlot slot, ItemData item)
    {
        UpdateCurrentPlayer();

        if (currentPlayer == null || currentPlayer.GetCharacterData() == null)
            return;

        if (slot == null || item == null)
            return;

        if (!slot.CanEquip(item))
            return;

        CharacterData character = currentPlayer.GetCharacterData();

        ItemData oldItem = GetEquippedItem(slot.Type);

        if (oldItem != null)
            inventory.Add(oldItem);

        character.equippedItems.RemoveAll(x => x != null && x.equipmentType == slot.Type);
        character.equippedItems.Add(item);

        inventory.Remove(item);

        currentPlayer.GetStats().UpdateStats();

        RefreshEquipmentUI();
        RefreshItemList();
    }

    public void RefreshEquipmentUI()
    {
        UpdateCurrentPlayer();

        if (currentPlayer == null)
            return;

        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot != null)
                slot.Refresh();
        }
    }

    public void AddToInventory(ItemData item)
    {
        if (item == null)
            return;

        inventory.Add(item);
    }

    public void RemoveFromInventory(ItemData item)
    {
        if (item == null)
            return;

        inventory.Remove(item);
    }
}
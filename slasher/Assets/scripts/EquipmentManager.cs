using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private GameObject itemSelectionPanel;
    [SerializeField] private Transform itemListContent;
    [SerializeField] private GameObject itemButtonPrefab;

    [SerializeField] private List<ItemData> inventory = new();

    private EquipmentSlot selectedSlot;

    private void Awake()
    {
        FindUI();
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

        selectedSlot = slot;
        itemSelectionPanel.SetActive(true);

        RefreshItemList();
    }

    public void RefreshItemList()
    {
        if (itemListContent == null)
            return;

        foreach (Transform child in itemListContent)
        {
            Destroy(child.gameObject);
        }

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
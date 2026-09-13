using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [SerializeField] private GameObject itemSelectionPanel;
    [SerializeField] private Transform itemListContent;
    [SerializeField] private GameObject itemButtonPrefab;

    [SerializeField] private List<ItemData> inventory = new();

    private EquipmentSlot selectedSlot;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenItemList(EquipmentSlot slot)
    {
        selectedSlot = slot;
        itemSelectionPanel.SetActive(true);

        RefreshItemList();
    }

    public void RefreshItemList()
    {
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
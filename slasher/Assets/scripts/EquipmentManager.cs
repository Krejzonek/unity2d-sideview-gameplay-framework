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
        foreach (Transform child in itemListContent)
        {
            Destroy(child.gameObject);
        }
        foreach (ItemData item in inventory)
        {
            if (item.equipmentType != slot.Type)
                continue;

            GameObject button =
                Instantiate(itemButtonPrefab, itemListContent);

            ItemButton itemButton =
                button.GetComponent<ItemButton>();

            itemButton.Setup(item);
        }
    }
    public void CloseItemList()
    {
        itemSelectionPanel.SetActive(false);
    }
    public void SelectItem(ItemData item)
    {
        if (selectedSlot == null)
            return;
        selectedSlot.Equip(item);
        itemSelectionPanel.SetActive(false);
        selectedSlot = null;
    }
    public void AddToInventory(ItemData item)
    {
        if (!inventory.Contains(item))
        {
            inventory.Add(item);
        }
    }
    public void RemoveFromInventory(ItemData item)
    {
        inventory.Remove(item);
    }
}
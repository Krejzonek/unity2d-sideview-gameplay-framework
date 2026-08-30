using UnityEngine;
using UnityEngine.UI;
public enum EquipmentType
{
    Weapon,
    Helmet,
    Armor,
    Boots,
    SecondHand
}

public class EquipmentSlot : MonoBehaviour
{
    [SerializeField] private EquipmentType equipmentType;
    [SerializeField] private Image icon;

    private ItemData equippedItem;

    public EquipmentType Type => equipmentType;

    public ItemData EquippedItem => equippedItem;

    public void OpenItemList()
    {
        EquipmentManager.Instance.OpenItemList(this);
    }
    public void Equip(ItemData item)
    {
        if (equippedItem != null)
        {
            EquipmentManager.Instance.AddToInventory(equippedItem);
        }

        equippedItem = item;

        EquipmentManager.Instance.RemoveFromInventory(item);

        Stats stats = FindFirstObjectByType<Stats>();

        if (stats != null)
            stats.UpdateStats();

        //icon.sprite = item.icon;
        //icon.enabled = true;
    }
}
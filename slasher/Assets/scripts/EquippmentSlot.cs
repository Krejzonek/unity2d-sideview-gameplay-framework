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
    [SerializeField] private Button button;

    private ItemData equippedItem;

    public EquipmentType Type => equipmentType;
    public ItemData EquippedItem => equippedItem;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    public void OpenItemList()
    {
        EquipmentManager.Instance.OpenItemList(this);
    }

    public bool CanEquip(ItemData item)
    {
        if (item == null) return false;
        if (item.equipmentType != equipmentType) return false;
        if (equipmentType != EquipmentType.Weapon) return true;
        Stats stats = FindFirstObjectByType<Stats>();
        if (stats == null || stats.characterClass == null) return false;
        switch (item.weaponType)
        {
            case WeaponType.Sword:
                return stats.characterClass.name == "Warrior";

            case WeaponType.Bow:
                return stats.characterClass.name == "Archer";

            case WeaponType.Staff:
                return stats.characterClass.name == "Mage";

            default:
                return false;
        }
    }

    public void Equip(ItemData item)
    {
        if (!CanEquip(item)) return;
        if (equippedItem != null) EquipmentManager.Instance.AddToInventory(equippedItem);
        equippedItem = item;
        EquipmentManager.Instance.RemoveFromInventory(item);
        if (button != null) button.image.color = RarityColor.GetColor(item.GetRarity());
        if (icon != null) icon.sprite = item.icon;
        Stats stats = FindFirstObjectByType<Stats>();
        if (stats != null) stats.UpdateStats();
        EquipmentManager.Instance.RefreshItemList();
    }
    
}
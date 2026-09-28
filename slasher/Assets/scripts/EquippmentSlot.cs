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
    private EquipmentManager equipmentManager;
    private Stats stats;

    public EquipmentType Type => equipmentType;
    public ItemData EquippedItem => equippedItem;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();

        equipmentManager = GetComponentInParent<EquipmentManager>();
        stats = GetComponentInParent<Stats>();
    }

    public void OpenItemList()
    {
        if (equipmentManager == null)
            return;

        equipmentManager.OpenItemList(this);
    }

    public bool CanEquip(ItemData item)
    {
        if (item == null) return false;
        if (item.equipmentType != equipmentType) return false;
        if (equipmentType != EquipmentType.Weapon) return true;

        if (stats == null || stats.characterClass == null)
            return false;

        switch (item.weaponType)
        {
            case WeaponType.Sword:
                return stats.characterClass.className == "Warrior";

            case WeaponType.Bow:
                return stats.characterClass.className == "Archer";

            case WeaponType.Staff:
                return stats.characterClass.className == "Mage";

            default:
                return false;
        }
    }

    public void Equip(ItemData item)
    {
        if (!CanEquip(item))
            return;

        if (equippedItem != null)
            equipmentManager.AddToInventory(equippedItem);

        equippedItem = item;

        equipmentManager.RemoveFromInventory(item);

        if (button != null)
            button.image.color = RarityColor.GetColor(item.GetRarity());

        if (icon != null)
            icon.sprite = item.icon;

        if (stats != null)
            stats.UpdateStats();

        equipmentManager.RefreshItemList();
    }
}
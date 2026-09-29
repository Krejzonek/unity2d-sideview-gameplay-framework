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

    private EquipmentManager equipmentManager;

    public EquipmentType Type => equipmentType;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        equipmentManager = FindFirstObjectByType<EquipmentManager>();

        if (button != null)
            button.onClick.AddListener(OpenItemList);
    }

    public void OpenItemList()
    {
        if (equipmentManager == null)
        {
            equipmentManager = FindFirstObjectByType<EquipmentManager>();

            if (equipmentManager == null)
                return;
        }

        equipmentManager.OpenItemList(this);
    }

    public bool CanEquip(ItemData item)
    {
        if (item == null)
            return false;

        if (item.equipmentType != equipmentType)
            return false;

        if (equipmentType != EquipmentType.Weapon)
            return true;

        Player player = TeamManager.Instance != null ? TeamManager.Instance.CurrentPlayer : null;

        if (player == null)
            return false;

        Stats stats = player.GetStats();

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

        if (equipmentManager != null)
            equipmentManager.EquipItem(this, item);
    }

    public void Refresh()
    {
        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>();

        if (equipmentManager == null)
            return;

        ItemData item = equipmentManager.GetEquippedItem(equipmentType);

        if (icon != null)
        {
            icon.sprite = item != null ? item.icon : null;
            icon.enabled = item != null;
        }

        if (button != null)
            button.image.color = item != null ? RarityColor.GetColor(item.GetRarity()) : Color.white;
    }
}
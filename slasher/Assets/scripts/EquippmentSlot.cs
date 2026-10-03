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
    private UiEq uiEq;
    private CharacterData character;

    public EquipmentType Type => equipmentType;
    public CharacterData Character => character;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
            button = GetComponentInChildren<Button>(true);

        uiEq = GetComponentInParent<UiEq>();

        equipmentManager = FindFirstObjectByType<EquipmentManager>();

        if (button != null)
        {
            button.onClick.RemoveListener(OpenItemList);
            button.onClick.AddListener(OpenItemList);
        }
    }

    public void Setup(CharacterData characterData)
    {
        character = characterData;
        Refresh();
    }

    public void OpenItemList()
    {
        if (uiEq == null)
            uiEq = GetComponentInParent<UiEq>();

        if (uiEq == null)
            return;

        uiEq.OpenItemList(this);
    }

    public bool CanEquip(ItemData item)
    {
        if (item == null)
            return false;

        if (item.equipmentType != equipmentType)
            return false;

        if (equipmentType != EquipmentType.Weapon)
            return true;

        if (character == null || character.characterClass == null)
            return false;

        switch (item.weaponType)
        {
            case WeaponType.Sword:
                return character.characterClass.className == "Warrior";

            case WeaponType.Bow:
                return character.characterClass.className == "Archer";

            case WeaponType.Staff:
                return character.characterClass.className == "Mage";

            default:
                return false;
        }
    }

    public void Equip(ItemData item)
    {
        if (!CanEquip(item))
            return;

        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>();

        if (equipmentManager != null)
            equipmentManager.EquipItem(this, item);
    }

    public void Refresh()
    {
        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>();

        if (equipmentManager == null || character == null)
            return;

        ItemData item = equipmentManager.GetEquippedItemForCharacter(character, equipmentType);

        if (icon != null)
        {
            icon.sprite = item != null ? item.icon : null;
            icon.enabled = item != null;
        }

        if (button != null)
            button.image.color = item != null ? RarityColor.GetColor(item.GetRarity()) : Color.white;
    }
}
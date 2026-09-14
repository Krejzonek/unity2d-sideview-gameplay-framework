using UnityEngine;

public enum WeaponType
{
    None,
    Sword,
    Bow,
    Staff
}

public enum MiscType
{
    None,
    CritRate,
    Knockback,
    Stun
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic,
    Godest
}

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    public EquipmentType equipmentType;
    public WeaponType weaponType;

    [Header("Rarity")]
    [SerializeField] private ItemRarity rarity;

    [Header("Stat Bonuses")]
    public float healthBonus;
    public float defenceBonus;
    public float staminaBonus;
    public float speedBonus;
    public float attackBonus;
    public float expBonus;

    [Header("Misc Bonus")]
    public MiscType miscType;

    public ItemRarity GetRarity()
    {
        return rarity;
    }

    public void SetRarity(ItemRarity value)
    {
        rarity = value;
    }

    public float GetMiscValue()
    {
        switch (miscType)
        {
            case MiscType.CritRate:
                return GetRarityValue(5f, 10f, 15f, 25f, 40f, 50f, 60f);

            case MiscType.Knockback:
                return GetRarityValue(10f, 20f, 30f, 45f, 65f, 80f, 100f);

            case MiscType.Stun:
                return GetRarityValue(2f, 5f, 8f, 12f, 18f, 25f, 35f);

            default:
                return 0f;
        }
    }

    private float GetRarityValue(float common, float uncommon, float rare, float epic, float legendary, float mythic, float godest)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return common;

            case ItemRarity.Uncommon:
                return uncommon;

            case ItemRarity.Rare:
                return rare;

            case ItemRarity.Epic:
                return epic;

            case ItemRarity.Legendary:
                return legendary;

            case ItemRarity.Mythic:
                return mythic;

            case ItemRarity.Godest:
                return godest;

            default:
                return 0f;
        }
    }
    public void SetMiscType(MiscType value)
    {
        miscType = value;
    }
}
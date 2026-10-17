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

[CreateAssetMenu(fileName = "New Equipment", menuName = "Equipment/Equipment")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    public EquipmentType equipmentType;
    public WeaponType weaponType;
    public ElementType element;

    [Header("Weapon Visual")]
    public WeaponVisualProfile weaponVisualProfile;

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

    [SerializeField, HideInInspector] private float baseHealthBonus;
    [SerializeField, HideInInspector] private float baseDefenceBonus;
    [SerializeField, HideInInspector] private float baseStaminaBonus;
    [SerializeField, HideInInspector] private float baseSpeedBonus;
    [SerializeField, HideInInspector] private float baseAttackBonus;
    [SerializeField, HideInInspector] private float baseExpBonus;
    [SerializeField, HideInInspector] private ItemRarity appliedRarity;
    [SerializeField, HideInInspector] private bool bonusesInitialized;

    public ItemRarity GetRarity()
    {
        return rarity;
    }

    public void SetRarity(ItemRarity value)
    {
        InitializeBaseBonuses();
        rarity = value;
        ApplyRarityToBonuses();
    }

    public float GetRarityBonus()
    {
        switch (rarity)
        {
            case ItemRarity.Common: return 0f;
            case ItemRarity.Uncommon: return 0.02f;
            case ItemRarity.Rare: return 0.04f;
            case ItemRarity.Epic: return 0.06f;
            case ItemRarity.Legendary: return 0.08f;
            case ItemRarity.Mythic: return 0.10f;
            case ItemRarity.Godest: return 0.12f;
            default: return 0f;
        }
    }

    private float ApplyRarityBonus(float value)
    {
        return value == 0f ? 0f : Mathf.Ceil(value * (1f + GetRarityBonus()));
    }

    private void InitializeBaseBonuses()
    {
        if (bonusesInitialized)
            return;

        baseHealthBonus = healthBonus;
        baseDefenceBonus = defenceBonus;
        baseStaminaBonus = staminaBonus;
        baseSpeedBonus = speedBonus;
        baseAttackBonus = attackBonus;
        baseExpBonus = expBonus;
        appliedRarity = rarity;
        bonusesInitialized = true;
    }

    private void ApplyRarityToBonuses()
    {
        healthBonus = ApplyRarityBonus(baseHealthBonus);
        defenceBonus = ApplyRarityBonus(baseDefenceBonus);
        staminaBonus = ApplyRarityBonus(baseStaminaBonus);
        speedBonus = ApplyRarityBonus(baseSpeedBonus);
        attackBonus = ApplyRarityBonus(baseAttackBonus);
        expBonus = ApplyRarityBonus(baseExpBonus);
        appliedRarity = rarity;
    }

    private void OnValidate()
    {
        if (!bonusesInitialized)
        {
            InitializeBaseBonuses();
            ApplyRarityToBonuses();
            return;
        }

        if (appliedRarity != rarity)
        {
            ApplyRarityToBonuses();
            return;
        }

        if (healthBonus != ApplyRarityBonus(baseHealthBonus))
            baseHealthBonus = healthBonus / (1f + GetRarityBonus());

        if (defenceBonus != ApplyRarityBonus(baseDefenceBonus))
            baseDefenceBonus = defenceBonus / (1f + GetRarityBonus());

        if (staminaBonus != ApplyRarityBonus(baseStaminaBonus))
            baseStaminaBonus = staminaBonus / (1f + GetRarityBonus());

        if (speedBonus != ApplyRarityBonus(baseSpeedBonus))
            baseSpeedBonus = speedBonus / (1f + GetRarityBonus());

        if (attackBonus != ApplyRarityBonus(baseAttackBonus))
            baseAttackBonus = attackBonus / (1f + GetRarityBonus());

        if (expBonus != ApplyRarityBonus(baseExpBonus))
            baseExpBonus = expBonus / (1f + GetRarityBonus());

        ApplyRarityToBonuses();
    }

    public StatusEffectType GetStatusEffect()
    {
        return StatusEffectSystem.GetStatusEffect(element);
    }

    public float GetStatusChance()
    {
        return StatusEffectSystem.GetStatusChance(rarity);
    }

    public float GetStatusDuration()
    {
        return StatusEffectSystem.GetStatusDuration(rarity);
    }

    public float GetStatusDamage(float attack)
    {
        return StatusEffectSystem.GetStatusDamage(attack);
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
            case ItemRarity.Common: return common;
            case ItemRarity.Uncommon: return uncommon;
            case ItemRarity.Rare: return rare;
            case ItemRarity.Epic: return epic;
            case ItemRarity.Legendary: return legendary;
            case ItemRarity.Mythic: return mythic;
            case ItemRarity.Godest: return godest;
            default: return 0f;
        }
    }

    public void SetMiscType(MiscType value)
    {
        miscType = value;
    }
}
public static class StatusEffectSystem
{
    public static StatusEffectType GetStatusEffect(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire:
                return StatusEffectType.Burn;

            case ElementType.Water:
                return StatusEffectType.Drown;

            case ElementType.Earth:
                return StatusEffectType.Root;

            case ElementType.Wind:
                return StatusEffectType.Bleed;

            case ElementType.Lightning:
                return StatusEffectType.Shock;

            case ElementType.Ice:
                return StatusEffectType.Freeze;

            case ElementType.Light:
                return StatusEffectType.Illusion;

            case ElementType.Dark:
                return StatusEffectType.Blind;

            case ElementType.Poison:
                return StatusEffectType.Poison;

            default:
                return StatusEffectType.None;
        }
    }

    public static float GetStatusChance(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return 5f;

            case ItemRarity.Uncommon:
                return 10f;

            case ItemRarity.Rare:
                return 15f;

            case ItemRarity.Epic:
                return 20f;

            case ItemRarity.Legendary:
                return 30f;

            case ItemRarity.Mythic:
                return 40f;

            case ItemRarity.Godest:
                return 50f;

            default:
                return 0f;
        }
    }

    public static float GetStatusDuration(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return 2f;

            case ItemRarity.Uncommon:
                return 2.5f;

            case ItemRarity.Rare:
                return 3f;

            case ItemRarity.Epic:
                return 3.5f;

            case ItemRarity.Legendary:
                return 4f;

            case ItemRarity.Mythic:
                return 5f;

            case ItemRarity.Godest:
                return 6f;

            default:
                return 0f;
        }
    }

    public static float GetStatusDamage(float attack)
    {
        return attack * 0.1f;
    }
}
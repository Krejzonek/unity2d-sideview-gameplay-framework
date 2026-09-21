using UnityEngine;

public static class RarityColor
{
    public static Color GetColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return Color.gray;

            case ItemRarity.Uncommon:
                return Color.green;

            case ItemRarity.Rare:
                return Color.blue;

            case ItemRarity.Epic:
                return new Color(0.6f, 0.2f, 1f);

            case ItemRarity.Legendary:
                return new Color(1f, 0.6f, 0f);

            case ItemRarity.Mythic:
                return Color.red;

            case ItemRarity.Godest:
                return new Color(1f, 0.2f, 0.8f);

            default:
                return Color.white;
        }
    }
}
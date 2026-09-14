using UnityEngine;

public static class RarityManager
{
    public static ItemRarity GetRandomRarity()
    {
        float roll = Random.Range(0f, 100f);

        if (roll < 50f)
            return ItemRarity.Common;

        if (roll < 75f)
            return ItemRarity.Uncommon;

        if (roll < 90f)
            return ItemRarity.Rare;

        if (roll < 97f)
            return ItemRarity.Epic;

        if (roll < 99.5f)
            return ItemRarity.Legendary;

        if (roll < 99.9f)
            return ItemRarity.Mythic;

        return ItemRarity.Godest;
    }

    public static bool RollMisc()
    {
        return Random.Range(0f, 100f) < 10f;
    }

    public static MiscType GetRandomMisc()
    {
        int roll = Random.Range(0, 3);

        switch (roll)
        {
            case 0:
                return MiscType.CritRate;

            case 1:
                return MiscType.Knockback;

            case 2:
                return MiscType.Stun;

            default:
                return MiscType.None;
        }
    }
}
using UnityEngine;

public static class LuckSystem
{
    public static float GetMultiplier(float luck)
    {
        luck = Mathf.Max(1f, luck);
        return 1f + (luck - 1f) / 49f * 0.5f;
    }

    public static int GetAmount(int baseAmount, float luck)
    {
        if (baseAmount <= 0)
            return 0;

        float amount = baseAmount * GetMultiplier(luck);
        int result = Mathf.FloorToInt(amount);

        if (Random.value < amount - result)
            result++;

        return Mathf.Max(result, baseAmount);
    }

    public static ItemRarity GetItemRarity(float luck)
    {
        luck = Mathf.Max(1f, luck);

        float legendaryChance = GetLegendaryChance(luck);
        float mythicChance = GetMythicChance(luck);
        float godestChance = GetGodestChance(luck);

        float roll = Random.Range(0f, 100f);

        if (roll < godestChance)
            return ItemRarity.Godest;

        roll -= godestChance;

        if (roll < mythicChance)
            return ItemRarity.Mythic;

        roll -= mythicChance;

        if (roll < legendaryChance)
            return ItemRarity.Legendary;

        roll -= legendaryChance;

        float epicChance = GetEpicChance(luck);

        if (roll < epicChance)
            return ItemRarity.Epic;

        roll -= epicChance;

        float rareChance = GetRareChance(luck);

        if (roll < rareChance)
            return ItemRarity.Rare;

        roll -= rareChance;

        float uncommonChance = GetUncommonChance(luck);

        if (roll < uncommonChance)
            return ItemRarity.Uncommon;

        return ItemRarity.Common;
    }

    public static ConsumableRarity GetConsumableRarity(float luck)
    {
        luck = Mathf.Max(1f, luck);

        float legendaryChance = GetLegendaryChance(luck);
        float mythicChance = GetMythicChance(luck);

        float roll = Random.Range(0f, 100f);

        if (roll < mythicChance)
            return ConsumableRarity.Mythic;

        roll -= mythicChance;

        if (roll < legendaryChance)
            return ConsumableRarity.Legendary;

        roll -= legendaryChance;

        float epicChance = GetEpicChance(luck);

        if (roll < epicChance)
            return ConsumableRarity.Epic;

        roll -= epicChance;

        float rareChance = GetRareChance(luck);

        if (roll < rareChance)
            return ConsumableRarity.Rare;

        roll -= rareChance;

        float uncommonChance = GetUncommonChance(luck);

        if (roll < uncommonChance)
            return ConsumableRarity.Uncommon;

        return ConsumableRarity.Common;
    }

    private static float GetLegendaryChance(float luck)
    {
        if (luck <= 1f)
            return 1f;

        return Mathf.Min(90f, 1f + 24f * Mathf.Pow((luck - 1f) / 4f, 0.8f));
    }

    private static float GetMythicChance(float luck)
    {
        if (luck <= 1f)
            return 0.1f;

        return Mathf.Min(25f, 0.1f + 0.015f * Mathf.Pow(luck - 1f, 1.35f));
    }

    private static float GetGodestChance(float luck)
    {
        if (luck < 20f)
            return 0f;

        return Mathf.Min(10f, (luck - 20f) * 0.02f);
    }

    private static float GetEpicChance(float luck)
    {
        return Mathf.Max(2f, 10f - luck * 0.1f);
    }

    private static float GetRareChance(float luck)
    {
        return Mathf.Max(5f, 20f - luck * 0.15f);
    }

    private static float GetUncommonChance(float luck)
    {
        return Mathf.Max(5f, 30f - luck * 0.2f);
    }

    public static float GetCookingPositiveMultiplier(float luck)
    {
        return GetMultiplier(luck);
    }

    public static float GetCookingNegativeMultiplier(float luck)
    {
        return 1f / GetMultiplier(luck);
    }
}
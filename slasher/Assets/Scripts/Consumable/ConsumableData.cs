using System.Collections.Generic;
using UnityEngine;

public enum ConsumableType
{
    Potion,
    Food
}

public enum ConsumableRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

[System.Serializable]
public class StatEffect
{
    public StatType statType;
    public float amount;
}

[CreateAssetMenu(fileName = "New Consumable", menuName = "Items/Consumable")]
public class ConsumableData : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    [Header("Type")]
    public ConsumableType consumableType;

    [Header("Rarity")]
    public ConsumableRarity rarity;

    [Header("Potion")]
    public float healAmount;
    public float buffDuration;
    public List<StatEffect> effects = new();

    [Header("Food")]
    public float regenerationAmount;
    public float regenerationDuration;

    [Header("Description")]
    [TextArea(3, 6)]
    public string description;

    public float GetDebuffReduction()
    {
        switch (rarity)
        {
            case ConsumableRarity.Common:
                return 0f;

            case ConsumableRarity.Uncommon:
                return 0.1f;

            case ConsumableRarity.Rare:
                return 0.2f;

            case ConsumableRarity.Epic:
                return 0.3f;

            case ConsumableRarity.Legendary:
                return 0.5f;

            case ConsumableRarity.Mythic:
                return 1f;
        }

        return 0f;
    }
}
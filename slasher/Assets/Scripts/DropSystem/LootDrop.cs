using UnityEngine;

public enum LootType
{
    Ingredient,
    Consumable,
    Equipment
}

[System.Serializable]
public class LootDrop
{
    public LootType lootType;

    [Header("Ingredient")]
    public IngredientData ingredient;

    [Header("Consumable")]
    public ConsumableData consumable;

    [Header("Equipment")]
    public ItemData equipment;

    [Header("Drop")]
    [Range(0f, 100f)]
    public float dropChance = 100f;

    public int minAmount = 1;
    public int maxAmount = 1;
}
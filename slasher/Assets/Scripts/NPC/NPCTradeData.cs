using UnityEngine;

public enum NPCTradeItemType
{
    Ingredient,
    Consumable,
    Equipment
}

[CreateAssetMenu(fileName = "New NPC Trade", menuName = "NPC/Trade")]
public class NPCTradeData : ScriptableObject
{
    [Header("Required Item")]
    public NPCTradeItemType requiredItemType;
    public IngredientData requiredIngredient;
    public ConsumableData requiredConsumable;
    public ItemData requiredEquipment;
    [Min(1)] public int requiredAmount = 1;

    [Header("Reward Item")]
    public NPCTradeItemType rewardItemType;
    public IngredientData rewardIngredient;
    public ConsumableData rewardConsumable;
    public ItemData rewardEquipment;
    [Min(1)] public int rewardAmount = 1;
}
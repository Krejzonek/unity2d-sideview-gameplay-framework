using System.Collections.Generic;
using UnityEngine;

public enum ConsumableType
{
    Potion,
    Food
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

    [Header("Potion")]
    public float healAmount;
    public float buffDuration;
    public List<StatEffect> effects = new();

    [Header("Food")]
    public float regenerationAmount;
    public float regenerationDuration;
}
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    public EquipmentType equipmentType;

    [Header("Stat Bonuses")]
    public float healthBonus;
    public float defenceBonus;
    public float staminaBonus;
    public float speedBonus;
    public float attackBonus;
    public float expBonus;
}
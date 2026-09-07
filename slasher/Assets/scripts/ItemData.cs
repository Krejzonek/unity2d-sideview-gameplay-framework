using UnityEngine;

public enum WeaponType
{
    None,
    Sword,
    Bow,
    Staff
}

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    public EquipmentType equipmentType;
    public WeaponType weaponType;

    [Header("Stat Bonuses")]
    public float healthBonus;
    public float defenceBonus;
    public float staminaBonus;
    public float speedBonus;
    public float attackBonus;
    public float expBonus;
}
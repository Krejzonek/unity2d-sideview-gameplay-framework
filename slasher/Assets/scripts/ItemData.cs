using UnityEngine;

public enum WeaponType
{
    None,
    Sword,
    Bow,
    Staff
}

public enum MiscType
{
    None,
    CritRate,
    Knockback,
    Stun
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

    [Header("Misc Bonus")]
    public MiscType miscType;
    public float miscValue;
}
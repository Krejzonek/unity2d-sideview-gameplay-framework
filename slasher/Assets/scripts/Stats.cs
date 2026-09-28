using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public enum CharacterClass
{
    None,
    Warrior,
    Archer,
    Mage
}

public enum StatType
{
    Health,
    Stamina,
    Speed,
    Attack,
    Defence,
    Luck,
    ExpBonus
}

public class Stats : MonoBehaviour
{
    [Header("Class")]
    public Class characterClass;

    [Header("Base Stats")]
    [SerializeField] private float baseHealth = 100f;
    [SerializeField] private float baseDefence = 0f;
    [SerializeField] private float baseStamina = 100f;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float baseAttack = 10f;
    [SerializeField] private float baseLuck = 1f;
    [SerializeField] private float baseCritRate = 5f;
    [SerializeField] private float baseCritDamage = 1.5f;
    [SerializeField] private float baseKnockback = 0f;

    [Header("Points")]
    public int freePoint;

    private float healthPoint;
    private float staminaPoint;
    private float speedPoint;
    private float attackPoint;
    private float luckPoint;

    [Header("Final Stats")]
    public float health;
    public float defence;
    public float stamina;
    public float speed;
    public float attack;
    public float luck;
    public float expBonus;
    public float critRate;
    public float critDamage;
    public float knockback;
    public float stunChance;

    [Header("Equipment")]
    [SerializeField] private ItemData[] equippedItems;

    [SerializeField] private EquipmentSlot[] equipmentSlots;
    private List<StatEffect> activeEffects = new();
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateStats();
        UpdateClassColor();
    }

    public void UpdateStats()
    {
        health = baseHealth + healthPoint;
        defence = baseDefence;
        stamina = baseStamina + staminaPoint;
        speed = baseSpeed + speedPoint;
        attack = baseAttack + attackPoint;
        luck = baseLuck + luckPoint;

        critRate = baseCritRate;
        critDamage = baseCritDamage;
        knockback = baseKnockback;
        stunChance = 0f;

        expBonus = 1f;

        if (characterClass != null)
        {
            health += characterClass.healthBonus;
            stamina += characterClass.staminaBonus;
            speed += characterClass.speedBonus;
            attack += characterClass.attackBonus;
        }

        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot == null)
                continue;

            ItemData item = slot.EquippedItem;

            if (item == null)
                continue;

            health += item.healthBonus;
            defence += item.defenceBonus;
            stamina += item.staminaBonus;
            speed += item.speedBonus;
            attack += item.attackBonus;
            expBonus += item.expBonus;

            switch (item.miscType)
            {
                case MiscType.CritRate:
                    critRate += item.GetMiscValue();
                    break;

                case MiscType.Knockback:
                    knockback += item.GetMiscValue();
                    break;

                case MiscType.Stun:
                    stunChance += item.GetMiscValue();
                    break;
            }
        }

        foreach (StatEffect effect in activeEffects)
        {
            switch (effect.statType)
            {
                case StatType.Health:
                    health += effect.amount;
                    break;

                case StatType.Stamina:
                    stamina += effect.amount;
                    break;

                case StatType.Speed:
                    speed += effect.amount;
                    break;

                case StatType.Attack:
                    attack += effect.amount;
                    break;

                case StatType.Defence:
                    defence += effect.amount;
                    break;

                case StatType.Luck:
                    luck += effect.amount;
                    break;

                case StatType.ExpBonus:
                    expBonus += effect.amount;
                    break;
            }
        }

        critRate = Mathf.Clamp(critRate, 0f, 100f);
        stunChance = Mathf.Clamp(stunChance, 0f, 100f);

        UpdateClassColor();
    }

    public void HealthAdd()
    {
        if (freePoint <= 0)
            return;

        healthPoint += 50;
        freePoint--;

        UpdateStats();
    }

    public void StaminaAdd()
    {
        if (freePoint <= 0)
            return;

        staminaPoint += 25;
        freePoint--;

        UpdateStats();
    }

    public void SpeedAdd()
    {
        if (freePoint <= 0)
            return;

        speedPoint += 1;
        freePoint--;

        UpdateStats();
    }

    public void AttackAdd()
    {
        if (freePoint <= 0)
            return;

        attackPoint += 10;
        freePoint--;

        UpdateStats();
    }

    public void LuckAdd()
    {
        if (freePoint <= 0)
            return;

        luckPoint += 1;
        freePoint--;

        UpdateStats();
    }

    public ItemData GetWeapon()
    {
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot == null)
                continue;

            if (slot.Type != EquipmentType.Weapon)
                continue;

            return slot.EquippedItem;
        }

        return null;
    }

    public void SetClass(Class newClass)
    {
        characterClass = newClass;

        if (CharacterManager.Instance != null)
            CharacterManager.Instance.SetCharacterClass(newClass);

        
        UpdateStats();
    }

    public void AddBuff(List<StatEffect> effects, float duration)
    {
        StartCoroutine(BuffCoroutine(effects, duration));
    }

    private IEnumerator BuffCoroutine(List<StatEffect> effects, float duration)
    {
        foreach (StatEffect effect in effects)
        {
            activeEffects.Add(effect);
        }

        UpdateStats();

        yield return new WaitForSeconds(duration);

        foreach (StatEffect effect in effects)
        {
            activeEffects.Remove(effect);
        }

        UpdateStats();
    }
    public void GetStatPoints(out float health, out float stamina, out float speed, out float attack, out float luck)
    {
        health = healthPoint;
        stamina = staminaPoint;
        speed = speedPoint;
        attack = attackPoint;
        luck = luckPoint;
    }
    private void UpdateClassColor()
    {
        if (spriteRenderer == null)
            return;

        if (characterClass == null)
        {
            spriteRenderer.color = Color.gray;
            return;
        }

        spriteRenderer.color = characterClass.classColor;
    }
    public void SetStatPoints(float health, float stamina, float speed, float attack, float luck)
    {
        healthPoint = health;
        staminaPoint = stamina;
        speedPoint = speed;
        attackPoint = attack;
        luckPoint = luck;

        UpdateStats();
    }
}
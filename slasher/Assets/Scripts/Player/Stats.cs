using System.Collections;
using System.Collections.Generic;
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

    private List<StatEffect> activeEffects = new();
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (transform.childCount > 0)
            spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

        LoadCharacterData();
        UpdateStats();
        UpdateClassColor();
    }

    private void LoadCharacterData()
    {
        Player player = GetComponent<Player>();

        if (player == null || player.GetCharacterData() == null)
            return;

        CharacterData character = player.GetCharacterData();

        characterClass = character.characterClass;
        freePoint = character.freePoint;
    }

    public void UpdateStats()
    {
        Player player = GetComponent<Player>();
        CharacterData character = player != null ? player.GetCharacterData() : null;

        float healthPoint = character != null ? character.healthPoint : 0f;
        float staminaPoint = character != null ? character.staminaPoint : 0f;
        float speedPoint = character != null ? character.speedPoint : 0f;
        float attackPoint = character != null ? character.attackPoint : 0f;
        float luckPoint = character != null ? character.luckPoint : 0f;

        if (character != null)
        {
            if (character.characterClass != null)
                characterClass = character.characterClass;
            else if (characterClass != null)
                character.characterClass = characterClass;

            freePoint = character.freePoint;
        }

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
            defence += characterClass.defenceBonus;
        }

        if (character != null && character.equippedItems != null)
        {
            foreach (ItemData item in character.equippedItems)
            {
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
        Player player = GetComponent<Player>();

        if (player == null || player.GetCharacterData() == null)
            return;

        CharacterData character = player.GetCharacterData();

        if (character.freePoint <= 0)
            return;

        character.healthPoint += 50;
        character.freePoint--;

        freePoint = character.freePoint;

        UpdateStats();
    }

    public void StaminaAdd()
    {
        Player player = GetComponent<Player>();

        if (player == null || player.GetCharacterData() == null)
            return;

        CharacterData character = player.GetCharacterData();

        if (character.freePoint <= 0)
            return;

        character.staminaPoint += 25;
        character.freePoint--;

        freePoint = character.freePoint;

        UpdateStats();
    }

    public void SpeedAdd()
    {
        Player player = GetComponent<Player>();

        if (player == null || player.GetCharacterData() == null)
            return;

        CharacterData character = player.GetCharacterData();

        if (character.freePoint <= 0)
            return;

        character.speedPoint += 1;
        character.freePoint--;

        freePoint = character.freePoint;

        UpdateStats();
    }

    public void AttackAdd()
    {
        Player player = GetComponent<Player>();

        if (player == null || player.GetCharacterData() == null)
            return;

        CharacterData character = player.GetCharacterData();

        if (character.freePoint <= 0)
            return;

        character.attackPoint += 10;
        character.freePoint--;

        freePoint = character.freePoint;

        UpdateStats();
    }

    public void LuckAdd()
    {
        Player player = GetComponent<Player>();

        if (player == null || player.GetCharacterData() == null)
            return;

        CharacterData character = player.GetCharacterData();

        if (character.freePoint <= 0)
            return;

        character.luckPoint += 1;
        character.freePoint--;

        freePoint = character.freePoint;

        UpdateStats();
    }

    public ItemData GetWeapon()
    {
        Player player = GetComponent<Player>();

        if (player == null || player.GetCharacterData() == null)
            return null;

        foreach (ItemData item in player.GetCharacterData().equippedItems)
        {
            if (item != null && item.equipmentType == EquipmentType.Weapon)
                return item;
        }

        return null;
    }

    public void SetClass(Class newClass)
    {
        characterClass = newClass;

        Player player = GetComponent<Player>();

        if (player != null && player.GetCharacterData() != null)
            player.GetCharacterData().characterClass = newClass;

        UpdateStats();
    }

    public void AddBuff(List<StatEffect> effects, float duration)
    {
        StartCoroutine(BuffCoroutine(effects, duration));
    }

    private IEnumerator BuffCoroutine(List<StatEffect> effects, float duration)
    {
        foreach (StatEffect effect in effects)
            activeEffects.Add(effect);

        UpdateStats();

        yield return new WaitForSeconds(duration);

        foreach (StatEffect effect in effects)
            activeEffects.Remove(effect);

        UpdateStats();
    }

    public void GetStatPoints(out float health, out float stamina, out float speed, out float attack, out float luck)
    {
        Player player = GetComponent<Player>();

        if (player == null || player.GetCharacterData() == null)
        {
            health = 0;
            stamina = 0;
            speed = 0;
            attack = 0;
            luck = 0;
            return;
        }

        CharacterData character = player.GetCharacterData();

        health = character.healthPoint;
        stamina = character.staminaPoint;
        speed = character.speedPoint;
        attack = character.attackPoint;
        luck = character.luckPoint;
    }

    public void SetStatPoints(float health, float stamina, float speed, float attack, float luck)
    {
        Player player = GetComponent<Player>();

        if (player == null || player.GetCharacterData() == null)
            return;

        CharacterData character = player.GetCharacterData();

        character.healthPoint = health;
        character.staminaPoint = stamina;
        character.speedPoint = speed;
        character.attackPoint = attack;
        character.luckPoint = luck;

        UpdateStats();
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
}
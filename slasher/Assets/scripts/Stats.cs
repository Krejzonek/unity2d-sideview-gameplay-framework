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

    [Header("First Door")]
    [SerializeField] private GameObject firstDoor;

    private void Awake()
    {
        UpdateStats();
    }

    public void UpdateStats()
    {
        health = baseHealth + healthPoint;
        defence = baseDefence;
        stamina = baseStamina + staminaPoint;
        speed = baseSpeed + speedPoint;
        attack = baseAttack + attackPoint;
        luck = baseLuck + luckPoint;

        expBonus = 1f;

        if (characterClass != null)
        {
            health += characterClass.healthBonus;
            stamina += characterClass.staminaBonus;
            speed += characterClass.speedBonus;
            attack += characterClass.attackBonus;
        }
        
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

    public void SetClass(Class newClass)
    {
        characterClass = newClass;
        UpdateStats();
    }
}
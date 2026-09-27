using System;

[Serializable]
public class CharacterData
{
    public string characterName;
    public Class characterClass;

    public int level;
    public int currentEXP;

    public int freePoint;

    public float healthPoint;
    public float staminaPoint;
    public float speedPoint;
    public float attackPoint;
    public float luckPoint;

    public CharacterData(string name)
    {
        characterName = name;
        characterClass = null;
        level = 1;
        currentEXP = 0;
        freePoint = 0;

        healthPoint = 0;
        staminaPoint = 0;
        speedPoint = 0;
        attackPoint = 0;
        luckPoint = 0;
    }
}
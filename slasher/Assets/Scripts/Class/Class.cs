using UnityEngine;

[CreateAssetMenu(fileName = "ClassBonus", menuName = "Game/Class Bonus")]
public class Class : ScriptableObject
{
    [Header("Class Info")]
    public string className;

    [TextArea(2, 4)]
    public string description;

    [Header("Class Bonus")]
    public float healthBonus;
    public float staminaBonus;
    public float attackBonus;
    public float defenceBonus;
    public float speedBonus;

    [Header("Passive Skills")]
    public PassiveSkill passiveSkill1;
    public PassiveSkill passiveSkill2;

    [Header("Active Skill")]
    public ActiveSkill activeSkill;
}
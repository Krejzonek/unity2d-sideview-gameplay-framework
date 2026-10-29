using UnityEngine;

public enum PassiveSkillType
{
    StrongHit,
    Tough,
    LongShot,
    Agility,
    ElementBoost,
    ManaSaving
}

[CreateAssetMenu(fileName = "PassiveSkill", menuName = "Class/Skills/Passive Skill")]
public class PassiveSkill : ClassSkill
{
    public PassiveSkillType type;

    [Header("Values")]
    public float value = 10f;
    public int attackCount = 5;
    public float duration = 1f;
}
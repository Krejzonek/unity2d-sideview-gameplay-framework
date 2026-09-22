using UnityEngine;

public enum ActiveSkillType
{
    HeavyStrike,
    DoubleShot,
    EmpoweredSpell
}

[CreateAssetMenu(fileName = "ActiveSkill", menuName = "Game/Skills/Active Skill")]
public class ActiveSkill : ClassSkill
{
    public ActiveSkillType type;

    [Header("Values")]
    public float value = 50f;
    public float cooldown = 10f;
    public float knockbackMultiplier = 1.5f;
}
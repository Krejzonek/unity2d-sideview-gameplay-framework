using System.Collections.Generic;
using UnityEngine;

public enum BossClass
{
    Warrior,
    Mage,
    Beast,
    Necromancer,
    Golem,
    Assassin
}

[CreateAssetMenu(fileName = "BossClass", menuName = "Boss/Boss Class")]
public class BossClassData : ScriptableObject
{
    public BossClass bossClass;

    [Header("Attacks")]
    public List<BossAttackData> attacks = new List<BossAttackData>();

    [Header("Phases")]
    [Range(0f, 1f)] public float secondPhaseHealth = 0.5f;
    [Range(0f, 1f)] public float enrageHealth = 0.2f;
    public float secondPhaseCooldownMultiplier = 0.8f;
    public float enrageCooldownMultiplier = 0.6f;
    public float enrageDamageMultiplier = 1.5f;
}
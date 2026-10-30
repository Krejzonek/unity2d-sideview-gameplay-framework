using UnityEngine;

public enum BossAttackType
{
    Charge,
    GroundSlam,
    Shockwave,
    Uppercut,
    Fireball,
    Meteor,
    Arrow,
    MagicBeam,
    Explosion,
    Pounce,
    TailSweep,
    Roar
}

public enum BossWarningShape
{
    Horizontal,
    Vertical,
    Circle
}

[CreateAssetMenu(fileName = "BossAttack", menuName = "Boss/Boss Attack")]
public class BossAttackData : ScriptableObject
{
    [Header("Attack")]
    public string attackName;
    public BossAttackType attackType;
    public float cooldown = 3f;
    public float windup = 0.8f;
    public float recovery = 0.6f;
    public float minDistance = 0f;
    public float maxDistance = 10f;
    public float damageMultiplier = 1f;
    public float knockbackForce = 10f;

    [Header("Warning")]
    public BossWarningShape warningShape = BossWarningShape.Horizontal;
    public float warningWidth = 2f;
    public float warningLength = 5f;
    public float warningRadius = 2f;
    public float warningHeight = 5f;
    public Color warningColor = new Color(1f, 0f, 0f, 0.8f);

    [Header("Movement")]
    public float chargeSpeed = 12f;
    public float chargeDuration = 0.8f;
    public float jumpForce = 12f;

    [Header("Hitbox")]
    public Vector2 hitboxSize = new Vector2(3f, 2f);
    public Vector2 hitboxOffset = new Vector2(1.5f, 0f);

    [Header("Projectile")]
    public GameObject magicProjectilePrefab;
    public GameObject arrowProjectilePrefab;
    public float projectileSpeed = 8f;
    public float projectileSpawnHeight = 3f;
}
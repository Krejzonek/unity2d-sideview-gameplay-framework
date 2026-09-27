using System.Collections.Generic;
using UnityEngine;

public enum EnemyClass
{
    Melee,
    Ranged,
    Tank,
    Assassin,
    Support,
    Boss
}

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private EnemyClass enemyClass;
    [SerializeField] private ElementType enemyElement;

    [Header("Elite")]
    [SerializeField] private float eliteChance = 0.05f;
    [SerializeField] private float eliteScale = 1.5f;
    [SerializeField] private float eliteHealthMultiplier = 2f;
    [SerializeField] private float eliteDamageMultiplier = 1.5f;
    [SerializeField] private float eliteKnockbackResistance = 0.5f;
    private bool isElite;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float health = 50f;
    [SerializeField] private float damage = 10f;

    [Header("Headshot")]
    [SerializeField] private float headshotMultiplier = 2f;
    [SerializeField] private float headshotHeight = 0.8f;

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Stun")]
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float stunChance = 0f;

    [Header("Hit Feedback")]
    [SerializeField] private GameObject damageNumberPrefab;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1f;

    [Header("Ranged")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectilePoint;
    [SerializeField] private float rangedPreferredDistance = 5f;
    [SerializeField] private float rangedRetreatDistance = 2.5f;
    [SerializeField] private float projectileCritRate = 0f;
    [SerializeField] private float projectileCritDamage = 1f;

    [Header("Assassin")]
    [SerializeField] private float assassinSpeedMultiplier = 1.75f;

    [Header("Tank")]
    [SerializeField] private float tankSpeedMultiplier = 0.6f;

    [Header("Support")]
    [SerializeField] private float supportHealRange = 6f;
    [SerializeField] private float supportHealAmount = 15f;
    [SerializeField] private float supportHealCooldown = 5f;

    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private Player playerScript;

    [Header("Loot")]
    [SerializeField] private LootTable lootTable;
    private LootManager lootManager;

    private Rigidbody2D rb;
    private float attackTimer;
    private float knockbackTimer;
    private float stunTimer;
    private float supportHealTimer;

    private bool isKnockedBack;
    private bool isStunned;

    private Room room;

    private List<StatusEffect> activeStatusEffects = new();

    private Transform illusionTarget;
    private float illusionTimer;
    private bool isIllusion;

    private EnemyAI attackingEnemy;

    private bool isRooted;
    private bool isFrozen;
    private bool isBlind;
    private bool isShock;

    private float normalDetectionRange;
    private float normalMoveSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerScript = playerObject.GetComponent<Player>();
        }

        lootManager = FindFirstObjectByType<LootManager>();
        health = maxHealth;
        attackTimer = 0f;
        room = GetComponentInParent<Room>();
        rb.freezeRotation = true;

        normalDetectionRange = detectionRange;
        normalMoveSpeed = moveSpeed;

        if (enemyClass != EnemyClass.Boss && Random.value <= eliteChance)
            MakeElite();
    }

    private void MakeElite()
    {
        isElite = true;

        transform.localScale *= eliteScale;
        maxHealth *= eliteHealthMultiplier;
        damage *= eliteDamageMultiplier;

        switch (enemyClass)
        {
            case EnemyClass.Melee:
                attackRange *= 1.25f;
                break;

            case EnemyClass.Ranged:
                attackCooldown *= 0.75f;
                break;

            case EnemyClass.Tank:
                eliteKnockbackResistance = 0.25f;
                break;

            case EnemyClass.Assassin:
                assassinSpeedMultiplier *= 1.2f;
                break;

            case EnemyClass.Support:
                supportHealAmount *= 1.5f;
                supportHealCooldown *= 0.7f;
                break;
        }

        health = maxHealth;
    }

    private void Update()
    {
        if (player == null)
            return;

        attackTimer -= Time.deltaTime;

        UpdateStatusEffects();

        if (illusionTimer > 0f)
        {
            illusionTimer -= Time.deltaTime;

            if (illusionTimer <= 0f)
            {
                illusionTarget = null;
                attackingEnemy = null;
            }
        }

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0f)
                isStunned = false;
        }

        if (isStunned || isShock || isFrozen)
        {
            StopMoving();
            return;
        }

        if (isRooted)
        {
            StopMoving();

            if (attackingEnemy != null)
                AttackEnemy(attackingEnemy);
            else if (enemyClass != EnemyClass.Support)
                Attack();

            return;
        }

        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }

            return;
        }

        if (illusionTarget != null)
        {
            UpdateIllusionAI();
            return;
        }

        if (attackingEnemy != null)
        {
            UpdateEnemyDefense();
            return;
        }

        UpdateNormalAI();
    }

    private void UpdateNormalAI()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (isBlind)
            detectionRange = normalDetectionRange * 0.5f;
        else
            detectionRange = normalDetectionRange;

        switch (enemyClass)
        {
            case EnemyClass.Melee:
                moveSpeed = normalMoveSpeed;
                MeleeAI(distance);
                break;

            case EnemyClass.Ranged:
                moveSpeed = normalMoveSpeed;
                RangedAI(distance);
                break;

            case EnemyClass.Tank:
                moveSpeed = normalMoveSpeed * tankSpeedMultiplier;
                TankAI(distance);
                break;

            case EnemyClass.Assassin:
                moveSpeed = normalMoveSpeed * assassinSpeedMultiplier;
                AssassinAI(distance);
                break;

            case EnemyClass.Support:
                moveSpeed = normalMoveSpeed;
                SupportAI();
                break;

            case EnemyClass.Boss:
                return;
        }
    }

    private void UpdateIllusionAI()
    {
        if (illusionTarget == null)
            return;

        EnemyAI targetEnemy = illusionTarget.GetComponentInParent<EnemyAI>();

        if (targetEnemy == null || targetEnemy == this)
        {
            illusionTarget = null;
            return;
        }

        targetEnemy.SetEnemyTarget(this);

        float distance = Vector2.Distance(transform.position, illusionTarget.position);

        if (distance <= attackRange)
        {
            StopMoving();
            AttackEnemy(targetEnemy);
        }
        else
        {
            MoveTowardsTarget(illusionTarget);
        }
    }

    private void UpdateEnemyDefense()
    {
        if (attackingEnemy == null)
            return;

        if (attackingEnemy == this)
        {
            attackingEnemy = null;
            return;
        }

        float distance = Vector2.Distance(transform.position, attackingEnemy.transform.position);

        if (distance <= attackRange)
        {
            StopMoving();

            if (enemyClass != EnemyClass.Support)
                AttackEnemy(attackingEnemy);
        }
        else
        {
            MoveTowardsTarget(attackingEnemy.transform);
        }
    }

    public void SetEnemyTarget(EnemyAI enemy)
    {
        if (enemy == null || enemy == this)
            return;

        attackingEnemy = enemy;
    }

    private void MeleeAI(float distance)
    {
        if (distance <= attackRange)
        {
            StopMoving();
            Attack();
        }
        else if (distance <= detectionRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();
        }
    }

    private void RangedAI(float distance)
    {
        if (distance <= rangedRetreatDistance)
        {
            MoveAwayFromPlayer();
        }
        else if (distance <= rangedPreferredDistance)
        {
            StopMoving();
            Shoot();
        }
        else if (distance <= detectionRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();
        }
    }

    private void TankAI(float distance)
    {
        if (distance <= attackRange)
        {
            StopMoving();
            Attack();
        }
        else if (distance <= detectionRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();
        }
    }

    private void AssassinAI(float distance)
    {
        if (distance <= attackRange)
        {
            StopMoving();
            Attack();
        }
        else if (distance <= detectionRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();
        }
    }

    private void SupportAI()
    {
        EnemyAI woundedEnemy = FindWoundedEnemy();

        if (woundedEnemy == null)
        {
            StopMoving();
            return;
        }

        float enemyDistance = Vector2.Distance(transform.position, woundedEnemy.transform.position);

        if (enemyDistance <= supportHealRange)
        {
            StopMoving();
            HealEnemy(woundedEnemy);
        }
        else if (enemyDistance <= detectionRange)
        {
            MoveTowardsTarget(woundedEnemy.transform);
        }
        else
        {
            StopMoving();
        }
    }

    private EnemyAI FindWoundedEnemy()
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        EnemyAI closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null || enemy == this)
                continue;

            if (enemy.room != room)
                continue;

            if (enemy.health >= enemy.maxHealth)
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance > detectionRange)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    private void HealEnemy(EnemyAI target)
    {
        if (target == null)
            return;

        if (supportHealTimer > 0f)
        {
            supportHealTimer -= Time.deltaTime;
            return;
        }

        target.Heal(supportHealAmount);
        target.ShowHealNumber(supportHealAmount);
        supportHealTimer = supportHealCooldown;
    }

    public void Heal(float amount)
    {
        if (health <= 0f)
            return;

        health += amount;
        health = Mathf.Clamp(health, 0f, maxHealth);
    }

    private void MoveTowardsPlayer()
    {
        MoveTowardsTarget(player);
    }

    private void MoveTowardsTarget(Transform target)
    {
        if (target == null)
            return;

        float direction = Mathf.Sign(target.position.x - transform.position.x);
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

        if (direction != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }
    }

    private void MoveAwayFromPlayer()
    {
        if (player == null)
            return;

        float direction = Mathf.Sign(transform.position.x - player.position.x);
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

        if (direction != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }
    }

    private void StopMoving()
    {
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    private void Attack()
    {
        if (attackTimer > 0f)
            return;

        if (playerScript == null || player == null)
            return;

        if (Vector2.Distance(transform.position, player.position) > attackRange)
            return;

        playerScript.TakeDamage(damage);

        attackTimer = attackCooldown;
    }

    private void Shoot()
    {
        if (attackTimer > 0f)
            return;

        if (projectilePrefab == null || projectilePoint == null || player == null)
            return;

        Vector2 direction = (player.position - projectilePoint.position).normalized;

        GameObject projectileObject = Instantiate(projectilePrefab, projectilePoint.position, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Setup(direction, damage, projectileCritRate, projectileCritDamage, 0, stunChance, enemyElement, StatusEffectType.None, 0f, 0f, 0f, true);
            attackTimer = attackCooldown;
        }
        else
        {
            Destroy(projectileObject);
        }
    }

    private void AttackEnemy(EnemyAI target)
    {
        if (attackTimer > 0f)
            return;

        if (target == null)
            return;

        float attackDamage = damage;

        if (isIllusion)
            attackDamage *= 0.5f;

        target.TakeDamage(attackDamage, target.transform.position, 0f, Vector2.zero, 0f, false);

        attackTimer = attackCooldown;
    }

    public void TakeDamage(float damageAmount, Vector2 hitPoint, float knockback, Vector2 knockbackDirection, float stunChance, bool isCrit, StatusEffectType statusEffect = StatusEffectType.None, float statusChance = 0f, float statusDuration = 0f, float statusDamage = 0f)
    {
        bool headshot = IsHeadshot(hitPoint);

        if (headshot)
            damageAmount *= headshotMultiplier;

        health -= damageAmount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        ShowDamageNumber(damageAmount, isCrit, headshot);

        if (Random.Range(0f, 100f) <= stunChance)
            ApplyStun();

        if (statusEffect != StatusEffectType.None && Random.Range(0f, 100f) <= statusChance)
            ApplyStatusEffect(statusEffect, statusDuration, statusDamage);

        if (knockback > 0f)
            ApplyKnockback(knockbackDirection, knockback);

        if (health <= 0f)
            Die();
    }

    private void ApplyStatusEffect(StatusEffectType type, float duration, float statusDamage)
    {
        if (type == StatusEffectType.None)
            return;

        foreach (StatusEffect effect in activeStatusEffects)
        {
            if (effect.type != type)
                continue;

            effect.remainingDuration = Mathf.Max(effect.remainingDuration, duration);
            effect.damage = statusDamage;

            if (type == StatusEffectType.Illusion)
                ApplyIllusion(duration);

            return;
        }

        StatusEffect newEffect = new StatusEffect(type, duration, statusDamage);
        activeStatusEffects.Add(newEffect);

        switch (type)
        {
            case StatusEffectType.Root:
                isRooted = true;
                break;

            case StatusEffectType.Freeze:
                isFrozen = true;
                break;

            case StatusEffectType.Shock:
                isShock = true;
                rb.velocity = Vector2.zero;
                break;

            case StatusEffectType.Illusion:
                ApplyIllusion(duration);
                break;

            case StatusEffectType.Blind:
                isBlind = true;
                break;
        }
    }

    private void UpdateStatusEffects()
    {
        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeStatusEffects[i];

            effect.remainingDuration -= Time.deltaTime;
            effect.tickTimer -= Time.deltaTime;

            if (effect.type == StatusEffectType.Burn || effect.type == StatusEffectType.Drown || effect.type == StatusEffectType.Bleed || effect.type == StatusEffectType.Poison)
            {
                if (effect.tickTimer <= 0f)
                {
                    effect.tickTimer = effect.tickInterval;
                    TakeStatusDamage(effect.damage);
                }
            }

            if (effect.remainingDuration <= 0f)
            {
                RemoveStatusEffect(effect);
                activeStatusEffects.RemoveAt(i);
            }
        }
    }

    private void RemoveStatusEffect(StatusEffect effect)
    {
        switch (effect.type)
        {
            case StatusEffectType.Root:
                isRooted = false;
                break;

            case StatusEffectType.Freeze:
                isFrozen = false;
                break;

            case StatusEffectType.Shock:
                isShock = false;
                break;

            case StatusEffectType.Illusion:
                if (illusionTarget != null)
                {
                    EnemyAI targetEnemy = illusionTarget.GetComponentInParent<EnemyAI>();

                    if (targetEnemy != null)
                        targetEnemy.ClearEnemyTarget(this);
                }

                illusionTarget = null;
                illusionTimer = 0f;
                isIllusion = false;
                break;

            case StatusEffectType.Blind:
                isBlind = false;
                detectionRange = normalDetectionRange;
                break;
        }
    }

    public void ClearEnemyTarget(EnemyAI enemy)
    {
        if (attackingEnemy == enemy)
            attackingEnemy = null;
    }

    private void TakeStatusDamage(float damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        ShowDamageNumber(damageAmount, false, false);

        if (health <= 0f)
            Die();
    }

    private void ApplyIllusion(float duration)
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        EnemyAI closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null || enemy == this)
                continue;

            if (enemy.room != room)
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance > detectionRange)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy == null)
            return;

        isIllusion = true;
        illusionTarget = closestEnemy.transform;
        illusionTimer = duration;

        closestEnemy.SetEnemyTarget(this);
    }

    private void ShowDamageNumber(float damageAmount, bool isCrit, bool isHeadshot)
    {
        if (damageNumberPrefab == null)
            return;

        Collider2D collider = GetComponent<Collider2D>();
        Vector3 spawnPosition = transform.position;

        if (collider != null)
            spawnPosition = new Vector3(collider.bounds.center.x, collider.bounds.max.y + 0.2f, transform.position.z);

        GameObject damageNumberObject = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);
        DamageNumber damageNumber = damageNumberObject.GetComponent<DamageNumber>();

        if (damageNumber != null)
            damageNumber.Setup(damageAmount, isCrit, isHeadshot);
    }

    private void ApplyStun()
    {
        isStunned = true;
        stunTimer = stunDuration;
        rb.velocity = Vector2.zero;
    }

    private void ApplyKnockback(Vector2 direction, float force)
    {
        if (rb == null)
            return;

        if (isElite)
            force *= eliteKnockbackResistance;

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isKnockedBack)
            return;

        EnemyAI otherEnemy = collision.gameObject.GetComponentInParent<EnemyAI>();

        if (otherEnemy != null || Mathf.Abs(collision.GetContact(0).normal.x) > 0.3f)
        {
            rb.velocity = Vector2.zero;
            isKnockedBack = false;
            knockbackTimer = 0f;
        }
    }

    private bool IsHeadshot(Vector2 hitPoint)
    {
        Collider2D collider = GetComponent<Collider2D>();

        if (collider == null)
            return false;

        Bounds bounds = collider.bounds;
        float headStart = bounds.min.y + bounds.size.y * headshotHeight;

        return hitPoint.y >= headStart;
    }

    public ElementType GetElement()
    {
        return enemyElement;
    }

    public EnemyClass GetEnemyClass()
    {
        return enemyClass;
    }

    public float GetHeadshotMultiplier()
    {
        return headshotMultiplier;
    }

    private void Die()
    {
        if (playerScript != null)
            playerScript.GetEXP(isElite ? 250 : 100);

        if (lootManager != null && lootTable != null)
        {
            lootManager.GenerateLoot(lootTable, transform.position);

            if (isElite)
                lootManager.GenerateLoot(lootTable, transform.position);
        }

        if (room != null)
            room.EnemyKilled();

        Destroy(gameObject);
    }

    public float GetDamage()
    {
        return damage;
    }

    public float GetAttackRange()
    {
        return attackRange;
    }

    public Transform GetPlayer()
    {
        return player;
    }

    public Player GetPlayerScript()
    {
        return playerScript;
    }

    public float GetDetectionRange()
    {
        return detectionRange;
    }
   
    private void ShowHealNumber(float amount)
    {
        if (damageNumberPrefab == null)
            return;

        Collider2D collider = GetComponent<Collider2D>();
        Vector3 spawnPosition = transform.position;

        if (collider != null)
            spawnPosition = new Vector3(collider.bounds.center.x, collider.bounds.max.y + 0.2f, transform.position.z);

        GameObject damageNumberObject = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);
        DamageNumber damageNumber = damageNumberObject.GetComponent<DamageNumber>();

        if (damageNumber != null)
            damageNumber.Setup(amount, false, false, true);
    }
}
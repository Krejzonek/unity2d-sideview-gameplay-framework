using System.Collections.Generic;
using UnityEngine;

public enum EnemyClass
{
    Melee,
    Ranged,
    Mage,
    Tank,
    Assassin,
    Support,
    Boss
}

public enum EnemyAIType
{
    Simple,
    Smart
}

public enum EnemyAffix
{
    None,
    Armored,
    Swift,
    Vampiric,
    Regenerating,
    Brutal,
    Resilient
}
public enum EnemySpawnType
{
    None,
    FromGround,
    FromAir
}

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private EnemyClass enemyClass;
    [SerializeField] private EnemyAIType aiType;
    [SerializeField] private ElementType enemyElement;

    [Header("Elite")]
    [SerializeField] private float eliteChance = 0.05f;
    [SerializeField] private float eliteScale = 1.5f;
    [SerializeField] private float eliteHealthMultiplier = 2f;
    [SerializeField] private float eliteDamageMultiplier = 1.5f;
    [SerializeField] private float eliteKnockbackResistance = 0.5f;
    private bool isElite;
    private EnemyAffix eliteAffix = EnemyAffix.None;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float health = 50f;
    [SerializeField] private float damage = 10f;

    [Header("Tank")]
    [SerializeField] private float tankDamageReduction = 0.5f;

    [Header("Experience")]
    [SerializeField] private int baseExp = 100;
    [SerializeField] private float eliteExpMultiplier = 2.5f;

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

    [Header("Mage")]
    [SerializeField] private float magePreferredDistance = 7f;
    [SerializeField] private float mageRetreatDistance = 3.5f;

    [Header("Assassin")]
    [SerializeField] private float assassinSpeedMultiplier = 1.75f;

    [Header("Support")]
    [SerializeField] private float supportHealRange = 6f;
    [SerializeField] private float supportHealAmount = 15f;
    [SerializeField] private float supportHealCooldown = 5f;

    [Header("Smart AI")]
    [SerializeField] private float smartTargetChangeInterval = 0.75f;
    [SerializeField] private float smartTargetRange = 20f;
    [SerializeField] private float smartRangedPreferredDistance = 7f;
    [SerializeField] private float smartRangedRetreatDistance = 4f;
    [SerializeField] private float smartMagePreferredDistance = 8f;
    [SerializeField] private float smartMageRetreatDistance = 4f;
    [SerializeField] private float smartSupportDangerDistance = 4f;

    [Header("Smart Dodge")]
    [SerializeField] private float smartDodgeDistance = 1.2f;
    [SerializeField] private float smartDodgeTime = 0.4f;
    [SerializeField] private float smartDodgeCooldown = 1f;
    [SerializeField] private float smartProjectileLookAhead = 1f;
    [SerializeField] private float smartProjectileDetectionDistance = 10f;

    [Header("Counter")]
    [SerializeField] private float counterMemoryTime = 5f;
    [SerializeField] private float counterDetectionDistance = 3f;
    [SerializeField] private float counterDetectionWidth = 1.5f;

    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private Player playerScript;

    [Header("Loot")]
    [SerializeField] private LootTable lootTable;
    private LootManager lootManager;

    [Header("Spawn")]
    [SerializeField] private EnemySpawnType spawnType = EnemySpawnType.None;
    [SerializeField] private float spawnDistance = 3f;
    [SerializeField] private float spawnTime = 0.8f;

    private bool isSpawning;

    private Rigidbody2D rb;
    private float attackTimer;
    private float knockbackTimer;
    private float stunTimer;
    private float supportHealTimer;
    private float smartTargetTimer;
    private float counterTimer;
    private float smartDodgeTimer;
    private ElementType lastDamageElement = ElementType.None;
    private bool isKnockedBack;
    private bool isStunned;
    private bool isDead;
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
    private bool restorePending;
    private MapSaveSystem.EnemySaveData pendingSaveData;
    private Player lastAttacker;
    private Player smartTarget;
    private Player counterTarget;

    private Vector2 dodgeDirection;
    private float dodgeTimer;

    private Dictionary<Projectile, Vector3> projectilePreviousPositions = new();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
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


        if (restorePending)
        {
            maxHealth = pendingSaveData.maxHealth;
            health = pendingSaveData.health;
            damage = pendingSaveData.damage;
            moveSpeed = pendingSaveData.moveSpeed;
            detectionRange = pendingSaveData.detectionRange;
            attackRange = pendingSaveData.attackRange;
            attackCooldown = pendingSaveData.attackCooldown;
            eliteKnockbackResistance = pendingSaveData.eliteKnockbackResistance;
            assassinSpeedMultiplier = pendingSaveData.assassinSpeedMultiplier;
            supportHealAmount = pendingSaveData.supportHealAmount;
            supportHealCooldown = pendingSaveData.supportHealCooldown;
            normalDetectionRange = pendingSaveData.normalDetectionRange;
            normalMoveSpeed = pendingSaveData.normalMoveSpeed;
            isElite = pendingSaveData.isElite;
            eliteAffix = pendingSaveData.eliteAffix;
            transform.localScale = pendingSaveData.localScale;
            restorePending = false;
            pendingSaveData = null;
        }
        else if (enemyClass != EnemyClass.Boss && Random.value <= eliteChance)
        {
            MakeElite();
        }
    }

    private void MakeElite()
    {
        isElite = true;
        eliteAffix = GetRandomAffix();

        transform.localScale *= eliteScale;
        maxHealth *= eliteHealthMultiplier;
        damage *= eliteDamageMultiplier;

        ApplyEliteAffix();

        switch (enemyClass)
        {
            case EnemyClass.Melee:
                attackRange *= 1.25f;
                break;

            case EnemyClass.Ranged:
                attackCooldown *= 0.75f;
                break;

            case EnemyClass.Mage:
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

    private EnemyAffix GetRandomAffix()
    {
        EnemyAffix[] affixes =
        {
            EnemyAffix.Armored,
            EnemyAffix.Swift,
            EnemyAffix.Vampiric,
            EnemyAffix.Regenerating,
            EnemyAffix.Brutal,
            EnemyAffix.Resilient
        };

        return affixes[Random.Range(0, affixes.Length)];
    }

    private void ApplyEliteAffix()
    {
        switch (eliteAffix)
        {
            case EnemyAffix.Armored:
                break;

            case EnemyAffix.Swift:
                moveSpeed *= 1.35f;
                normalMoveSpeed *= 1.35f;
                break;

            case EnemyAffix.Vampiric:
                break;

            case EnemyAffix.Regenerating:
                break;

            case EnemyAffix.Brutal:
                damage *= 1.3f;
                break;

            case EnemyAffix.Resilient:
                eliteKnockbackResistance = 0.25f;
                break;
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        if (isSpawning)
        {
            StopMoving();
            return;
        }

        attackTimer -= Time.deltaTime;
        smartDodgeTimer -= Time.deltaTime;
        counterTimer -= Time.deltaTime;

        if (counterTimer <= 0f)
            counterTarget = null;

        UpdateStatusEffects();
        UpdateEliteAffix();

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

        if (aiType == EnemyAIType.Smart && enemyClass != EnemyClass.Boss)
        {
            if (UpdateSmartDodge())
                return;
        }

        UpdateNormalAI();
    }

    private void LateUpdate()
    {
        UpdateProjectilePositions();
    }

    private void UpdateEliteAffix()
    {
        if (!isElite)
            return;

        if (eliteAffix == EnemyAffix.Regenerating && health > 0f)
        {
            health += maxHealth * 0.02f * Time.deltaTime;
            health = Mathf.Min(health, maxHealth);
        }
    }

    private void UpdateNormalAI()
    {
        if (aiType == EnemyAIType.Simple)
            UpdateSimpleAI();
        else
            UpdateSmartAI();
    }

    private void UpdateSimpleAI()
    {
        if (counterTarget != null && !counterTarget.IsDead() && counterTimer > 0f)
        {
            UpdateSimpleCounter();
            return;
        }

        counterTarget = null;

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

            case EnemyClass.Mage:
                moveSpeed = normalMoveSpeed;
                MageAI(distance);
                break;

            case EnemyClass.Tank:
                moveSpeed = normalMoveSpeed * 0.6f;
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

    private void UpdateSimpleCounter()
    {
        if (counterTarget == null || counterTarget.IsDead())
        {
            counterTarget = null;
            return;
        }

        if (enemyClass == EnemyClass.Support)
        {
            MoveAwayFromTarget(counterTarget.transform);
            return;
        }

        player = counterTarget.transform;
        playerScript = counterTarget;

        float distance = Vector2.Distance(transform.position, counterTarget.transform.position);

        if (enemyClass == EnemyClass.Ranged)
        {
            if (distance <= rangedRetreatDistance)
            {
                MoveAwayFromTarget(counterTarget.transform);
                return;
            }

            if (distance <= rangedPreferredDistance)
            {
                StopMoving();
                Shoot();
                return;
            }

            MoveTowardsTarget(counterTarget.transform);
            return;
        }

        if (enemyClass == EnemyClass.Mage)
        {
            if (distance <= mageRetreatDistance)
            {
                MoveAwayFromTarget(counterTarget.transform);
                return;
            }

            if (distance <= magePreferredDistance)
            {
                StopMoving();
                Shoot();
                return;
            }

            MoveTowardsTarget(counterTarget.transform);
            return;
        }

        if (enemyClass == EnemyClass.Melee || enemyClass == EnemyClass.Tank || enemyClass == EnemyClass.Assassin)
        {
            EnemyAI enemyTarget = FindEnemyOnCounterPath();

            if (enemyTarget != null)
            {
                float enemyDistance = Vector2.Distance(transform.position, enemyTarget.transform.position);

                if (enemyDistance <= attackRange)
                {
                    StopMoving();
                    AttackEnemy(enemyTarget);
                }
                else
                {
                    MoveTowardsTarget(enemyTarget.transform);
                }

                return;
            }
        }

        if (distance <= attackRange)
        {
            StopMoving();
            Attack();
            return;
        }

        MoveTowardsTarget(counterTarget.transform);
    }

    private EnemyAI FindEnemyOnCounterPath()
    {
        if (counterTarget == null)
            return null;

        Vector2 direction = (counterTarget.transform.position - transform.position).normalized;

        if (direction == Vector2.zero)
            return null;

        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        EnemyAI closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null || enemy == this)
                continue;

            if (!enemy.gameObject.activeInHierarchy)
                continue;

            if (enemy.room != room)
                continue;

            if (enemy.enemyClass == EnemyClass.Boss)
                continue;

            Vector2 offset = enemy.transform.position - transform.position;
            float forwardDistance = Vector2.Dot(offset, direction);

            if (forwardDistance <= attackRange)
                continue;

            if (forwardDistance > counterDetectionDistance)
                continue;

            float sideDistance = Mathf.Abs(Vector2.Dot(offset, Vector2.Perpendicular(direction)));

            if (sideDistance > counterDetectionWidth)
                continue;

            if (forwardDistance < closestDistance)
            {
                closestDistance = forwardDistance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    private void UpdateSmartAI()
    {
        if (counterTarget != null && !counterTarget.IsDead())
        {
            smartTarget = counterTarget;
        }
        else
        {
            smartTargetTimer -= Time.deltaTime;

            if (smartTarget == null || smartTarget.IsDead() || smartTargetTimer <= 0f)
            {
                smartTarget = FindSmartTarget();
                smartTargetTimer = smartTargetChangeInterval;
            }
        }

        if (enemyClass == EnemyClass.Support)
        {
            moveSpeed = normalMoveSpeed;
            SmartSupportAI();
            return;
        }

        if (smartTarget == null)
        {
            StopMoving();
            return;
        }

        player = smartTarget.transform;
        playerScript = smartTarget;

        float distance = Vector2.Distance(transform.position, player.position);

        if (isBlind)
            detectionRange = normalDetectionRange * 0.5f;
        else
            detectionRange = normalDetectionRange;

        switch (enemyClass)
        {
            case EnemyClass.Melee:
                moveSpeed = normalMoveSpeed;
                SmartMeleeAI(distance);
                break;

            case EnemyClass.Ranged:
                moveSpeed = normalMoveSpeed;
                SmartRangedAI(distance);
                break;

            case EnemyClass.Mage:
                moveSpeed = normalMoveSpeed;
                SmartMageAI(distance);
                break;

            case EnemyClass.Tank:
                moveSpeed = normalMoveSpeed * 0.6f;
                SmartTankAI(distance);
                break;

            case EnemyClass.Assassin:
                moveSpeed = normalMoveSpeed * assassinSpeedMultiplier;
                SmartAssassinAI(distance);
                break;

            case EnemyClass.Boss:
                return;
        }
    }

    private Player FindSmartTarget()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        Player bestTarget = null;
        float bestScore = float.MinValue;

        foreach (Player targetPlayer in players)
        {
            if (targetPlayer == null || targetPlayer.IsDead())
                continue;

            Room targetRoom = targetPlayer.GetComponentInParent<Room>();

            if (room != null && targetRoom != null && targetRoom != room)
                continue;

            float distance = Vector2.Distance(transform.position, targetPlayer.transform.position);

            if (distance > smartTargetRange)
                continue;

            float score = GetTargetScore(targetPlayer, distance);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = targetPlayer;
            }
        }

        return bestTarget;
    }

    private float GetTargetScore(Player targetPlayer, float distance)
    {
        float score = 100f - distance * 5f;

        if (targetPlayer == lastAttacker)
            score += 40f;

        CharacterData data = targetPlayer.GetCharacterData();

        string className = "";

        if (data != null && data.characterClass != null)
            className = data.characterClass.className;

        switch (enemyClass)
        {
            case EnemyClass.Melee:

                if (className == "Warrior")
                    score += 20f;

                if (className == "Archer" || className == "Mage")
                    score += 10f;

                break;

            case EnemyClass.Ranged:

                if (className == "Archer")
                    score += 25f;

                if (className == "Mage")
                    score += 30f;

                if (className == "Warrior")
                    score -= 20f;

                break;

            case EnemyClass.Mage:

                if (className == "Mage")
                    score += 35f;

                if (className == "Archer")
                    score += 25f;

                if (className == "Warrior")
                    score -= 20f;

                break;

            case EnemyClass.Tank:

                if (targetPlayer == lastAttacker)
                    score += 50f;

                if (className == "Warrior")
                    score += 20f;

                break;

            case EnemyClass.Assassin:

                if (className == "Archer")
                    score += 50f;

                if (className == "Mage")
                    score += 50f;

                if (className == "Warrior")
                    score -= 30f;

                break;
        }

        return score;
    }

    private void SmartMeleeAI(float distance)
    {
        if (distance <= attackRange)
        {
            StopMoving();
            Attack();
        }
        else if (distance <= detectionRange || counterTarget != null)
        {
            MoveTowardsTarget(player);
        }
        else
        {
            StopMoving();
        }
    }

    private void SmartRangedAI(float distance)
    {
        if (distance <= smartRangedRetreatDistance)
        {
            MoveAwayFromPlayer();
        }
        else if (distance <= smartRangedPreferredDistance)
        {
            StopMoving();
            Shoot();
        }
        else if (distance <= detectionRange || counterTarget != null)
        {
            MoveTowardsTarget(player);
        }
        else
        {
            StopMoving();
        }
    }

    private void SmartMageAI(float distance)
    {
        if (distance <= smartMageRetreatDistance)
        {
            MoveAwayFromPlayer();
        }
        else if (distance <= smartMagePreferredDistance)
        {
            StopMoving();
            Shoot();
        }
        else if (distance <= detectionRange || counterTarget != null)
        {
            MoveTowardsTarget(player);
        }
        else
        {
            StopMoving();
        }
    }

    private void SmartTankAI(float distance)
    {
        if (distance <= attackRange)
        {
            StopMoving();
            Attack();
        }
        else if (distance <= detectionRange || counterTarget != null)
        {
            MoveTowardsTarget(player);
        }
        else
        {
            StopMoving();
        }
    }

    private void SmartAssassinAI(float distance)
    {
        if (distance <= attackRange)
        {
            StopMoving();
            Attack();
        }
        else if (distance <= detectionRange || counterTarget != null)
        {
            MoveTowardsTarget(player);
        }
        else
        {
            StopMoving();
        }
    }

    private void SmartSupportAI()
    {
        if (counterTarget != null && !counterTarget.IsDead())
        {
            float attackerDistance = Vector2.Distance(transform.position, counterTarget.transform.position);

            if (attackerDistance <= smartSupportDangerDistance)
            {
                MoveAwayFromTarget(counterTarget.transform);
                return;
            }
        }

        EnemyAI woundedEnemy = FindSmartWoundedEnemy();

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

    private EnemyAI FindSmartWoundedEnemy()
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        EnemyAI bestEnemy = null;
        float bestScore = float.MinValue;

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

            float healthPercent = enemy.health / enemy.maxHealth;
            float missingHealth = 1f - healthPercent;

            float score = missingHealth * 100f - distance * 2f;

            switch (enemy.enemyClass)
            {
                case EnemyClass.Tank:
                    score += 15f;
                    break;

                case EnemyClass.Assassin:
                    score += 10f;
                    break;

                case EnemyClass.Ranged:
                    score += 20f;
                    break;

                case EnemyClass.Mage:
                    score += 20f;
                    break;

                case EnemyClass.Support:
                    score += 25f;
                    break;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestEnemy = enemy;
            }
        }

        return bestEnemy;
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

    private void MageAI(float distance)
    {
        if (distance <= mageRetreatDistance)
        {
            MoveAwayFromPlayer();
        }
        else if (distance <= magePreferredDistance)
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

        MoveAwayFromTarget(player);
    }

    private void MoveAwayFromTarget(Transform target)
    {
        if (target == null)
            return;

        float direction = Mathf.Sign(transform.position.x - target.position.x);

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
        if (rb == null)
            return;
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

        if (isElite && eliteAffix == EnemyAffix.Vampiric)
            Heal(damage * 0.1f);

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

        if (isElite && eliteAffix == EnemyAffix.Vampiric)
            Heal(attackDamage * 0.1f);

        attackTimer = attackCooldown;
    }

    public void TakeDamage(float damageAmount, Vector2 hitPoint, float knockback, Vector2 knockbackDirection, float stunChance, bool isCrit, StatusEffectType statusEffect = StatusEffectType.None, float statusChance = 0f, float statusDuration = 0f, float statusDamage = 0f, Player attacker = null, ElementType damageElement = ElementType.None)
    {
        if (attacker != null)
        {
            lastAttacker = attacker;
            lastDamageElement = damageElement;

            if (enemyClass != EnemyClass.Boss)
            {
                counterTarget = attacker;
                counterTimer = counterMemoryTime;

                player = attacker.transform;
                playerScript = attacker;

                if (aiType == EnemyAIType.Smart)
                    smartTarget = attacker;
            }
        }

        bool headshot = IsHeadshot(hitPoint);

        if (headshot)
            damageAmount *= headshotMultiplier;

        if(attacker != null && TeamManager.Instance != null)
        {
            damageAmount *= TeamManager.Instance.GetComboDamageMultiplier();
            TeamManager.Instance.RegisterComboHit();
        }

        if (enemyClass == EnemyClass.Tank && IsTankFrontHit(hitPoint))
            damageAmount *= 1f - tankDamageReduction;

        if (isElite && eliteAffix == EnemyAffix.Armored)
            damageAmount *= 0.75f;

        health -= damageAmount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        
            

        ShowDamageNumber(damageAmount, isCrit, headshot, damageElement);

        if (Random.Range(0f, 100f) <= stunChance)
            ApplyStun();

        if (statusEffect != StatusEffectType.None && Random.Range(0f, 100f) <= statusChance)
            ApplyStatusEffect(statusEffect, statusDuration, statusDamage, damageElement);

        if (knockback > 0f)
            ApplyKnockback(knockbackDirection, knockback);

        if (health <= 0f)
            Die();
    }

    private bool IsTankFrontHit(Vector2 hitPoint)
    {
        Collider2D collider = GetComponent<Collider2D>();

        if (collider == null)
            return false;

        float hitHeight = hitPoint.y - collider.bounds.center.y;

        if (hitHeight > collider.bounds.size.y * 0.3f)
            return false;

        float hitDirection = hitPoint.x - collider.bounds.center.x;

        return hitDirection < 0f;
    }

    private float GetDodgeChance()
    {
        switch (enemyClass)
        {
            case EnemyClass.Tank:
                return 0.25f;

            case EnemyClass.Melee:
                return 0.5f;

            case EnemyClass.Ranged:
                return 0.75f;

            case EnemyClass.Assassin:
                return 1f;

            case EnemyClass.Mage:
            case EnemyClass.Support:
            default:
                return 0f;
        }
    }

    private bool UpdateSmartDodge()
    {
        if (smartDodgeTimer > 0f)
            return false;

        if (dodgeTimer > 0f)
        {
            dodgeTimer -= Time.deltaTime;
            rb.velocity = new Vector2(dodgeDirection.x * moveSpeed * 1.5f, rb.velocity.y);
            return true;
        }

        Projectile projectile = FindIncomingProjectile();

        if (projectile == null)
            return false;

        if (Random.value > GetDodgeChance())
        {
            smartDodgeTimer = smartDodgeCooldown;
            return false;
        }

        Vector2 direction = GetDodgeDirection(projectile);

        if (direction == Vector2.zero)
            return false;

        dodgeDirection = direction;
        dodgeTimer = smartDodgeTime;
        smartDodgeTimer = smartDodgeCooldown;

        if (lastAttacker != null && !lastAttacker.IsDead())
        {
            counterTarget = lastAttacker;
            counterTimer = counterMemoryTime;
            smartTarget = lastAttacker;
        }

        rb.velocity = new Vector2(dodgeDirection.x * moveSpeed * 1.5f, rb.velocity.y);

        return true;
    }

    private Projectile FindIncomingProjectile()
    {
        Projectile[] projectiles = FindObjectsByType<Projectile>(FindObjectsSortMode.None);

        Projectile bestProjectile = null;
        float bestTime = Mathf.Infinity;

        foreach (Projectile projectile in projectiles)
        {
            if (projectile == null || !projectile.gameObject.activeInHierarchy)
                continue;

            Vector3 currentPosition = projectile.transform.position;

            if (!projectilePreviousPositions.TryGetValue(projectile, out Vector3 previousPosition))
                continue;

            Vector2 velocity = (currentPosition - previousPosition) / Mathf.Max(Time.deltaTime, 0.001f);

            if (velocity.sqrMagnitude < 0.1f)
                continue;

            Vector2 toEnemy = (Vector2)transform.position - (Vector2)currentPosition;

            float distance = toEnemy.magnitude;

            if (distance > smartProjectileDetectionDistance)
                continue;

            Vector2 velocityDirection = velocity.normalized;
            float forwardDistance = Vector2.Dot(toEnemy, velocityDirection);

            if (forwardDistance <= 0f)
                continue;

            float timeToEnemy = forwardDistance / velocity.magnitude;

            if (timeToEnemy > smartProjectileLookAhead)
                continue;

            Vector2 closestPoint = (Vector2)currentPosition + velocityDirection * forwardDistance;
            float closestDistance = Vector2.Distance(transform.position, closestPoint);

            if (closestDistance > smartDodgeDistance)
                continue;

            if (timeToEnemy < bestTime)
            {
                bestTime = timeToEnemy;
                bestProjectile = projectile;
            }
        }

        return bestProjectile;
    }

    private Vector2 GetDodgeDirection(Projectile projectile)
    {
        if (projectile == null)
            return Vector2.zero;

        Vector2 projectilePosition = projectile.transform.position;

        Vector2 leftPosition = (Vector2)transform.position + Vector2.left * smartDodgeDistance;
        Vector2 rightPosition = (Vector2)transform.position + Vector2.right * smartDodgeDistance;

        bool leftBlocked = Physics2D.Linecast(transform.position, leftPosition).collider != null;
        bool rightBlocked = Physics2D.Linecast(transform.position, rightPosition).collider != null;

        if (leftBlocked && rightBlocked)
            return Vector2.zero;

        if (leftBlocked)
            return Vector2.right;

        if (rightBlocked)
            return Vector2.left;

        if (projectilePosition.x < transform.position.x)
            return Vector2.right;

        return Vector2.left;
    }

    private void UpdateProjectilePositions()
    {
        Projectile[] projectiles = FindObjectsByType<Projectile>(FindObjectsSortMode.None);

        HashSet<Projectile> activeProjectiles = new HashSet<Projectile>();

        foreach (Projectile projectile in projectiles)
        {
            if (projectile == null)
                continue;

            activeProjectiles.Add(projectile);
            projectilePreviousPositions[projectile] = projectile.transform.position;
        }

        List<Projectile> oldProjectiles = new List<Projectile>();

        foreach (Projectile projectile in projectilePreviousPositions.Keys)
        {
            if (!activeProjectiles.Contains(projectile))
                oldProjectiles.Add(projectile);
        }

        foreach (Projectile projectile in oldProjectiles)
            projectilePreviousPositions.Remove(projectile);
    }

    private void UpdateIllusionAI()
    {
        if (illusionTarget == null)
            return;

        EnemyAI targetEnemy = illusionTarget.GetComponentInParent<EnemyAI>();

        if (targetEnemy == null || targetEnemy.IsDead())
        {
            illusionTarget = null;
            return;
        }

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
        if (attackingEnemy == null || attackingEnemy.IsDead())
        {
            attackingEnemy = null;
            return;
        }

        float distance = Vector2.Distance(transform.position, attackingEnemy.transform.position);

        if (distance <= attackRange)
        {
            StopMoving();
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

    private void ApplyStatusEffect(StatusEffectType type, float duration, float statusDamage, ElementType element)
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

        StatusEffect newEffect = new StatusEffect(type, duration, statusDamage, element);
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
                    TakeStatusDamage(effect);
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

    private void TakeStatusDamage(StatusEffect statusEffect)
    {
        float damageAmount = statusEffect.damage;
        float elementMultiplier = ElementSystem.GetDamageMultiplier(statusEffect.element, enemyElement);

        damageAmount *= elementMultiplier;

        health -= damageAmount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        ShowDamageNumber(damageAmount, false, false, statusEffect.element);

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

    private void ShowDamageNumber(float damageAmount, bool isCrit, bool isHeadshot, ElementType element)
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
            damageNumber.Setup(damageAmount, isCrit, isHeadshot, element, false);
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

    public bool IsDead()
    {
        return health <= 0f;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        int exp = Mathf.RoundToInt(baseExp * (isElite ? eliteExpMultiplier : 1f));

        if (lastAttacker != null)
            lastAttacker.GetEXP(exp);

        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (Player teamPlayer in players)
        {
            if (teamPlayer == null || teamPlayer == lastAttacker || teamPlayer.IsDead())
                continue;

            teamPlayer.GetEXP(Mathf.RoundToInt(exp * 0.5f));
        }

        if (lootManager != null && lootTable != null)
        {
            lootManager.GenerateLoot(lootTable, transform.position, lastAttacker);

            if (isElite)
                lootManager.GenerateLoot(lootTable, transform.position, lastAttacker);
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
            damageNumber.Setup(amount, false, false, ElementType.None, true);
    }

    public EnemySpawnType GetSpawnType()
    {
        return spawnType;
    }

    public float GetSpawnDistance()
    {
        return spawnDistance;
    }

    public float GetSpawnTime()
    {
        return spawnTime;
    }

    public void SetSpawning(bool value)
    {
        isSpawning = value;

        if (value)
            StopMoving();
    }

    public MapSaveSystem.EnemySaveData GetSaveData(string prefabName)
    {
        return new MapSaveSystem.EnemySaveData
        {
            prefabName = prefabName,
            position = transform.position,
            localScale = transform.localScale,
            health = health,
            maxHealth = maxHealth,
            damage = damage,
            moveSpeed = moveSpeed,
            detectionRange = detectionRange,
            attackRange = attackRange,
            attackCooldown = attackCooldown,
            eliteKnockbackResistance = eliteKnockbackResistance,
            assassinSpeedMultiplier = assassinSpeedMultiplier,
            supportHealAmount = supportHealAmount,
            supportHealCooldown = supportHealCooldown,
            normalDetectionRange = normalDetectionRange,
            normalMoveSpeed = normalMoveSpeed,
            isElite = isElite,
            eliteAffix = eliteAffix
        };
    }

    public void RestoreFromSave(MapSaveSystem.EnemySaveData data)
    {
        if (data == null)
            return;

        pendingSaveData = data;
        restorePending = true;
    }
}
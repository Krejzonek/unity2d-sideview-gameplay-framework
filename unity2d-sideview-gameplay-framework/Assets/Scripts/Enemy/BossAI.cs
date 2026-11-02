using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    private enum BossState
    {
        Idle,
        Windup,
        Attacking,
        Recovery
    }

    [Header("Boss Class")]
    [SerializeField] private BossClassData bossClassData;

    [Header("Attack Settings")]
    [SerializeField] private bool enableAttacks = true;
    [SerializeField] private bool enablePhaseSystem = true;
    [SerializeField] private bool enableEnrage = true;
    [SerializeField] private float attackCheckInterval = 0.2f;
    [SerializeField] private float attackDetectionPadding = 0.5f;

    [Header("Attack Toggles")]
    [SerializeField] private bool enableCharge = true;
    [SerializeField] private bool enableGroundSlam = true;
    [SerializeField] private bool enableShockwave = true;
    [SerializeField] private bool enableUppercut = true;
    [SerializeField] private bool enableFireball = true;
    [SerializeField] private bool enableMeteor = true;
    [SerializeField] private bool enableArrow = true;
    [SerializeField] private bool enableMagicBeam = true;
    [SerializeField] private bool enableExplosion = true;
    [SerializeField] private bool enablePounce = true;
    [SerializeField] private bool enableTailSweep = true;
    [SerializeField] private bool enableRoar = true;

    [Header("Detection")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float obstacleCheckOffset = 0.5f;
    [SerializeField] private float groundOffset = 0f;
    [SerializeField] private float detectionCheckInterval = 0.2f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    private EnemyAI enemyAI;
    private Rigidbody2D rb;
    private Collider2D bossCollider;
    private Collider2D roomBounds;
    private Room room;
    private Transform player;
    private Player playerScript;
    private BossAttackWarning warning;

    private readonly Dictionary<BossAttackData, float> attackCooldowns = new Dictionary<BossAttackData, float>();
    private readonly HashSet<Player> hitPlayers = new HashSet<Player>();
    private readonly List<Collider2D> chargeContacts = new List<Collider2D>();

    private ContactFilter2D chargeContactFilter;

    private BossState state = BossState.Idle;
    private BossAttackData currentAttack;

    private float stateTimer;
    private float attackCheckTimer;
    private float detectionCheckTimer;
    private float attackDirection;
    private float phaseCooldownMultiplier = 1f;
    private float phaseDamageMultiplier = 1f;
    private bool secondPhase;
    private bool enraged;
    private bool chargeHit;

    private void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<Collider2D>();

        if (enemyAI == null || rb == null || bossClassData == null)
        {
            Debug.LogError("[BossAI] Missing EnemyAI, Rigidbody2D or BossClassData.", this);
            enabled = false;
            return;
        }

        if (bossCollider == null)
        {
            Debug.LogError("[BossAI] Missing Collider2D.", this);
            enabled = false;
            return;
        }

        room = GetComponentInParent<Room>();

        if (room == null)
        {
            Debug.LogError("[BossAI] Missing parent Room.", this);
            enabled = false;
            return;
        }

        roomBounds = room.GetMapBounds();

        if (roomBounds == null)
        {
            Debug.LogError("[BossAI] Missing Room MapBounds.", this);
            enabled = false;
            return;
        }

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        chargeContactFilter = new ContactFilter2D();
        chargeContactFilter.NoFilter();
        chargeContactFilter.useTriggers = true;

        warning = new GameObject("BossAttackWarning").AddComponent<BossAttackWarning>();

        UpdatePlayer();
    }

    private void Update()
    {
        if (enemyAI == null || rb == null)
            return;

        detectionCheckTimer -= Time.deltaTime;

        if (detectionCheckTimer <= 0f)
        {
            detectionCheckTimer = detectionCheckInterval;
            UpdatePlayer();
        }

        if (player == null || playerScript == null || playerScript.IsDead())
        {
            StopBoss();
            return;
        }

        UpdateCooldowns();
        UpdatePhases();

        switch (state)
        {
            case BossState.Idle:
                UpdateIdle();
                break;

            case BossState.Windup:
                UpdateWindup();
                break;

            case BossState.Attacking:
                UpdateAttacking();
                break;

            case BossState.Recovery:
                UpdateRecovery();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (state != BossState.Attacking || currentAttack == null)
            return;

        if (currentAttack.attackType == BossAttackType.Charge && !chargeHit)
            PerformChargeHit();
    }

    private void UpdatePlayer()
    {
        if (roomBounds == null)
        {
            player = null;
            playerScript = null;
            return;
        }

        Bounds bounds = roomBounds.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);

        Player detectedPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            Player target = hit.GetComponentInParent<Player>();

            if (target == null || target.IsDead())
                continue;

            float distance = Vector2.Distance(bossCollider.bounds.center, target.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                detectedPlayer = target;
            }
        }

        if (detectedPlayer != null)
        {
            playerScript = detectedPlayer;
            player = detectedPlayer.transform;
        }
        else
        {
            player = null;
            playerScript = null;
        }
    }

    private float GetDistanceToPlayer()
    {
        if (player == null)
            return float.MaxValue;

        Collider2D playerCollider = playerScript != null
            ? playerScript.GetComponent<Collider2D>()
            : null;

        if (playerCollider == null || bossCollider == null)
            return Vector2.Distance(transform.position, player.position);

        return Vector2.Distance(bossCollider.bounds.center, playerCollider.bounds.center);
    }

    private void UpdateCooldowns()
    {
        List<BossAttackData> keys = new List<BossAttackData>(attackCooldowns.Keys);

        foreach (BossAttackData attack in keys)
            attackCooldowns[attack] = Mathf.Max(0f, attackCooldowns[attack] - Time.deltaTime);
    }

    private void UpdatePhases()
    {
        if (!enablePhaseSystem || bossClassData == null)
            return;

        float healthPercent = enemyAI.GetHealthPercent();

        if (!secondPhase && healthPercent <= bossClassData.secondPhaseHealth)
        {
            secondPhase = true;
            phaseCooldownMultiplier = bossClassData.secondPhaseCooldownMultiplier;
        }

        if (enableEnrage && !enraged && healthPercent <= bossClassData.enrageHealth)
        {
            enraged = true;
            phaseCooldownMultiplier = bossClassData.enrageCooldownMultiplier;
            phaseDamageMultiplier = bossClassData.enrageDamageMultiplier;
        }
    }

    private void UpdateIdle()
    {
        if (!enableAttacks)
        {
            StopMoving();
            return;
        }

        if (!IsTargetInAttackRange())
            MoveTowardsPlayer();
        else
            StopMoving();

        attackCheckTimer -= Time.deltaTime;

        if (attackCheckTimer > 0f)
            return;

        attackCheckTimer = attackCheckInterval;

        BossAttackData attack = ChooseAttack();

        if (attack != null)
            StartAttack(attack);
    }

    private bool IsTargetInAttackRange()
    {
        if (player == null || bossClassData == null || bossClassData.attacks == null)
            return false;

        float distance = GetDistanceToPlayer();

        foreach (BossAttackData attack in bossClassData.attacks)
        {
            if (attack == null || !IsAttackEnabled(attack.attackType))
                continue;

            if (distance >= attack.minDistance && distance <= attack.maxDistance)
                return true;
        }

        return false;
    }

    private void MoveTowardsPlayer()
    {
        if (player == null || bossClassData == null || bossClassData.attacks == null)
        {
            StopMoving();
            return;
        }

        float distance = GetDistanceToPlayer();
        float closestRangeDifference = float.MaxValue;
        float movementDirection = 0f;

        foreach (BossAttackData attack in bossClassData.attacks)
        {
            if (attack == null || !IsAttackEnabled(attack.attackType))
                continue;

            if (distance >= attack.minDistance && distance <= attack.maxDistance)
            {
                StopMoving();
                return;
            }

            float difference;
            float direction;

            if (distance > attack.maxDistance)
            {
                difference = distance - attack.maxDistance;
                direction = 1f;
            }
            else
            {
                difference = attack.minDistance - distance;
                direction = -1f;
            }

            if (difference < closestRangeDifference)
            {
                closestRangeDifference = difference;
                movementDirection = direction;
            }
        }

        if (closestRangeDifference == float.MaxValue)
        {
            StopMoving();
            return;
        }

        float targetDirection = Mathf.Sign(player.position.x - transform.position.x);

        if (targetDirection == 0f)
        {
            StopMoving();
            return;
        }

        float finalDirection = targetDirection * movementDirection;

        FaceDirection(finalDirection);
        rb.velocity = new Vector2(finalDirection * moveSpeed, rb.velocity.y);
    }

    private BossAttackData ChooseAttack()
    {
        if (bossClassData.attacks == null)
            return null;

        List<BossAttackData> available = new List<BossAttackData>();

        foreach (BossAttackData attack in bossClassData.attacks)
        {
            if (attack == null || !IsAttackEnabled(attack.attackType))
                continue;

            if (attackCooldowns.TryGetValue(attack, out float cooldown) && cooldown > 0f)
                continue;

            float distance = GetDistanceToPlayer();

            if (distance < attack.minDistance || distance > attack.maxDistance)
                continue;

            if (IsObstacleBetweenPlayer() && attack.attackType == BossAttackType.Charge)
                continue;

            available.Add(attack);
        }

        if (available.Count == 0)
            return null;

        return available[Random.Range(0, available.Count)];
    }

    private bool IsAttackEnabled(BossAttackType type)
    {
        switch (type)
        {
            case BossAttackType.Charge: return enableCharge;
            case BossAttackType.GroundSlam: return enableGroundSlam;
            case BossAttackType.Shockwave: return enableShockwave;
            case BossAttackType.Uppercut: return enableUppercut;
            case BossAttackType.Fireball: return enableFireball;
            case BossAttackType.Meteor: return enableMeteor;
            case BossAttackType.Arrow: return enableArrow;
            case BossAttackType.MagicBeam: return enableMagicBeam;
            case BossAttackType.Explosion: return enableExplosion;
            case BossAttackType.Pounce: return enablePounce;
            case BossAttackType.TailSweep: return enableTailSweep;
            case BossAttackType.Roar: return enableRoar;
            default: return false;
        }
    }

    private void StartAttack(BossAttackData attack)
    {
        currentAttack = attack;
        state = BossState.Windup;
        stateTimer = attack.windup;
        attackDirection = Mathf.Sign(player.position.x - transform.position.x);

        if (attackDirection == 0f)
            attackDirection = transform.localScale.x >= 0f ? 1f : -1f;

        FaceDirection(attackDirection);
        StopMoving();

        if (warning != null)
            warning.Show(attack, GetWarningOrigin(attack), attackDirection, GetBossHeight());
    }

    private void UpdateWindup()
    {
        StopMoving();

        if (currentAttack == null)
        {
            FinishAttack();
            return;
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
            ExecuteAttack();
    }

    private void ExecuteAttack()
    {
        if (currentAttack == null)
        {
            FinishAttack();
            return;
        }

        state = BossState.Attacking;
        stateTimer = currentAttack.attackType == BossAttackType.Charge || currentAttack.attackType == BossAttackType.Pounce ? currentAttack.chargeDuration : 0.15f;
        chargeHit = false;
        hitPlayers.Clear();

        switch (currentAttack.attackType)
        {
            case BossAttackType.Charge:
                rb.velocity = new Vector2(attackDirection * currentAttack.chargeSpeed, rb.velocity.y);
                break;

            case BossAttackType.Pounce:
                rb.velocity = new Vector2(attackDirection * currentAttack.chargeSpeed, currentAttack.jumpForce);
                break;

            case BossAttackType.Fireball:
            case BossAttackType.Meteor:
            case BossAttackType.Arrow:
                SpawnProjectile();
                break;

            case BossAttackType.GroundSlam:
            case BossAttackType.Shockwave:
            case BossAttackType.Uppercut:
            case BossAttackType.MagicBeam:
            case BossAttackType.Explosion:
            case BossAttackType.TailSweep:
            case BossAttackType.Roar:
                PerformAreaAttack();
                break;
        }

        if (currentAttack.attackType != BossAttackType.Charge && currentAttack.attackType != BossAttackType.Pounce)
            stateTimer = 0.15f;
    }

    private void UpdateAttacking()
    {
        if (currentAttack == null)
        {
            FinishAttack();
            return;
        }

        if (currentAttack.attackType == BossAttackType.Pounce)
            PerformPounceHit();

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            if (currentAttack.attackType == BossAttackType.Charge || currentAttack.attackType == BossAttackType.Pounce)
                StopMoving();

            state = BossState.Recovery;
            stateTimer = currentAttack.recovery;

            if (warning != null)
                warning.Hide();
        }
    }

    private void UpdateRecovery()
    {
        StopMoving();
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
            FinishAttack();
    }

    private void FinishAttack()
    {
        if (currentAttack != null)
        {
            float cooldown = currentAttack.cooldown * phaseCooldownMultiplier;
            attackCooldowns[currentAttack] = cooldown;
        }

        currentAttack = null;
        state = BossState.Idle;

        if (warning != null)
            warning.Hide();

        StopMoving();
    }

    private void StopBoss()
    {
        currentAttack = null;
        state = BossState.Idle;
        stateTimer = 0f;
        chargeHit = false;
        hitPlayers.Clear();

        StopMoving();

        if (warning != null)
            warning.Hide();
    }

    private void PerformChargeHit()
    {
        if (bossCollider == null || chargeHit)
            return;

        chargeContacts.Clear();
        bossCollider.OverlapCollider(chargeContactFilter, chargeContacts);

        foreach (Collider2D hit in chargeContacts)
        {
            if (hit == null)
                continue;

            Player target = hit.GetComponentInParent<Player>();

            if (target == null || target.IsDead() || hitPlayers.Contains(target))
                continue;

            HitPlayer(target, new Vector2(attackDirection, 0.3f));
            chargeHit = true;
            StopMoving();
            break;
        }
    }

    private void PerformPounceHit()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(GetAttackCenter(), GetAttackSize(), 0f);

        foreach (Collider2D hit in hits)
            TryHitPlayer(hit, new Vector2(attackDirection, 0.5f));
    }

    private void PerformAreaAttack()
    {
        if (currentAttack.attackType == BossAttackType.Explosion)
        {
            Collider2D[] explosionHits = Physics2D.OverlapCircleAll(transform.position, currentAttack.warningRadius + attackDetectionPadding);

            foreach (Collider2D hit in explosionHits)
                TryHitPlayer(hit, new Vector2(attackDirection, 0.5f));

            return;
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(GetAttackCenter(), GetAttackSize(), 0f);

        foreach (Collider2D hit in hits)
        {
            Vector2 direction = new Vector2(attackDirection, 0.3f);

            if (currentAttack.attackType == BossAttackType.Uppercut)
                direction = new Vector2(attackDirection * 0.4f, 1f);

            if (currentAttack.attackType == BossAttackType.GroundSlam || currentAttack.attackType == BossAttackType.Shockwave)
                direction = new Vector2(attackDirection, 0.2f);

            TryHitPlayer(hit, direction);
        }
    }

    private Vector2 GetAttackSize()
    {
        Vector2 size = currentAttack.hitboxSize;

        switch (currentAttack.attackType)
        {
            case BossAttackType.Shockwave:
            case BossAttackType.TailSweep:
            case BossAttackType.MagicBeam:
            case BossAttackType.Roar:
            case BossAttackType.GroundSlam:
                size = new Vector2(currentAttack.warningLength, currentAttack.warningWidth);
                break;

            case BossAttackType.Uppercut:
                size = new Vector2(currentAttack.warningWidth, currentAttack.warningHeight);
                break;
        }

        size.x += attackDetectionPadding * 2f;
        size.y += attackDetectionPadding * 2f;

        return size;
    }

    private void TryHitPlayer(Collider2D hit, Vector2 direction)
    {
        if (hit == null)
            return;

        Player target = hit.GetComponentInParent<Player>();

        if (target == null || target.IsDead() || hitPlayers.Contains(target))
            return;

        HitPlayer(target, direction);
    }

    private void HitPlayer(Player target, Vector2 direction)
    {
        if (target == null || target.IsDead() || hitPlayers.Contains(target))
            return;

        hitPlayers.Add(target);

        float damage = enemyAI.GetDamage() * currentAttack.damageMultiplier * phaseDamageMultiplier;

        target.TakeDamage(damage);
        target.ApplyKnockback(direction, currentAttack.knockbackForce);
    }

    private void SpawnProjectile()
    {
        GameObject prefab = currentAttack.attackType == BossAttackType.Arrow
            ? currentAttack.arrowProjectilePrefab
            : currentAttack.magicProjectilePrefab;

        if (prefab == null)
        {
            Debug.LogWarning("[BossAI] Missing projectile prefab for " + currentAttack.attackName, this);
            return;
        }

        Vector2 origin = transform.position;
        Vector2 targetPosition = player.position;

        if (currentAttack.attackType == BossAttackType.Meteor)
            origin = new Vector2(player.position.x, player.position.y + currentAttack.projectileSpawnHeight);
        else
            origin += Vector2.up * currentAttack.projectileSpawnHeight;

        Vector2 direction = (targetPosition - origin).normalized;

        if (currentAttack.attackType == BossAttackType.Meteor)
            direction = Vector2.down;

        GameObject projectileObject = Instantiate(prefab, origin, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Setup(direction, enemyAI.GetDamage() * currentAttack.damageMultiplier * phaseDamageMultiplier, 0f, 1f, currentAttack.knockbackForce, 0f, ElementType.None, StatusEffectType.None, 0f, 0f, 0f, true);
        }
        else
        {
            Rigidbody2D projectileRb = projectileObject.GetComponent<Rigidbody2D>();

            if (projectileRb != null)
                projectileRb.velocity = direction * currentAttack.projectileSpeed;
            else
                Debug.LogWarning("[BossAI] Projectile prefab has neither Projectile nor Rigidbody2D.", projectileObject);
        }
    }

    private Vector2 GetAttackCenter()
    {
        Vector2 offset = currentAttack.hitboxOffset;
        offset.x *= attackDirection;

        Vector2 center = (Vector2)transform.position + offset;

        if (currentAttack.attackType == BossAttackType.Shockwave ||
            currentAttack.attackType == BossAttackType.TailSweep ||
            currentAttack.attackType == BossAttackType.MagicBeam ||
            currentAttack.attackType == BossAttackType.Roar ||
            currentAttack.attackType == BossAttackType.GroundSlam)
        {
            center.y = transform.position.y + groundOffset + GetAttackSize().y / 2f;
        }

        return center;
    }

    private Vector2 GetWarningOrigin(BossAttackData attack)
    {
        float groundY = bossCollider != null ? bossCollider.bounds.min.y : transform.position.y;
        return new Vector2(transform.position.x, groundY + groundOffset);
    }

    private float GetBossHeight()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        bool foundRenderer = false;

        foreach (SpriteRenderer sprite in renderers)
        {
            if (sprite == null || !sprite.enabled)
                continue;

            if (!foundRenderer)
            {
                bounds = sprite.bounds;
                foundRenderer = true;
            }
            else
            {
                bounds.Encapsulate(sprite.bounds);
            }
        }

        if (foundRenderer)
            return bounds.size.y;

        return bossCollider != null ? bossCollider.bounds.size.y : 1f;
    }

    private bool IsObstacleBetweenPlayer()
    {
        if (obstacleLayer.value == 0 || player == null)
            return false;

        Vector2 origin = (Vector2)transform.position + Vector2.up * obstacleCheckOffset;
        Vector2 target = player.position;
        Vector2 direction = (target - origin).normalized;
        float distance = Vector2.Distance(origin, target);

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, obstacleLayer);

        return hit.collider != null;
    }

    private void FaceDirection(float direction)
    {
        if (Mathf.Abs(direction) < 0.01f)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction);
        transform.localScale = scale;
    }

    private void StopMoving()
    {
        if (rb != null)
            rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (state != BossState.Attacking || currentAttack == null ||
            currentAttack.attackType != BossAttackType.Charge)
            return;

        Player target = collision.collider.GetComponentInParent<Player>();

        if (target != null && !target.IsDead() && !hitPlayers.Contains(target))
        {
            HitPlayer(target, new Vector2(attackDirection, 0.3f));
            chargeHit = true;
        }

        StopMoving();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (state != BossState.Attacking || currentAttack == null ||
            currentAttack.attackType != BossAttackType.Charge)
            return;

        Player target = other.GetComponentInParent<Player>();

        if (target == null || target.IsDead() || hitPlayers.Contains(target))
            return;

        HitPlayer(target, new Vector2(attackDirection, 0.3f));
        chargeHit = true;
        StopMoving();
    }

    private void OnDisable()
    {
        StopMoving();

        if (warning != null)
            warning.Hide();
    }

    private void OnDrawGizmosSelected()
    {
        if (roomBounds != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(roomBounds.bounds.center, roomBounds.bounds.size);
        }

        if (currentAttack == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetAttackCenter(), GetAttackSize());
    }
}
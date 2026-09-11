using UnityEngine;

public enum EnemyElement
{
    None,
    Fire,
    Water,
    Earth,
    Wind,
    Lightning,
    Ice,
    Light,
    Dark,
    Poison
}

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
    [SerializeField] private EnemyElement enemyElement;

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

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1f;

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
    private bool isKnockedBack;
    private bool isStunned;
    private Room room;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;
        playerScript = GameObject.FindWithTag("Player").GetComponent<Player>();
        lootManager = FindFirstObjectByType<LootManager>();
        health = maxHealth;
        attackTimer = 0f;
        room = GetComponentInParent<Room>();
        rb.freezeRotation = true;
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null) player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        attackTimer -= Time.deltaTime;
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            StopMoving();
            if (stunTimer <= 0f) isStunned = false;

            return;
        }

        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                rb.velocity = new Vector2(0f,rb.velocity.y);
            }

            return;
        }

        float distance = Vector2.Distance(transform.position,player.position);

        switch (enemyClass)
        {
            case EnemyClass.Melee:
                MeleeAI(distance);
                break;

            case EnemyClass.Ranged:
                RangedAI(distance);
                break;

            case EnemyClass.Tank:
                TankAI(distance);
                break;

            case EnemyClass.Assassin:
                AssassinAI(distance);
                break;

            case EnemyClass.Support:
                SupportAI(distance);
                break;

            case EnemyClass.Boss:
                BossAI(distance);
                break;
        }
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

    private void SupportAI(float distance)
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

    private void BossAI(float distance)
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

    private void MoveTowardsPlayer()
    {
        float direction = Mathf.Sign(player.position.x -transform.position.x);
        rb.velocity = new Vector2(direction * moveSpeed,rb.velocity.y);

        if (direction != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }
    }

    private void StopMoving()
    {
        rb.velocity = new Vector2(0f,rb.velocity.y);
    }

    private void Attack()
    {
        if (attackTimer > 0f)
            return;

        if (playerScript != null) playerScript.TakeDamage(damage);

        attackTimer = attackCooldown;
    }

    public void TakeDamage(float damageAmount,Vector2 hitPoint,float knockback,Vector2 knockbackDirection,float stunChance)
    {
        bool headshot = IsHeadshot(hitPoint);

        if (headshot) damageAmount *= headshotMultiplier;

        health -= damageAmount;
        health = Mathf.Clamp(health,0f,maxHealth);

        if (Random.Range(0f, 100f) <= stunChance) ApplyStun();

        if (knockback > 0f)
        {
            ApplyKnockback(knockbackDirection,knockback);
        }
        if (health <= 0f)
            Die();
    }

    private void ApplyStun()
    {
        isStunned = true;
        stunTimer = stunDuration;
        rb.velocity = Vector2.zero;
    }

    private void ApplyKnockback(Vector2 direction,float force)
    {
        if (rb == null)
            return;

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction.normalized * force,ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(
        Collision2D collision)
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

    public EnemyElement GetElement()
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
        playerScript.GetEXP(100);
        if (lootManager != null && lootTable != null)
        {
            lootManager.GenerateLoot(lootTable,transform.position);
        }
        if (room != null)
            room.EnemyKilled();

        Destroy(gameObject);
    }
}
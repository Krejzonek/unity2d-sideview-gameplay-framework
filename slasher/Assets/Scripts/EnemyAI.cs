using Unity.Mathematics;
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

            if (playerObject != null)player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;
        attackTimer -= Time.deltaTime;
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
        float direction = Mathf.Sign(player.position.x - transform.position.x);
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
        if (attackTimer > 0f)return;
        if (playerScript != null) playerScript.TakeDamage(damage);
        attackTimer = attackCooldown;
    }
    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Clamp(health,0f,maxHealth);
        if (health <= 0f) Die();
        
    }

    public EnemyElement GetElement()
    {
        return enemyElement;
    }

    public EnemyClass GetEnemyClass()
    {
        return enemyClass;
    }
    private void Die()
    {
        playerScript.GetEXP(100);
        if (lootManager != null && lootTable != null)
        {
            lootManager.GenerateLoot(lootTable,transform.position);
        }
        if (room != null)
        {
            room.EnemyKilled();
        }
        Destroy(gameObject);
    }
   /* private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position,detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }*/
}
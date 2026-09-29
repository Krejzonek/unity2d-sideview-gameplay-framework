using System.Collections;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float followDistance = 2.5f;
    [SerializeField] private float detectionRange = 10f;

    [Header("Formation")]
    [SerializeField] private float formationSpacing = 2f;
    [SerializeField] private float formationStopDistance = 0.2f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float rangedAttackRange = 6f;

    private Player player;
    private Stats stats;
    private Rigidbody2D rb;
    private TeamManager teamManager;
    private EnemyAI target;

    private float targetUpdateTimer;
    private int formationPosition;

    private void Awake()
    {
        player = GetComponent<Player>();
        stats = GetComponent<Stats>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        target = null;
        targetUpdateTimer = 0f;
    }

    private void Update()
    {
        if (player == null || player.IsDead())
            return;

        if (TeamManager.Instance == null)
            return;

        teamManager = TeamManager.Instance;

        UpdateTarget();
        UpdateAI();
    }

    private void FixedUpdate()
    {
        if (player == null || player.IsDead())
            return;

        Move();
    }

    private void UpdateAI()
    {
        if (target != null)
        {
            if (CanAttackTarget())
                AttackTarget();

            return;
        }
    }

    private void Move()
    {
        if (target != null)
        {
            MoveToTarget();
            return;
        }

        FollowFormation();
    }

    private void FollowFormation()
    {
        if (TeamManager.Instance == null)
        {
            StopMoving();
            return;
        }

        Player leader = TeamManager.Instance.CurrentPlayer;

        if (leader == null || leader == player || leader.IsDead())
        {
            StopMoving();
            return;
        }

        Vector3 targetPosition = GetFormationTargetPosition(leader);

        float distance = Vector2.Distance(transform.position, targetPosition);

        if (distance <= formationStopDistance)
        {
            StopMoving();
            return;
        }

        MoveTowards(targetPosition);
    }

    private Vector3 GetFormationTargetPosition(Player leader)
    {
        Vector3 position = leader.transform.position;

        if (formationPosition == 0)
            return position;

        float direction = Mathf.Sign(leader.transform.localScale.x);

        if (direction == 0)
            direction = 1f;

        position.x += formationPosition * formationSpacing * direction;

        return position;
    }

    private void MoveToTarget()
    {
        if (target == null)
            return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (IsRangedClass())
        {
            if (distance < rangedAttackRange * 0.6f)
                MoveAwayFrom(target.transform.position);
            else if (distance > rangedAttackRange)
                MoveTowards(target.transform.position);
            else
                StopMoving();
        }
        else
        {
            if (distance > attackRange)
                MoveTowards(target.transform.position);
            else
                StopMoving();
        }
    }

    private void UpdateTarget()
    {
        targetUpdateTimer -= Time.deltaTime;

        if (targetUpdateTimer > 0f)
            return;

        targetUpdateTimer = 0.25f;

        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        EnemyAI closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null)
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

        target = ChooseTarget(closestEnemy);
    }

    private EnemyAI ChooseTarget(EnemyAI closestEnemy)
    {
        return closestEnemy;
    }

    private bool CanAttackTarget()
    {
        if (target == null)
            return false;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (IsRangedClass())
            return distance <= rangedAttackRange;

        return distance <= attackRange;
    }

    private void AttackTarget()
    {
        if (target == null)
            return;

        if (IsRangedClass())
            player.BotRangedAttack(target);
        else
            player.BotMeleeAttack();
    }

    private bool IsRangedClass()
    {
        if (stats == null || stats.characterClass == null)
            return false;

        return stats.characterClass.className == "Archer" || stats.characterClass.className == "Mage";
    }

    private void MoveTowards(Vector3 position)
    {
        float direction = Mathf.Sign(position.x - transform.position.x);

        rb.velocity = new Vector2(direction * stats.speed, rb.velocity.y);

        FaceDirection(direction);
    }

    private void MoveAwayFrom(Vector3 position)
    {
        float direction = Mathf.Sign(transform.position.x - position.x);

        rb.velocity = new Vector2(direction * stats.speed, rb.velocity.y);

        FaceDirection(direction);
    }

    private void StopMoving()
    {
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    private void FaceDirection(float direction)
    {
        if (direction == 0)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    public void SetFormationPosition(int position)
    {
        formationPosition = position;
    }

    public EnemyAI GetTarget()
    {
        return target;
    }

    public Player GetLeader()
    {
        return teamManager != null ? teamManager.CurrentPlayer : null;
    }
}
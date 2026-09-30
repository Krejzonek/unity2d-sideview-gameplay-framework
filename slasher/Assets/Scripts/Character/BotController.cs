using UnityEngine;

public enum BotFormationMode
{
    Idle,
    Combat
}

public class BotController : MonoBehaviour
{
    [Header("Formation")]
    [SerializeField] private float formationSpacing = 2f;
    [SerializeField] private float formationStopDistance = 0.8f;

    [Header("Combat")]
    [SerializeField] private float combatSearchRange = 15f;
    [SerializeField] private float meleeAttackRange = 1.5f;
    [SerializeField] private float rangedAttackRange = 8f;
    [SerializeField] private float retreatDistance = 4f;

    private Player player;
    private Stats stats;
    private Rigidbody2D rb;
    private TeamManager teamManager;

    private int formationPosition;
    private BotFormationMode formationMode = BotFormationMode.Idle;
    private EnemyAI target;

    private float formationDirection = 1f;

    private void Awake()
    {
        player = GetComponent<Player>();
        stats = GetComponent<Stats>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (player == null || player.IsDead())
            return;

        if (TeamManager.Instance == null)
            return;

        teamManager = TeamManager.Instance;

        if (formationMode == BotFormationMode.Idle)
            FollowFormation();
        else
            Combat();
    }

    private void FollowFormation()
    {
        Player leader = teamManager.CurrentPlayer;

        if (leader == null || leader == player)
            return;

        float targetX = leader.transform.position.x + formationPosition * formationSpacing;
        float distance = targetX - transform.position.x;

        if (Mathf.Abs(distance) <= formationStopDistance)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        float moveDirection = Mathf.Sign(distance);

        rb.velocity = new Vector2(moveDirection * teamManager.GetFormationSpeed(), rb.velocity.y);
    }

    private void Combat()
    {
        if (stats == null || stats.characterClass == null)
        {
            RetreatFromEnemy();
            return;
        }

        target = GetClosestEnemy();

        if (target == null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        string className = stats.characterClass.className;

        if (className == "Warrior")
            CombatMelee();
        else if (className == "Archer" || className == "Mage")
            CombatRanged();
        else
            RetreatFromEnemy();
    }

    private void CombatMelee()
    {
        if (target == null)
            return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance > meleeAttackRange)
        {
            MoveTowards(target.transform.position);
            return;
        }

        rb.velocity = new Vector2(0f, rb.velocity.y);
        player.BotMeleeAttack();
    }

    private void CombatRanged()
    {
        if (target == null)
            return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance > rangedAttackRange)
        {
            MoveTowards(target.transform.position);
            return;
        }

        if (distance < retreatDistance)
        {
            MoveAwayFrom(target.transform.position);
            return;
        }

        rb.velocity = new Vector2(0f, rb.velocity.y);
        player.BotRangedAttack(target);
    }

    private void RetreatFromEnemy()
    {
        target = GetClosestEnemy();

        if (target == null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance < retreatDistance)
            MoveAwayFrom(target.transform.position);
        else
            rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    private void MoveTowards(Vector3 position)
    {
        float direction = Mathf.Sign(position.x - transform.position.x);

        rb.velocity = new Vector2(direction * stats.speed, rb.velocity.y);
    }

    private void MoveAwayFrom(Vector3 position)
    {
        float direction = Mathf.Sign(transform.position.x - position.x);

        rb.velocity = new Vector2(direction * stats.speed, rb.velocity.y);
    }

    private EnemyAI GetClosestEnemy()
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        EnemyAI closest = null;
        float closestDistance = combatSearchRange;

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    public void SetFormationPosition(int position)
    {
        formationPosition = position;
    }

    public void SetFormationMode(BotFormationMode mode)
    {
        formationMode = mode;

        if (mode == BotFormationMode.Idle)
            target = null;
    }

    public BotFormationMode GetFormationMode()
    {
        return formationMode;
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
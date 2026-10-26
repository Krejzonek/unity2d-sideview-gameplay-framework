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
    [SerializeField] private float formationStopDistance = 1.6f;

    [Header("Combat")]
    [SerializeField] private float maxCombatDistance = 20f;
    [SerializeField] private float meleeAttackRange = 1.5f;
    [SerializeField] private float rangedAttackRange = 15f;
    [SerializeField] private float rangedPreferredDistance = 7f;
    [SerializeField] private float retreatDistance = 3f;
    [SerializeField] private float attackMissChance = 20f;

    [Header("Low Health")]
    [SerializeField] private float lowHealthPercent = 0.3f;
    [SerializeField] private float lowHealthRetreatDistance = 6f;

    private Player player;
    private Stats stats;
    private ClassSkillManager skillManager;
    private Rigidbody2D rb;
    private TeamManager teamManager;
    private Animator animator;

    private int formationPosition;
    private BotFormationMode formationMode = BotFormationMode.Idle;

    private EnemyAI target;
    private Room combatRoom;
    private bool deathHandled;

    private void Awake()
    {
        player = GetComponent<Player>();
        stats = GetComponent<Stats>();
        skillManager = GetComponent<ClassSkillManager>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        IgnoreTeamCollisions();
        CheckDeath();
    }

    private void FixedUpdate()
    {
        if (player == null || rb == null)
            return;

        if (CheckDeath())
            return;

        if (TeamManager.Instance == null)
            return;

        teamManager = TeamManager.Instance;

        if (formationMode == BotFormationMode.Combat)
        {
            if (combatRoom == null || !RoomHasLivingEnemies())
            {
                formationMode = BotFormationMode.Idle;
                target = null;
            }
        }

        if (formationMode == BotFormationMode.Idle)
        {
            player.BotDefense(false);
            FollowFormation();
            return;
        }

        Combat();
    }

    private bool CheckDeath()
    {
        if (player == null)
            return false;

        if (!player.IsDead())
            return deathHandled;

        HandleDeath();

        if (animator != null)
            animator.enabled = false;

        return true;
    }

    private void HandleDeath()
    {
        if (deathHandled)
            return;

        deathHandled = true;
        target = null;
        formationMode = BotFormationMode.Idle;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        player.BotDefense(false);

        if (animator != null)
            animator.enabled = false;
    }

    private void IgnoreTeamCollisions()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        Collider2D[] myColliders = GetComponents<Collider2D>();

        foreach (Player otherPlayer in players)
        {
            if (otherPlayer == null || otherPlayer == player)
                continue;

            Collider2D[] otherColliders = otherPlayer.GetComponents<Collider2D>();

            foreach (Collider2D myCollider in myColliders)
            {
                foreach (Collider2D otherCollider in otherColliders)
                    Physics2D.IgnoreCollision(myCollider, otherCollider);
            }
        }
    }

    private void FollowFormation()
    {
        if (player == null || player.IsDead())
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (teamManager == null)
            return;

        Player leader = teamManager.CurrentPlayer;

        if (leader == null || leader == player)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        float targetX = leader.transform.position.x + formationPosition * formationSpacing;
        float distance = targetX - transform.position.x;
        float speed = stats != null ? stats.speed : 5f;

        if (Mathf.Abs(distance) < 0.2f)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        float targetSpeed = Mathf.Clamp(distance * 5f, -speed, speed);

        UpdateFacingDirection(targetSpeed);
        rb.velocity = new Vector2(targetSpeed, rb.velocity.y);
    }

    private void Combat()
    {
        if (stats == null || stats.characterClass == null)
        {
            player.BotDefense(false);
            FollowFormation();
            return;
        }

        if (combatRoom == null || !RoomHasLivingEnemies())
        {
            formationMode = BotFormationMode.Idle;
            target = null;
            player.BotDefense(false);
            FollowFormation();
            return;
        }

        if (target == null || !IsValidTarget(target))
        {
            target = FindNearestTarget();

            if (target == null)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                player.BotDefense(false);
                return;
            }
        }

        if (HandleLowStamina())
            return;

        UseActiveSkill();

        string className = stats.characterClass.className;

        if (className == "Warrior")
            CombatWarrior();
        else if (className == "Archer")
            CombatArcher();
        else if (className == "Mage")
            CombatMage();
        else
            FollowFormation();
    }

    private bool RoomHasLivingEnemies()
    {
        if (combatRoom == null)
            return false;

        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.IsDead())
                continue;

            Room enemyRoom = enemy.GetComponentInParent<Room>();

            if (enemyRoom == combatRoom)
                return true;
        }

        return false;
    }

    private EnemyAI FindNearestTarget()
    {
        if (combatRoom == null)
            return null;

        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        EnemyAI nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (EnemyAI enemy in enemies)
        {
            if (!IsValidTarget(enemy))
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    private bool IsValidTarget(EnemyAI enemy)
    {
        if (enemy == null || !enemy.gameObject.activeInHierarchy)
            return false;

        if (combatRoom == null)
            return false;

        if (enemy.IsDead() || enemy.IsSpawning())
            return false;

        Room enemyRoom = enemy.GetComponentInParent<Room>();

        if (enemyRoom == null || enemyRoom != combatRoom)
            return false;

        return true;
    }

    private bool HandleLowStamina()
    {
        float attackStaminaCost = player.GetAttackStaminaCost();

        if (player.IsDefending())
        {
            if (player.GetStamina() >= attackStaminaCost)
            {
                player.BotDefense(false);
                return false;
            }

            rb.velocity = new Vector2(0f, rb.velocity.y);
            return true;
        }

        if (player.GetStamina() < attackStaminaCost)
        {
            player.BotDefense(true);
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return true;
        }

        return false;
    }

    private void CombatWarrior()
    {
        if (target == null)
            return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance > meleeAttackRange)
        {
            MoveTowardsCombatTarget(target.transform.position);
            return;
        }

        rb.velocity = new Vector2(0f, rb.velocity.y);
        UpdateFacingDirection(target.transform.position.x - transform.position.x);
        player.BotMeleeAttack();
    }

    private void CombatArcher()
    {
        if (target == null)
            return;

        float retreatRange = IsLowHealth() ? lowHealthRetreatDistance : retreatDistance;
        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance < retreatRange)
        {
            MoveAwayFrom(target.transform.position);
            return;
        }

        if (distance > rangedPreferredDistance)
        {
            MoveTowardsCombatTarget(target.transform.position);
            return;
        }

        rb.velocity = new Vector2(0f, rb.velocity.y);
        UpdateFacingDirection(target.transform.position.x - transform.position.x);

        bool shouldMiss = Random.Range(0f, 100f) < attackMissChance;
        player.BotRangedAttack(target, shouldMiss);
    }

    private void CombatMage()
    {
        if (target == null)
            return;

        float retreatRange = IsLowHealth() ? lowHealthRetreatDistance : retreatDistance;
        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance < retreatRange)
        {
            MoveAwayFrom(target.transform.position);
            return;
        }

        if (distance > rangedPreferredDistance)
        {
            MoveTowardsCombatTarget(target.transform.position);
            return;
        }

        rb.velocity = new Vector2(0f, rb.velocity.y);
        UpdateFacingDirection(target.transform.position.x - transform.position.x);

        bool shouldMiss = Random.Range(0f, 100f) < attackMissChance;
        player.BotRangedAttack(target, shouldMiss);
    }

    private void MoveTowardsCombatTarget(Vector3 position)
    {
        float distance = Vector2.Distance(transform.position, position);

        if (distance > maxCombatDistance)
        {
            FollowFormation();
            return;
        }

        float speed = stats != null ? stats.speed : 5f;
        float direction = Mathf.Sign(position.x - transform.position.x);

        UpdateFacingDirection(direction);
        rb.velocity = new Vector2(direction * speed, rb.velocity.y);
    }

    private void MoveAwayFrom(Vector3 position)
    {
        float speed = stats != null ? stats.speed : 5f;
        float direction = Mathf.Sign(transform.position.x - position.x);

        UpdateFacingDirection(direction);
        rb.velocity = new Vector2(direction * speed, rb.velocity.y);
    }

    private void UpdateFacingDirection(float direction)
    {
        if (player == null || Mathf.Abs(direction) < 0.01f)
            return;

        player.SetBotFacingDirection(direction > 0f ? 1 : -1);
    }

    private void UseActiveSkill()
    {
        if (skillManager == null)
            return;

        if (!skillManager.IsActiveSkillReady())
            return;

        string className = stats.characterClass.className;

        if (className == "Warrior")
        {
            if (target != null && Vector2.Distance(transform.position, target.transform.position) <= meleeAttackRange)
                skillManager.UseActiveSkill();
        }
        else if (className == "Archer" || className == "Mage")
        {
            if (target != null && Vector2.Distance(transform.position, target.transform.position) <= rangedAttackRange)
                skillManager.UseActiveSkill();
        }
    }

    private bool IsLowHealth()
    {
        return player.GetHealthPercent() <= lowHealthPercent;
    }

    public void SetCombatRoom(Room room)
    {
        if (deathHandled)
            return;

        combatRoom = room;
        formationMode = BotFormationMode.Combat;
        target = null;
        player.BotDefense(false);
    }

    public void SetFormationPosition(int position)
    {
        formationPosition = position;
    }

    public void SetFormationMode(BotFormationMode mode)
    {
        if (deathHandled)
            return;

        formationMode = mode;

        if (mode == BotFormationMode.Idle)
        {
            target = null;
            player.BotDefense(false);
        }
    }
}
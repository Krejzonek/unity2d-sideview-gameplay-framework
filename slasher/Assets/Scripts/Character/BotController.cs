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
    [SerializeField] private float maxFormationDistance = 5f;

    [Header("Combat")]
    [SerializeField] private float combatSearchRange = 100f;
    [SerializeField] private float maxCombatDistance = 20f;
    [SerializeField] private float meleeAttackRange = 1.5f;
    [SerializeField] private float rangedAttackRange = 15f;
    [SerializeField] private float rangedPreferredDistance = 7f;
    [SerializeField] private float retreatDistance = 4f;
    [SerializeField] private float attackMissChance = 20f;
    [SerializeField] private float targetChangeCooldown = 0.75f;

    [Header("Low Health")]
    [SerializeField] private float lowHealthPercent = 0.3f;
    [SerializeField] private float lowHealthRetreatDistance = 6f;

    private Player player;
    private Stats stats;
    private ClassSkillManager skillManager;
    private Rigidbody2D rb;
    private TeamManager teamManager;

    private int formationPosition;
    private BotFormationMode formationMode = BotFormationMode.Idle;

    private EnemyAI target;
    private float targetChangeTimer;

    private void Awake()
    {
        player = GetComponent<Player>();
        stats = GetComponent<Stats>();
        skillManager = GetComponent<ClassSkillManager>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (player == null || player.IsDead())
            return;

        if (TeamManager.Instance == null)
            return;

        teamManager = TeamManager.Instance;

        if (targetChangeTimer > 0f)
            targetChangeTimer -= Time.fixedDeltaTime;

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
            ReturnToFormation();
            return;
        }

        Player leader = teamManager.CurrentPlayer;

        if (leader == null)
        {
            ReturnToFormation();
            return;
        }

        if (Vector2.Distance(transform.position, leader.transform.position) > maxFormationDistance + maxCombatDistance)
        {
            target = null;
            ReturnToFormation();
            return;
        }

        if (target == null || !IsValidTarget(target) || targetChangeTimer <= 0f)
        {
            EnemyAI newTarget = FindBestTarget();

            if (newTarget != null)
            {
                target = newTarget;
                targetChangeTimer = targetChangeCooldown;
            }
            else if (target == null)
            {
                ReturnToFormation();
                return;
            }
        }

        if (target == null)
        {
            ReturnToFormation();
            return;
        }

        if (ShouldProtectTeam())
        {
            EnemyAI threat = FindThreatToTeam();

            if (threat != null)
                target = threat;
        }

        UseActiveSkill();

        string className = stats.characterClass.className;

        if (className == "Warrior")
            CombatWarrior();
        else if (className == "Archer")
            CombatArcher();
        else if (className == "Mage")
            CombatMage();
        else
            ReturnToFormation();
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
        player.BotMeleeAttack();
    }

    private void CombatArcher()
    {
        if (target == null)
            return;

        float retreatRange = IsLowHealth() ? lowHealthRetreatDistance : retreatDistance;
        EnemyAI threat = FindClosestEnemyInRange(retreatRange);

        if (threat != null)
        {
            MoveAwayFrom(threat.transform.position);
            return;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance > rangedPreferredDistance)
        {
            MoveTowardsCombatTarget(target.transform.position);
            return;
        }

        rb.velocity = new Vector2(0f, rb.velocity.y);

        bool shouldMiss = Random.Range(0f, 100f) < attackMissChance;
        player.BotRangedAttack(target, shouldMiss);
    }

    private void CombatMage()
    {
        if (target == null)
            return;

        float retreatRange = IsLowHealth() ? lowHealthRetreatDistance : retreatDistance;
        EnemyAI threat = FindClosestEnemyInRange(retreatRange);

        if (threat != null)
        {
            MoveAwayFrom(threat.transform.position);
            return;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance > rangedPreferredDistance)
        {
            MoveTowardsCombatTarget(target.transform.position);
            return;
        }

        rb.velocity = new Vector2(0f, rb.velocity.y);

        bool shouldMiss = Random.Range(0f, 100f) < attackMissChance;
        player.BotRangedAttack(target, shouldMiss);
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

    private EnemyAI FindBestTarget()
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        EnemyAI bestTarget = null;
        float bestScore = float.MinValue;

        foreach (EnemyAI enemy in enemies)
        {
            if (!IsValidTarget(enemy))
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance > combatSearchRange)
                continue;

            float score = GetTargetScore(enemy, distance);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    private float GetTargetScore(EnemyAI enemy, float distance)
    {
        float score = 100f - distance * 5f;

        EnemyClass enemyClass = enemy.GetEnemyClass();
        string className = stats.characterClass.className;

        if (enemyClass == EnemyClass.Support)
            score += 50f;

        if (enemyClass == EnemyClass.Ranged)
            score += 25f;

        if (enemyClass == EnemyClass.Assassin)
            score += 35f;

        if (IsAttackingLeader(enemy))
            score += 70f;

        if (IsAttackingTeammate(enemy))
            score += 60f;

        if (className == "Warrior")
        {
            if (enemyClass == EnemyClass.Melee || enemyClass == EnemyClass.Tank)
                score += 20f;
        }

        if (className == "Archer" || className == "Mage")
        {
            if (enemyClass == EnemyClass.Support)
                score += 30f;

            if (enemyClass == EnemyClass.Ranged)
                score += 20f;

            if (enemyClass == EnemyClass.Assassin)
                score += 25f;
        }

        return score;
    }

    private bool ShouldProtectTeam()
    {
        string className = stats.characterClass.className;

        return className == "Warrior";
    }

    private EnemyAI FindThreatToTeam()
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        EnemyAI closestThreat = null;
        float closestDistance = Mathf.Infinity;

        foreach (EnemyAI enemy in enemies)
        {
            if (!IsValidTarget(enemy))
                continue;

            if (!IsAttackingTeammate(enemy) && !IsAttackingLeader(enemy))
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance > maxCombatDistance)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestThreat = enemy;
            }
        }

        return closestThreat;
    }

    private EnemyAI FindClosestEnemyInRange(float range)
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        EnemyAI closestEnemy = null;
        float closestDistance = range;

        foreach (EnemyAI enemy in enemies)
        {
            if (!IsValidTarget(enemy))
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    private bool IsAttackingLeader(EnemyAI enemy)
    {
        Player leader = teamManager.CurrentPlayer;

        if (leader == null)
            return false;

        return enemy.GetPlayerScript() == leader;
    }

    private bool IsAttackingTeammate(EnemyAI enemy)
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (Player teammate in players)
        {
            if (teammate == null || teammate == player || teammate.IsDead())
                continue;

            if (enemy.GetPlayerScript() == teammate)
                return true;
        }

        return false;
    }

    private bool IsValidTarget(EnemyAI enemy)
    {
        if (enemy == null || !enemy.gameObject.activeInHierarchy)
            return false;

        Room currentRoom = GetComponentInParent<Room>();
        Room enemyRoom = enemy.GetComponentInParent<Room>();

        if (currentRoom != null && enemyRoom != currentRoom)
            return false;

        return true;
    }

    private void MoveTowardsCombatTarget(Vector3 position)
    {
        Player leader = teamManager.CurrentPlayer;

        if (leader == null)
            return;

        float leaderDistance = Vector2.Distance(transform.position, leader.transform.position);

        if (leaderDistance > maxFormationDistance + maxCombatDistance)
        {
            ReturnToFormation();
            return;
        }

        float direction = Mathf.Sign(position.x - transform.position.x);

        rb.velocity = new Vector2(direction * stats.speed, rb.velocity.y);
    }

    private void MoveAwayFrom(Vector3 position)
    {
        float direction = Mathf.Sign(transform.position.x - position.x);

        rb.velocity = new Vector2(direction * stats.speed, rb.velocity.y);
    }

    private void ReturnToFormation()
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

        float direction = Mathf.Sign(distance);

        rb.velocity = new Vector2(direction * teamManager.GetFormationSpeed(), rb.velocity.y);
    }

    private bool IsLowHealth()
    {
        return player.GetHealthPercent() <= lowHealthPercent;
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
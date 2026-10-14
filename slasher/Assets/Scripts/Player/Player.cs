using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float health;
    [SerializeField] private float stamina;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce = 10f;
    public int lvl = 1;
    public int currentEXP;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float defenseDamageReduction = 0.5f;

    [Header("I-Frames")]
    [SerializeField] private float invulnerabilityTime = 0.3f;

    [Header("Projectile")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject magicPrefab;

    [Header("Floor")]
    [SerializeField] private Transform floorCheck;
    [SerializeField] private float floorCheckRadius = 0.1f;
    [SerializeField] private LayerMask floorLayer;

    [Header("Crouch")]
    [SerializeField] private CapsuleCollider2D playerCollider;

    private Vector2 normalColliderSize;
    private Vector2 normalColliderOffset;

    private Rigidbody2D rb;
    private float moveInput;
    private float formationSpeed = -1f;
    private Stats stats;
    private ClassSkillManager skillManager;
    private float nextAttackTime;
    private float invulnerabilityTimer;
    private bool isDefending;
    private float knockbackTimer;

    private bool isBot;
    private TeamManager teamManager;
    private CharacterData characterData;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<Stats>();
        skillManager = GetComponent<ClassSkillManager>();

        normalColliderSize = playerCollider.size;
        normalColliderOffset = playerCollider.offset;

        rb.freezeRotation = true;

        moveSpeed = stats.speed;
        health = stats.health;
        stamina = stats.stamina;

        if (CharacterManager.Instance != null && characterData != null)
        {
            int index = CharacterManager.Instance.Characters.IndexOf(characterData);

            if (index >= 0 && CharacterManager.Instance.IsCharacterDead(index))
                SetDead();
        }
    }

    private void Update()
    {
        if (IsDead())
            return;

        if (invulnerabilityTimer > 0f)
            invulnerabilityTimer -= Time.deltaTime;

        RegenerateStamina();

        if (isBot)
            return;

        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && OnFloor() && !IsCrouching() && stamina > 25f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            UseStamina(25);
        }

        if (Input.GetKey(KeyCode.LeftShift))
            Crouch();
        else
            StundUp();

        if (Input.GetMouseButton(0))
            Attack();

        if (Input.GetMouseButton(1))
            StartDefense();
        else
            StopDefense();

        LvlUp();
    }

    private void FixedUpdate()
    {
        if (IsDead())
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        if (isBot)
            return;

        float currentMoveSpeed = formationSpeed > 0f ? formationSpeed : moveSpeed;

        rb.velocity = new Vector2(moveInput * currentMoveSpeed, rb.velocity.y);
    }

    public void SetFormationSpeed(float speed)
    {
        formationSpeed = speed;
    }

    public void ClearFormationSpeed()
    {
        formationSpeed = -1f;
    }

    public void SetTeamManager(TeamManager manager)
    {
        teamManager = manager;
    }

    public void SetBotMode(bool value)
    {
        isBot = value;

        BotController botController = GetComponent<BotController>();

        if (botController != null)
            botController.enabled = value;
    }

    public bool IsBot()
    {
        return isBot;
    }

    public CharacterData GetCharacterData()
    {
        return characterData;
    }

    public void SetCharacterData(CharacterData data)
    {
        characterData = data;
    }

    public void InitializeCharacter(CharacterData data)
    {
        characterData = data;

        if (CharacterManager.Instance == null || data == null)
            return;

        CharacterManager.Instance.ApplyCharacterToPlayer(data, this, stats);
    }

    public void SaveCharacterData()
    {
        if (characterData == null || CharacterManager.Instance == null || stats == null)
            return;

        CharacterManager.Instance.UpdateCharacterFromPlayer(characterData, this, stats);
    }

    public void RefreshStats()
    {
        if (stats == null)
            return;

        stats.UpdateStats();

        if (health > stats.health)
            health = stats.health;

        if (stamina > stats.stamina)
            stamina = stats.stamina;

        moveSpeed = stats.speed;
    }

    public void GetEXP(int exp)
    {
        currentEXP += Mathf.RoundToInt(exp * stats.expBonus);
        LvlUp();

        if (characterData != null)
            SaveCharacterData();
    }

    private void LvlUp()
    {
        while (currentEXP >= lvl * 100)
        {
            currentEXP -= lvl * 100;
            lvl += 1;
            stats.freePoint += 1;
        }

        SaveCharacterData();
    }

    private bool OnFloor()
    {
        if (floorCheck == null)
            return false;

        return Physics2D.OverlapCircle(floorCheck.position, floorCheckRadius, floorLayer) != null;
    }

    private void Crouch()
    {
        if (!IsCrouching())
        {
            float oldHeight = playerCollider.size.y;
            float newHeight = oldHeight / 2f;

            moveSpeed /= 2;

            playerCollider.size = new Vector2(playerCollider.size.x, newHeight);
            playerCollider.offset = new Vector2(playerCollider.offset.x, normalColliderOffset.y - (oldHeight - newHeight) / 2f);
        }
    }

    private bool IsCrouching()
    {
        return playerCollider.size.y < normalColliderSize.y;
    }

    private void StundUp()
    {
        if (IsCrouching())
        {
            playerCollider.size = normalColliderSize;
            moveSpeed *= 2;
            playerCollider.offset = normalColliderOffset;
        }
    }

    private void StartDefense()
    {
        if (isDefending)
            return;

        isDefending = true;
    }

    private void StopDefense()
    {
        isDefending = false;
    }

    private void Attack()
    {
        if (isDefending)
            return;

        if (Time.time < nextAttackTime)
            return;

        ItemData weapon = stats.GetWeapon();
        float staminaCost = 5f;

        if (weapon != null)
        {
            staminaCost = GetAttackStaminaCost();
        }

        if (skillManager != null)
            staminaCost *= skillManager.GetStaminaCostMultiplier();

        if (stamina < staminaCost)
            return;

        UseStamina(staminaCost);

        if (skillManager != null)
            skillManager.RegisterAttack();

        if (weapon == null)
            MeleeAttack();
        else
        {
            switch (weapon.weaponType)
            {
                case WeaponType.Sword: MeleeAttack(); break;
                case WeaponType.Bow: ShootProjectile(arrowPrefab); break;
                case WeaponType.Staff: ShootProjectile(magicPrefab); break;
                default: MeleeAttack(); break;
            }
        }

        SetAttackCooldown();
    }

    private void SetAttackCooldown()
    {
        float speedMultiplier = skillManager != null ? skillManager.GetAttackSpeedMultiplier() : 1f;
        float currentCooldown = Mathf.Max(attackCooldown / (1f + stats.speed * 0.1f) / speedMultiplier, 0.1f);

        nextAttackTime = Time.time + currentCooldown;
    }

    private void MeleeAttack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange);

        GetWeaponStatus(out StatusEffectType statusEffect, out float statusChance, out float statusDuration, out float statusDamage);

        foreach (Collider2D enemy in enemies)
        {
            EnemyAI enemyAI = enemy.GetComponentInParent<EnemyAI>();

            if (enemyAI != null)
            {
                Vector2 hitPoint = enemy.ClosestPoint(transform.position);
                bool isCrit = Random.Range(0f, 100f) <= stats.critRate;
                float damage = stats.attack;
                float distance = Vector2.Distance(transform.position, enemy.transform.position);

                if (isCrit)
                    damage *= stats.critDamage;

                if (skillManager != null)
                {
                    damage *= skillManager.GetDamageMultiplier(distance);
                    skillManager.RegisterHit();
                }

                float elementMultiplier = ElementSystem.GetDamageMultiplier(GetAttackElement(), enemyAI.GetElement());
                damage *= elementMultiplier;
                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;

                enemyAI.TakeDamage(damage, hitPoint, stats.knockback * (skillManager != null ? skillManager.GetKnockbackMultiplier() : 1f), knockbackDirection, stats.stunChance, isCrit, statusEffect, statusChance, statusDuration, statusDamage, this);
            }
        }

        if (skillManager != null)
            skillManager.ConsumeAttackSkill();
    }

    private void ShootProjectile(GameObject projectilePrefab)
    {
        if (projectilePrefab == null || attackPoint == null)
            return;

        Camera cam = Camera.main;

        if (cam == null)
            return;

        ItemData weapon = stats.GetWeapon();

        if (weapon == null)
            return;

        Vector3 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - attackPoint.position).normalized;

        bool doubleShot = false;

        if (skillManager != null && weapon.weaponType == WeaponType.Bow)
            doubleShot = skillManager.ConsumeDoubleShot();

        if (doubleShot)
        {
            ShootSingleProjectile(projectilePrefab, direction);
            ShootSingleProjectile(projectilePrefab, Quaternion.Euler(0f, 0f, 10f) * direction);
        }
        else
        {
            ShootSingleProjectile(projectilePrefab, direction);
        }

        if (skillManager != null)
            skillManager.ConsumeAttackSkill();
    }

    private void ShootSingleProjectile(GameObject projectilePrefab, Vector2 direction)
    {
        GameObject projectileObject = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            ItemData weapon = stats.GetWeapon();

            if (weapon == null)
                return;

            StatusEffectType statusEffect = weapon.GetStatusEffect();
            float statusChance = weapon.GetStatusChance();
            float statusDuration = weapon.GetStatusDuration();
            float statusDamage = weapon.GetStatusDamage(stats.attack);

            float damageMultiplier = skillManager != null ? skillManager.GetDamageMultiplier(Vector2.Distance(transform.position, attackPoint.position)) : 1f;
            float damage = stats.attack * damageMultiplier;

            projectile.Setup(direction, damage, stats.critRate, stats.critDamage, stats.knockback * (skillManager != null ? skillManager.GetKnockbackMultiplier() : 1f), stats.stunChance, GetAttackElement(), statusEffect, statusChance, statusDuration, statusDamage, false, this);
        }
    }

    private ElementType GetAttackElement()
    {
        ItemData weapon = stats.GetWeapon();

        if (weapon == null)
            return ElementType.None;

        return weapon.element;
    }

    public void TakeDamage(float damage)
    {
        if (invulnerabilityTimer > 0f || IsDead())
            return;

        invulnerabilityTimer = invulnerabilityTime;

        damage *= 100f / (100f + stats.defence);

        if (skillManager != null)
            damage *= 1f - skillManager.GetDamageReduction();

        if (isDefending)
            damage *= 1f - defenseDamageReduction;

        health -= damage;
        health = Mathf.Clamp(health, 0f, stats.health);

        if (health <= 0f)
        {
            health = 0f;
            isDefending = false;
            rb.velocity = Vector2.zero;

            if (teamManager != null)
                teamManager.PlayerDied(this);
            else if (TeamManager.Instance != null)
                TeamManager.Instance.PlayerDied(this);
        }
    }

    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0f, stats.health);
    }

    private void UseStamina(float amount)
    {
        stamina -= amount;
        stamina = Mathf.Clamp(stamina, 0f, stats.stamina);
    }

    private void GetWeaponStatus(out StatusEffectType statusEffect, out float statusChance, out float statusDuration, out float statusDamage)
    {
        statusEffect = StatusEffectType.None;
        statusChance = 0f;
        statusDuration = 0f;
        statusDamage = 0f;

        ItemData weapon = stats.GetWeapon();

        if (weapon == null)
            return;

        statusEffect = weapon.GetStatusEffect();
        statusChance = weapon.GetStatusChance();
        statusDuration = weapon.GetStatusDuration();
        statusDamage = weapon.GetStatusDamage(stats.attack);
    }

    private void RegenerateStamina()
    {
        if (!OnFloor())
            return;

        float regeneration = IsCrouching() ? 0.15f : 0.1f;

        stamina += stats.stamina * regeneration * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0f, stats.stamina);
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (rb == null || IsDead())
            return;

        knockbackTimer = 0.3f;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    public void UseClassSkill()
    {
        if (skillManager != null)
            skillManager.UseActiveSkill();
    }

    public float GetHealthPercent()
    {
        return stats.health > 0f ? health / stats.health : 0f;
    }

    public float GetHealth()
    {
        return health;
    }

    public float GetStamina()
    {
        return stamina;
    }

    public float GetMaxHealth()
    {
        return stats != null ? stats.health : 0f;
    }

    public Stats GetStats()
    {
        return stats;
    }

    public bool IsDead()
    {
        return health <= 0f;
    }

    public bool IsDefending()
    {
        return isDefending;
    }

    public void SetDead()
    {
        health = 0f;
        isDefending = false;
        knockbackTimer = 0f;

        if (rb != null)
            rb.velocity = Vector2.zero;

        BotController botController = GetComponent<BotController>();

        if (botController != null)
            botController.enabled = false;
    }


    public bool LoseLevelForResurrection()
    {
        if (IsDead() || lvl <= 1)
            return false;

        lvl--;
        currentEXP = Mathf.Min(currentEXP, lvl * 100 - 1);

        if (stats != null)
            stats.UpdateStats();

        SaveCharacterData();
        return true;
    }
    public void Respawn(Vector3 position)
    {
        transform.position = position;

        health = stats.health;
        stamina = stats.stamina;

        invulnerabilityTimer = 2f;
        isDefending = false;
        knockbackTimer = 0f;

        if (rb != null)
            rb.velocity = Vector2.zero;

        BotController botController = GetComponent<BotController>();

        if (botController != null)
            botController.enabled = isBot;
    }

    public void BotMeleeAttack()
    {
        if (isDefending || IsDead() || Time.time < nextAttackTime)
            return;

        float staminaCost = GetAttackStaminaCost();

        if (stamina < staminaCost)
            return;

        UseStamina(staminaCost);

        if (skillManager != null)
            skillManager.RegisterAttack();

        MeleeAttack();
        SetAttackCooldown();
    }

    public void BotRangedAttack(EnemyAI enemy, bool shouldMiss = false)
    {
        if (enemy == null || isDefending || IsDead() || Time.time < nextAttackTime)
            return;

        if (attackPoint == null)
            return;

        ItemData weapon = stats.GetWeapon();

        if (weapon == null)
            return;

        GameObject projectilePrefab = null;

        switch (weapon.weaponType)
        {
            case WeaponType.Bow:
                projectilePrefab = arrowPrefab;
                break;

            case WeaponType.Staff:
                projectilePrefab = magicPrefab;
                break;

            default:
                return;
        }

        if (projectilePrefab == null)
            return;

        float staminaCost = weapon.weaponType == WeaponType.Staff ? 50f : 25f;

        if (skillManager != null)
            staminaCost *= skillManager.GetStaminaCostMultiplier();

        if (stamina < staminaCost)
            return;

        UseStamina(staminaCost);

        if (skillManager != null)
            skillManager.RegisterAttack();

        Vector2 direction = (enemy.transform.position - attackPoint.position).normalized;

        if (shouldMiss)
        {
            float missAngle = Random.Range(20f, 45f);

            if (Random.value < 0.5f)
                missAngle *= -1f;

            direction = Quaternion.Euler(0f, 0f, missAngle) * direction;
        }

        bool doubleShot = false;

        if (skillManager != null && weapon.weaponType == WeaponType.Bow)
            doubleShot = skillManager.ConsumeDoubleShot();

        if (doubleShot)
        {
            ShootSingleProjectile(projectilePrefab, direction);
            ShootSingleProjectile(projectilePrefab, Quaternion.Euler(0f, 0f, 10f) * direction);
        }
        else
        {
            ShootSingleProjectile(projectilePrefab, direction);
        }

        if (skillManager != null)
            skillManager.ConsumeAttackSkill();

        SetAttackCooldown();
    }

    public void BotUseClassSkill()
    {
        if (!IsDead() && skillManager != null)
            skillManager.UseActiveSkill();
    }

    public void BotDefense(bool value)
    {
        if (value)
            StartDefense();
        else
            StopDefense();
    }
    public float GetAttackStaminaCost()
    {
        ItemData weapon = stats.GetWeapon();
        float staminaCost = 5f;

        if (weapon != null)
        {
            switch (weapon.weaponType)
            {
                case WeaponType.Sword:
                    staminaCost = 10f;
                    break;
                case WeaponType.Bow:
                    staminaCost = 25f;
                    break;
                case WeaponType.Staff:
                    staminaCost = 50f;
                    break;
                default:
                    staminaCost = 5f;
                    break;
            }
        }

        if (skillManager != null)
            staminaCost *= skillManager.GetStaminaCostMultiplier();

        return staminaCost;
    }
}
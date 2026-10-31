using System.Collections.Generic;
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

    [Header("Panel Attack Blocking")]
    public bool blockAttackWhenPanelOpen = true;

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

    [Header("Hand Sorting")]
    [SerializeField] private Transform frontHand;
    [SerializeField] private Transform backHand;
    [SerializeField] private int frontHandSortingOrder = 3;
    [SerializeField] private int backHandSortingOrder = -1;

    private readonly Dictionary<SpriteRenderer, int> weaponBaseSortingOrders = new Dictionary<SpriteRenderer, int>();
    private readonly Dictionary<SpriteRenderer, int> handBaseSortingOrders = new Dictionary<SpriteRenderer, int>();

    private Vector2 normalColliderSize;
    private Vector2 normalColliderOffset;

    private Rigidbody2D rb;
    private GameInputManager inputManager;
    private float moveInput;
    private float formationSpeed = -1f;
    private Stats stats;
    private ClassSkillManager skillManager;
    private float nextAttackTime;
    private float invulnerabilityTimer;
    private bool isDefending;
    private float knockbackTimer;
    private bool isTurningToShoot;
    private int facingDirection = 1;
    private MeleeWeaponHitbox meleeWeaponHitbox;
    private bool isBot;
    private TeamManager teamManager;
    private CharacterData characterData;
    private bool waitForAttackRelease;
    private bool isCrouching;
    private static bool blockAttackUntilMouseRelease;

    private void OnEnable()
    {
        ConnectInputManager();
    }

    private void OnDisable()
    {
        DisconnectInputManager();
    }

    private void ConnectInputManager()
    {
        if (inputManager != null)
            return;

        inputManager = GameInputManager.Instance;

        if (inputManager != null)
            inputManager.ActionStarted += OnActionStarted;
    }

    private void DisconnectInputManager()
    {
        if (inputManager == null)
            return;

        inputManager.ActionStarted -= OnActionStarted;
        inputManager = null;
    }

    private void OnActionStarted(GameInputAction action)
    {
        if (IsDead() || isBot)
            return;

        if (action == GameInputAction.Jump && rb != null && OnFloor() && !IsCrouching() && stamina > 25f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            UseStamina(25f);
        }

        if (action == GameInputAction.Skill1 && skillManager != null)
            skillManager.UseActiveSkill();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<Stats>();
        skillManager = GetComponent<ClassSkillManager>();
        ConnectInputManager();

        normalColliderSize = playerCollider.size;
        normalColliderOffset = playerCollider.offset;

        rb.freezeRotation = true;

        moveSpeed = stats.speed;
        health = stats.health;
        stamina = stats.stamina;

        facingDirection = transform.localScale.x < 0f ? -1 : 1;
        CacheHandSorting();
        UpdateHandSorting();

        if (CharacterManager.Instance != null && characterData != null)
        {
            int index = CharacterManager.Instance.Characters.IndexOf(characterData);

            if (index >= 0 && CharacterManager.Instance.IsCharacterDead(index))
                SetDead();
        }
    }

    private void Awake()
    {
        meleeWeaponHitbox = GetComponentInChildren<MeleeWeaponHitbox>(true);

        if (meleeWeaponHitbox != null)
            meleeWeaponHitbox.Initialize(this);
    }

    private void CacheHandSorting()
    {
        weaponBaseSortingOrders.Clear();
        handBaseSortingOrders.Clear();

        CacheHandSorting(frontHand);
        CacheHandSorting(backHand);
    }

    private void CacheHandSorting(Transform hand)
    {
        if (hand == null)
            return;

        Transform weaponHolder = hand.Find("WeaponHolder");

        foreach (SpriteRenderer renderer in hand.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (weaponHolder != null && (renderer.transform == weaponHolder || renderer.transform.IsChildOf(weaponHolder)))
            {
                if (!weaponBaseSortingOrders.ContainsKey(renderer))
                    weaponBaseSortingOrders.Add(renderer, renderer.sortingOrder);
            }
            else if (!handBaseSortingOrders.ContainsKey(renderer))
            {
                handBaseSortingOrders.Add(renderer, renderer.sortingOrder);
            }
        }
    }

    private bool IsAnyPanelOpen()
    {
        if (!blockAttackWhenPanelOpen || TeamManager.Instance == null || TeamManager.Instance.CurrentPlayer != this)
            return false;

        PanelAttackBlocker[] blockers = FindObjectsByType<PanelAttackBlocker>(FindObjectsSortMode.None);

        foreach (PanelAttackBlocker blocker in blockers)
        {
            if (blocker != null && blocker.gameObject.activeInHierarchy)
                return true;
        }

        return false;
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

        if (inputManager == null)
            ConnectInputManager();

        if (inputManager == null)
            return;

        moveInput = inputManager.Movement.x;

        Vector2 aimDirection = GetAttackAimDirection();
        bool aiming = inputManager.MobileAimActive;

        if (aiming && aimDirection.sqrMagnitude > 0.01f)
            SetFacingDirection(aimDirection.x >= 0f ? 1 : -1);
        else if (!inputManager.IsActionHeld(GameInputAction.Attack))
            UpdateFacingDirection();

        if (inputManager.IsActionHeld(GameInputAction.Crouch))
            Crouch();
        else
            StundUp();

        if (blockAttackUntilMouseRelease && !inputManager.IsActionHeld(GameInputAction.Attack))
            blockAttackUntilMouseRelease = false;

        bool attackHeld = inputManager.IsActionHeld(GameInputAction.Attack) || aiming;

        if (attackHeld && !blockAttackUntilMouseRelease && !IsAnyPanelOpen())
            Attack(aimDirection);

        if (inputManager.IsActionHeld(GameInputAction.Defend))
            StartDefense();
        else
            StopDefense();

        LvlUp();
    }

    private void UpdateFacingDirection()
    {
        if (moveInput > 0f)
            SetFacingDirection(1);
        else if (moveInput < 0f)
            SetFacingDirection(-1);
    }

    private void SetFacingDirection(int direction)
    {
        direction = direction >= 0 ? 1 : -1;

        if (facingDirection == direction && Mathf.Sign(transform.localScale.x) == direction)
            return;

        facingDirection = direction;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facingDirection, transform.localScale.y, transform.localScale.z);

        UpdateHandSorting();
    }

    private void UpdateHandSorting()
    {
        if (frontHand == null || backHand == null)
            return;

        int frontOrder = facingDirection == 1 ? frontHandSortingOrder : backHandSortingOrder;
        int backOrder = facingDirection == 1 ? backHandSortingOrder : frontHandSortingOrder;

        SetSortingOrder(frontHand, frontOrder);
        SetSortingOrder(backHand, backOrder);
    }

    private void SetSortingOrder(Transform hand, int order)
    {
        if (hand == null)
            return;

        foreach (SpriteRenderer renderer in hand.GetComponentsInChildren<SpriteRenderer>(true))
        {
            bool isWeapon = false;
            Transform current = renderer.transform;

            while (current != null && current != hand)
            {
                if (current.name == "WeaponHolder")
                {
                    isWeapon = true;
                    break;
                }

                current = current.parent;
            }

            renderer.sortingOrder = isWeapon ? 2 : order;
        }
    }

    private Transform GetWeaponAttackPoint(EquipmentData weapon)
    {
        if (weapon == null)
            return attackPoint;

        Transform[] hands = { frontHand, backHand };

        foreach (Transform hand in hands)
        {
            if (hand == null)
                continue;

            Transform[] children = hand.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child.name != "WeaponHolder" || !child.gameObject.activeInHierarchy)
                    continue;

                Transform point = FindChildRecursive(child, "AttackPoint");

                if (point != null && point.gameObject.activeInHierarchy)
                    return point;
            }
        }

        return attackPoint;
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent.name == childName)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, childName);

            if (result != null)
                return result;
        }

        return null;
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

        if (isCrouching || isDefending)
            currentMoveSpeed *= 0.5f;

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
        if (!isCrouching)
        {
            isCrouching = true;

            float oldHeight = playerCollider.size.y;
            float newHeight = oldHeight / 2f;

            playerCollider.size = new Vector2(playerCollider.size.x, newHeight);
            playerCollider.offset = new Vector2(playerCollider.offset.x, normalColliderOffset.y - (oldHeight - newHeight) / 2f);
        }
    }

    private bool IsCrouching()
    {
        return isCrouching;
    }

    private void StundUp()
    {
        if (isCrouching)
        {
            isCrouching = false;
            playerCollider.size = normalColliderSize;
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

    private void Attack(Vector2 aimDirection)
    {
        if (isDefending || isTurningToShoot)
            return;

        if (Time.time < nextAttackTime)
            return;

        EquipmentData weapon = stats.GetWeapon();
        float staminaCost = weapon != null ? GetAttackStaminaCost() : 5f;

        if (stamina < staminaCost)
            return;

        if (aimDirection.sqrMagnitude > 0.01f)
            SetFacingDirection(aimDirection.x >= 0f ? 1 : -1);

        UseStamina(staminaCost);

        if (skillManager != null)
            skillManager.RegisterAttack();

        if (weapon == null)
            MeleeAttack();
        else
        {
            switch (weapon.weaponType)
            {
                case WeaponType.Sword:
                    if (meleeWeaponHitbox != null)
                    {
                        meleeWeaponHitbox.StartAttack();
                        StartCoroutine(EndMeleeAttack(0.25f));
                    }
                    else
                        MeleeAttack();
                    break;
                case WeaponType.Bow:
                    ShootProjectile(arrowPrefab, aimDirection);
                    break;
                case WeaponType.Staff:
                    ShootProjectile(magicPrefab, aimDirection);
                    break;
                default:
                    MeleeAttack();
                    break;
            }
        }

        SetAttackCooldown();
    }

    private Vector2 GetAttackAimDirection()
    {
        if (inputManager != null && inputManager.MobileAimActive)
            return inputManager.MobileAim.normalized;

        Camera cam = Camera.main;

        if (cam == null)
            return new Vector2(facingDirection, 0f);

        Vector3 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;
        Vector2 direction = (mousePosition - transform.position);

        return direction.sqrMagnitude > 0.01f ? direction.normalized : new Vector2(facingDirection, 0f);
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

        foreach (Collider2D enemy in enemies)
        {
            EnemyAI enemyAI = enemy.GetComponentInParent<EnemyAI>();

            if (enemyAI == null)
                continue;

            if ((enemy.transform.position.x - transform.position.x) * facingDirection < 0f)
                continue;

            DealMeleeHit(enemyAI, enemy);
        }

        if (skillManager != null)
            skillManager.ConsumeAttackSkill();
    }

    private void ShootProjectile(GameObject projectilePrefab, Vector2 aimDirection)
    {
        if (projectilePrefab == null)
            return;

        EquipmentData weapon = stats.GetWeapon();

        if (weapon == null)
            return;

        Transform currentAttackPoint = GetWeaponAttackPoint(weapon);

        if (currentAttackPoint == null)
        {
            Debug.LogWarning("[Player] No AttackPoint found on the equipped weapon. Assign the fallback Attack Point or add an AttackPoint child to the weapon prefab.", this);
            return;
        }

        if (aimDirection.sqrMagnitude <= 0.01f)
            aimDirection = new Vector2(facingDirection, 0f);

        Vector2 direction = aimDirection.normalized;
        bool doubleShot = false;

        if (skillManager != null && weapon.weaponType == WeaponType.Bow)
            doubleShot = skillManager.ConsumeDoubleShot();

        if (doubleShot)
        {
            ShootSingleProjectile(projectilePrefab, direction, currentAttackPoint);
            ShootSingleProjectile(projectilePrefab, Quaternion.Euler(0f, 0f, 10f) * direction, currentAttackPoint);
        }
        else
        {
            ShootSingleProjectile(projectilePrefab, direction, currentAttackPoint);
        }

        if (skillManager != null)
            skillManager.ConsumeAttackSkill();
    }

    private void ShootSingleProjectile(GameObject projectilePrefab, Vector2 direction, Transform currentAttackPoint = null)
    {
        if (currentAttackPoint == null)
            currentAttackPoint = GetWeaponAttackPoint(stats.GetWeapon());

        if (currentAttackPoint == null)
            return;

        GameObject projectileObject = Instantiate(projectilePrefab, currentAttackPoint.position, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Setup(direction, 0f, 0f, 1f, 0f, 0f, ElementType.None, StatusEffectType.None, 0f, 0f, 0f, false, this);
            projectile.AlignTipToPoint(currentAttackPoint);

            EquipmentData weapon = stats.GetWeapon();

            if (weapon == null)
                return;

            StatusEffectType statusEffect = weapon.GetStatusEffect();
            float statusChance = weapon.GetStatusChance();
            float statusDuration = weapon.GetStatusDuration();
            float statusDamage = weapon.GetStatusDamage(stats.attack);

            float damageMultiplier = skillManager != null ? skillManager.GetDamageMultiplier(Vector2.Distance(transform.position, currentAttackPoint.position)) : 1f;
            float damage = stats.attack * damageMultiplier;

            projectile.Setup(direction, damage, stats.critRate, stats.critDamage, stats.knockback * (skillManager != null ? skillManager.GetKnockbackMultiplier() : 1f), stats.stunChance, GetAttackElement(), statusEffect, statusChance, statusDuration, statusDamage, false, this);
        }
    }

    private ElementType GetAttackElement()
    {
        EquipmentData weapon = stats.GetWeapon();

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

        EquipmentData weapon = stats.GetWeapon();

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
        isTurningToShoot = false;

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
        isTurningToShoot = false;

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

        EquipmentData weapon = stats.GetWeapon();

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

        Transform currentAttackPoint = GetWeaponAttackPoint(weapon);

        if (currentAttackPoint == null)
            return;

        float staminaCost = weapon.weaponType == WeaponType.Staff ? 50f : 25f;

        if (skillManager != null)
            staminaCost *= skillManager.GetStaminaCostMultiplier();

        if (stamina < staminaCost)
            return;

        UseStamina(staminaCost);

        if (skillManager != null)
            skillManager.RegisterAttack();

        int desiredDirection = enemy.transform.position.x >= transform.position.x ? 1 : -1;
        SetFacingDirection(desiredDirection);

        Vector2 direction = (enemy.transform.position - currentAttackPoint.position).normalized;

        if (weapon.weaponType == WeaponType.Bow)
            direction = new Vector2(desiredDirection, 0f);

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
            ShootSingleProjectile(projectilePrefab, direction, currentAttackPoint);
            ShootSingleProjectile(projectilePrefab, Quaternion.Euler(0f, 0f, 10f) * direction, currentAttackPoint);
        }
        else
        {
            ShootSingleProjectile(projectilePrefab, direction, currentAttackPoint);
        }

        if (skillManager != null)
            skillManager.ConsumeAttackSkill();

        SetAttackCooldown();
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
        EquipmentData weapon = stats.GetWeapon();
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

    public void SetBotFacingDirection(int direction)
    {
        SetFacingDirection(direction);
    }

    public void DealMeleeHit(EnemyAI enemyAI, Collider2D enemy)
    {
        if (enemyAI == null || enemy == null)
            return;

        GetWeaponStatus(out StatusEffectType statusEffect, out float statusChance, out float statusDuration, out float statusDamage);

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

        ElementType attackElement = GetAttackElement();
        damage *= ElementSystem.GetDamageMultiplier(attackElement, enemyAI.GetElement());

        Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;

        enemyAI.TakeDamage(
            damage,
            hitPoint,
            stats.knockback * (skillManager != null ? skillManager.GetKnockbackMultiplier() : 1f),
            knockbackDirection,
            stats.stunChance,
            isCrit,
            statusEffect,
            statusChance,
            statusDuration,
            statusDamage,
            this,
            attackElement
        );
    }

    public static void BlockAttackUntilMouseRelease()
    {
        blockAttackUntilMouseRelease = true;
    }

    private System.Collections.IEnumerator EndMeleeAttack(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (meleeWeaponHitbox != null)
            meleeWeaponHitbox.EndAttack();

        if (skillManager != null)
            skillManager.ConsumeAttackSkill();
    }
}
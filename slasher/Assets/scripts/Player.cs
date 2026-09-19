using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float health;
    [SerializeField] private float stamina;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private int lvl = 0;
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

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text lvlText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text staminaText;

    [Header("Floor")]
    [SerializeField] private Transform floorCheck;
    [SerializeField] private float floorCheckRadius = 0.1f;
    [SerializeField] private LayerMask floorLayer;

    [Header("Crouch")]
    [SerializeField] private float crouchHeigth = 0.8f;
    [SerializeField] private CapsuleCollider2D playerCollider;

    private Vector2 normalColliderSize;
    private Vector2 normalColliderOffset;

    private Rigidbody2D rb;
    private float moveInput;
    private Stats stats;
    private float nextAttackTime;
    private float invulnerabilityTimer;
    private bool isDefending;
    private bool canAttack = true;
    private float knockbackTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<Stats>();

        normalColliderSize = playerCollider.size;
        normalColliderOffset = playerCollider.offset;

        rb.freezeRotation = true;

        moveSpeed = stats.speed;
        health = stats.health;
        stamina = stats.stamina;

        UiUpdate();

        expSlider.maxValue = (lvl + 1) * 100;
        expSlider.value = currentEXP;
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && OnFloor() && !IsCrouching() && stamina > 25f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            UseStamina(25);
        }

        if (invulnerabilityTimer > 0f)
            invulnerabilityTimer -= Time.deltaTime;

        UiUpdate();

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

        RegenerateStamina();
        LvlUp();
    }

    public void GetEXP(int exp)
    {
        currentEXP += Mathf.RoundToInt(exp * stats.expBonus);
        expSlider.value = currentEXP;
        LvlUp();
    }

    private void LvlUp()
    {
        if (currentEXP >= (lvl + 1) * 100)
        {
            lvl += 1;
            currentEXP -= lvl * 100;
            lvlText.text = $"Lvl {lvl}";
            expSlider.maxValue = (lvl + 1) * 100;
            expSlider.value = currentEXP;
            stats.freePoint += 1;
        }
    }

    private void UiUpdate()
    {
        healthSlider.maxValue = stats.health;
        staminaSlider.maxValue = stats.stamina;
        healthSlider.value = health;
        staminaSlider.value = stamina;
        healthText.text = Mathf.Round(healthSlider.value) + "/" + Mathf.Round(healthSlider.maxValue);
        staminaText.text = Mathf.Round(staminaSlider.value) + "/" + Mathf.Round(staminaSlider.maxValue);
    }

    private void FixedUpdate()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    private bool OnFloor()
    {
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
        if (!canAttack)
            return;

        if (isDefending)
            return;

        if (Time.time < nextAttackTime)
            return;

        ItemData weapon = stats.GetWeapon();

        if (weapon == null)
        {
            MeleeAttack();
        }
        else
        {
            switch (weapon.weaponType)
            {
                case WeaponType.Sword:
                    MeleeAttack();
                    break;

                case WeaponType.Bow:
                    ShootProjectile(arrowPrefab);
                    break;

                case WeaponType.Staff:
                    ShootProjectile(magicPrefab);
                    break;

                default:
                    MeleeAttack();
                    break;
            }
        }

        float currentCooldown = Mathf.Max(attackCooldown / (1f + stats.speed * 0.05f), 0.15f);
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

                if (isCrit)
                    damage *= stats.critDamage;

                float elementMultiplier = ElementSystem.GetDamageMultiplier(GetAttackElement(), enemyAI.GetElement());
                damage *= elementMultiplier;

                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                enemyAI.TakeDamage(damage, hitPoint, stats.knockback, knockbackDirection, stats.stunChance, isCrit, statusEffect, statusChance, statusDuration, statusDamage);
            }
        }
    }

    private void ShootProjectile(GameObject projectilePrefab)
    {
        if (projectilePrefab == null)
            return;

        if (attackPoint == null)
            return;

        Camera cam = Camera.main;

        if (cam == null)
            return;

        ItemData weapon = stats.GetWeapon();

        if (weapon == null)
            return;

        Vector3 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - attackPoint.position).normalized;
        GameObject projectileObject = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            StatusEffectType statusEffect = weapon.GetStatusEffect();
            float statusChance = weapon.GetStatusChance();
            float statusDuration = weapon.GetStatusDuration();
            float statusDamage = weapon.GetStatusDamage(stats.attack);

            projectile.Setup(direction, stats.attack, stats.critRate, stats.critDamage, stats.knockback, stats.stunChance, GetAttackElement(), statusEffect, statusChance, statusDuration, statusDamage);
        }
    }

    private ElementType GetAttackElement()
    {
        ItemData weapon = stats.GetWeapon();

        if (weapon == null)
            return ElementType.None;

        return weapon.element;
    }

    public void SetCanAttack(bool value)
    {
        canAttack = value;
        if (!value) isDefending = false;
    }

    public void TakeDamage(float damage)
    {
        if (invulnerabilityTimer > 0f)
            return;

        invulnerabilityTimer = invulnerabilityTime;

        damage *= 100f / (100f + stats.defence);

        if (isDefending)
        {
            damage *= 1f - defenseDamageReduction;
        }

        health -= damage;
        health = Mathf.Clamp(health, 0f, stats.health);

        if (health <= 0f)
        {
            //Die 
        }

        UiUpdate();
    }

    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0f, stats.health);
        UiUpdate();
    }

    private void UseStamina(float amount)
    {
        stamina -= amount;
        stamina = Mathf.Clamp(stamina, 0f, stats.stamina);
        UiUpdate();
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

        float regeneration = IsCrouching() ? 15f : 10f;
        stamina += regeneration * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0f, stats.stamina);
        UiUpdate();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (rb == null)
            return;

        knockbackTimer = 0.3f;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }
}
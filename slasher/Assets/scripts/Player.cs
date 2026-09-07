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
    private bool isDefending;
    private bool canAttack = true;

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

        if (Input.GetKeyDown(KeyCode.Space) && OnFloor() && !IsCrouching() &&stamina > 25f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            UseStamina(25);
        }

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
        rb.velocity = new Vector2(moveInput * moveSpeed,rb.velocity.y);
    }

    private bool OnFloor()
    {
        return Physics2D.OverlapCircle(floorCheck.position,floorCheckRadius,floorLayer) != null;
    }

    private void Crouch()
    {
        if (!IsCrouching())
        {
            float oldHeight = playerCollider.size.y;
            float newHeight = oldHeight / 2f;
            moveSpeed /= 2;
            playerCollider.size = new Vector2(playerCollider.size.x,newHeight);
            playerCollider.offset = new Vector2(playerCollider.offset.x,normalColliderOffset.y -(oldHeight - newHeight) / 2f);
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

        float currentCooldown = Mathf.Max(attackCooldown / (1f + stats.speed * 0.05f),0.15f);
        nextAttackTime = Time.time + currentCooldown;
    }

    private void MeleeAttack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position,attackRange);
        foreach (Collider2D enemy in enemies)
        {
            EnemyAI enemyAI =
                enemy.GetComponent<EnemyAI>();

            if (enemyAI != null)
            {
                enemyAI.TakeDamage(stats.attack);
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

        Vector3 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = (mousePosition - attackPoint.position).normalized;

        GameObject projectileObject = Instantiate(projectilePrefab,attackPoint.position,Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Setup(direction,stats.attack);
        }
    }

    public void SetCanAttack(bool value)
    {
        canAttack = value;
        if (!value) isDefending = false;
    }

    public void TakeDamage(float damage)
    {
        damage *= 100f /(100f + stats.defence);

        if (isDefending)
        {
            damage *= 1f -defenseDamageReduction;
        }

        health -= damage;
        health = Mathf.Clamp(health,0f,stats.health);

        if (health <= 0f)
        {
            //Die
        }
        UiUpdate();
    }

    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health,0f,stats.health);
        UiUpdate();
    }

    private void UseStamina(float amount)
    {
        stamina -= amount;
        stamina = Mathf.Clamp(stamina,0f, stats.stamina);
        UiUpdate();
    }

    private void RegenerateStamina()
    {
        if (!OnFloor())
            return;

        float regeneration = IsCrouching()? 15f: 10f;
        stamina += regeneration * Time.deltaTime;
        stamina = Mathf.Clamp(stamina,0f,stats.stamina);
        UiUpdate();
    }
}
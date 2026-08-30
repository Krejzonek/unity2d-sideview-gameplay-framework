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

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text lvlText;

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
    private float attackTimer;
    private bool isDefending;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<Stats>();
        normalColliderSize = playerCollider.size;
        normalColliderOffset = playerCollider.offset;

        rb.freezeRotation = true;
        moveSpeed = stats.speed;
        healthSlider.maxValue = stats.health;
        healthSlider.value = health;
        staminaSlider.maxValue = stats.stamina;
        staminaSlider.value = stamina;
        expSlider.maxValue = (lvl + 1) * 100;
        expSlider.value = currentEXP;
        
    }
    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space) && OnFloor() && !IsCrouching() && stamina>25f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            UseStamina(25);
        }
        healthSlider.maxValue = stats.health;
        staminaSlider.maxValue = stats.stamina;
        if (Input.GetKey(KeyCode.LeftShift)) Crouch();
        else StundUp();
        attackTimer -= Time.deltaTime;
        
        if (Input.GetMouseButtonDown(0))Attack();

        if (Input.GetMouseButton(1))StartDefense();
        else StopDefense();

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
        if(currentEXP >= (lvl+1) * 100)
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
        healthSlider.value = health;
        staminaSlider.value = stamina;
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

    }
    private bool OnFloor()
    {
        return Physics2D.OverlapCircle(floorCheck.position,floorCheckRadius, floorLayer) != null;
    }
    private void Crouch()
    {
        if (!IsCrouching())
        {
            float oldHeight = playerCollider.size.y;
            float newHeight = oldHeight / 2f;
            moveSpeed /= 2;
            playerCollider.size = new Vector2(playerCollider.size.x,newHeight);
            playerCollider.offset = new Vector2(playerCollider.offset.x,normalColliderOffset.y - (oldHeight - newHeight) / 2f);
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
        if (isDefending)return;
        isDefending = true;
      
    }

    private void StopDefense()
    {
        isDefending = false;
    }
    private void Attack()
    {
        if (attackTimer > 0f)return;
        if (isDefending)return;
        float currentCooldown = Mathf.Max(attackCooldown / stats.speed,0.1f);
        attackTimer = currentCooldown;
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position,attackRange);
        foreach (Collider2D enemy in enemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(stats.attack);
            }
        }
    }
    public void TakeDamage(float damage)
    {
        damage *= 100f / (100f + stats.defence);
        if (isDefending)
        {
            damage *= 1f - defenseDamageReduction;
        }
        health -= damage;
        health = Mathf.Clamp(health, 0f, stats.health);
        if(health <= 0f)
        {
            //Die
        }

        UiUpdate();
    }

    private void Heal(float amount)
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
    private void RegenerateStamina()
    {
        if (!OnFloor())return;
        float regeneration = IsCrouching() ? 10f : 5f;
        stamina += regeneration * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0f, stats.stamina);
        UiUpdate();
    }
    /*private void OnDrawGizmosSelected()
    {

        if(floorCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(floorCheck.position, floorCheckRadius);
        
    }*/
}
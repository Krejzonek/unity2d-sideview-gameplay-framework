using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float health;

    [SerializeField] private float stamina = 100f;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce = 10f;

    [SerializeField] private int lvl = 0;
    [SerializeField] private int currentEXP;

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
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<Stats>();
        normalColliderSize = playerCollider.size;
        normalColliderOffset = playerCollider.offset;

        rb.freezeRotation = true;
        healthSlider.maxValue = stats.health;
        healthSlider.value = health;
        staminaSlider.maxValue = stats.stamina;
        staminaSlider.value = stamina;
        expSlider.maxValue = (lvl + 1) * 100;
        expSlider.value = currentEXP;
        moveSpeed = stats.speed;
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

        RegenerateStamina();
        LvlUp();
    }

    public void GetEXP(int exp)
    {
        currentEXP += exp;
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
    public void TakeDamage(float damage)
    {
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
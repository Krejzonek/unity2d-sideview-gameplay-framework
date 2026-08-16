using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;


    [Header("Floor")]
    [SerializeField] private Transform floorCheck;
    [SerializeField] private float floorCheckRadius = 0.2f;
    [SerializeField] private LayerMask floorLayer;

    private Rigidbody2D rb;
    private float moveInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        if(Input.GetKeyDown(KeyCode.Space) && OnFloor()) 
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

    }
    private bool OnFloor()
    {
        return Physics2D.OverlapCircle(floorCheck.position,floorCheckRadius, floorLayer);
    }
    private void OnDrawGizmosSelected()
    {
        if(floorCheck == null) return;
        Gizmos.DrawWireSphere(floorCheck.position, floorCheckRadius);
        
    }

}
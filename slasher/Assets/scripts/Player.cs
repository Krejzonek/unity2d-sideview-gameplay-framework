using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;


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

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        normalColliderSize = playerCollider.size;
        normalColliderOffset = playerCollider.offset;
        rb.freezeRotation = true;
    }
    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        if(Input.GetKeyDown(KeyCode.Space) && OnFloor() && !IsCrouching()) rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        if (Input.GetKey(KeyCode.LeftShift)) Crouch();
        else StundUp();
        
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
            playerCollider.offset = normalColliderOffset;
        }
    }
    /*private void OnDrawGizmosSelected()
    {

        if(floorCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(floorCheck.position, floorCheckRadius);
        
    }*/
}
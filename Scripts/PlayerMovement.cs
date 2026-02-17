using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Combat")]
    public PlayerAttack playerAttack;

    private Rigidbody2D rb;
    private Vector2 movementDirection;
    private Vector2 lastDirection = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerAttack == null)
            playerAttack = GetComponent<PlayerAttack>();
    }

    void Update()
    {
        if (playerAttack != null && playerAttack.IsAttacking())
        {
            return;
        }

        movementDirection.x = Input.GetAxisRaw("Horizontal");
        movementDirection.y = Input.GetAxisRaw("Vertical");

        if (movementDirection.sqrMagnitude > 1)
        {
            movementDirection.Normalize();
        }

        if (movementDirection != Vector2.zero)
        {
            lastDirection = movementDirection;
        }

        if (animator != null)
        {
            if (movementDirection != Vector2.zero)
            {
                animator.SetFloat("MoveX", movementDirection.x);
                animator.SetFloat("MoveY", movementDirection.y);
            }
            else
            {
                animator.SetFloat("MoveX", lastDirection.x);
                animator.SetFloat("MoveY", lastDirection.y);
            }

            animator.SetBool("IsMoving", movementDirection != Vector2.zero);
        }
    }

    void FixedUpdate()
    {
        if (playerAttack != null && playerAttack.IsAttacking())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (rb != null)
        {
            rb.linearVelocity = movementDirection * moveSpeed;
        }
    }

    public Vector2 GetLastDirection()
    {
        return lastDirection;
    }
}
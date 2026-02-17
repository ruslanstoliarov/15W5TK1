using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float damage = 1f;
    public float attackRange = 1.5f;
    public float attackRate = 2f;
    private float nextAttackTime = 0f;

    [Header("Animation")]
    public Animator animator;
    public float attackAnimationTime = 0.3f;

    [Header("Visual")]
    public GameObject attackEffect;
    public Transform attackPoint;
    public float attackRadius = 0.5f;

    [Header("Layers")]
    public LayerMask enemyLayers;

    private bool isAttacking = false;
    private Vector2 lastAttackDirection = Vector2.down;
    private Camera mainCamera;

    void Start()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        if (attackPoint == null)
        {
            GameObject point = new GameObject("AttackPoint");
            point.transform.parent = transform;
            attackPoint = point.transform;
        }
    }

    void Update()
    {
        UpdateAttackPointPosition();

        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime && !isAttacking)
        {
            StartCoroutine(PerformAttack());
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    void UpdateAttackPointPosition()
    {
        if (mainCamera == null || isAttacking) return;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector2 direction = (mousePos - transform.position).normalized;

        if (direction.magnitude > 0.1f)
        {
            lastAttackDirection = direction;
        }

        attackPoint.position = transform.position + (Vector3)direction * attackRange;
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetFloat("AttackDirectionX", lastAttackDirection.x);
            animator.SetFloat("AttackDirectionY", lastAttackDirection.y);
            animator.SetTrigger("IsAttacking");
        }

        yield return new WaitForSeconds(0.1f);

        CheckHit();

        if (attackEffect != null)
        {
            Instantiate(attackEffect, attackPoint.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(attackAnimationTime);

        if (animator != null)
        {
            animator.ResetTrigger("IsAttacking");
        }

        isAttacking = false;
    }

    void CheckHit()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"Hit: {enemy.name}");

            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyScript.TakeDamage(Mathf.RoundToInt(damage), knockbackDir);
            }
        }
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, attackPoint.position);
        }
    }
}
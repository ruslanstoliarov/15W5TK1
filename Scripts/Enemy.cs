using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 3f;
    public int health = 3;
    public int damage = 1;

    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float stoppingDistance = 0.5f;
    public float aggroRange = 10f;

    [Header("Combat")]
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Knockback")]
    public float knockbackForce = 20f;
    public float knockbackDuration = 0.2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isHurting = false;
    private bool isKnockedBack = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (isHurting || isKnockedBack || player == null || rb == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < aggroRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;

            if (spriteRenderer != null && direction.x != 0)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isHurting && !isKnockedBack)
        {
            if (Time.time > lastAttackTime + attackCooldown)
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    lastAttackTime = Time.time;

                    Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
                    rb.AddForce(knockbackDirection * 3f, ForceMode2D.Impulse);
                }
            }
        }
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isKnockedBack) return;

        health -= damage;
        Debug.Log($"Hit: {health}");

        StartCoroutine(HitAnimation());
        StartCoroutine(Knockback(knockbackDirection));

        if (health <= 0)
        {
            Die();
        }
    }

    IEnumerator Knockback(Vector2 direction)
    {
        isKnockedBack = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(knockbackDuration);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        isKnockedBack = false;
    }

    IEnumerator HitAnimation()
    {
        isHurting = true;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if (spriteRenderer != null && health > 0)
            spriteRenderer.color = Color.white;

        isHurting = false;
    }

    void Die()
    {
        Debug.Log("Kill!");
        Destroy(gameObject);
    }
}
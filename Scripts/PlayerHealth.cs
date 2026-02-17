using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("Effects")]
    public float invincibilityTime = 1f;
    public int blinkCount = 5;
    private bool isInvincible = false;

    private SpriteRenderer spriteRenderer;
    private GameOverManager gameOverManager;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateUI();

        gameOverManager = FindFirstObjectByType<GameOverManager>();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log($"Health: {currentHealth}");

        StartCoroutine(InvincibilityFrames());
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.3f);
            yield return new WaitForSeconds(invincibilityTime / (blinkCount * 2));
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(invincibilityTime / (blinkCount * 2));
        }

        isInvincible = false;
    }

    void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    void Die()
    {
        Debug.Log("Игрок умер!");

        if (gameOverManager != null)
            gameOverManager.ShowGameOver();
        else
            gameObject.SetActive(false);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }
}
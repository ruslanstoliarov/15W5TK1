using UnityEngine;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    public GameObject upgradeCanvas;
    public PlayerAttack playerAttack;
    public PlayerMovement playerMovement;
    public PlayerHealth playerHealth;
    public TextMeshProUGUI titleText;

    private bool isUpgrading = false;

    void Start()
    {
        if (upgradeCanvas != null)
            upgradeCanvas.SetActive(false);
    }

    public void ShowUpgradeMenu()
    {
        isUpgrading = true;
        Time.timeScale = 0f;
        upgradeCanvas.SetActive(true);
    }

    public void HideUpgradeMenu()
    {
        isUpgrading = false;
        Time.timeScale = 1f;
        upgradeCanvas.SetActive(false);
    }

    public void OnDamageUpgrade()
    {
        if (playerAttack != null)
        {
            playerAttack.damage += 0.3f;
            Debug.Log("DMG +0.3");
        }
        HideUpgradeMenu();
    }

    public void OnSpeedUpgrade()
    {
        if (playerMovement != null)
        {
            playerMovement.moveSpeed += 0.2f;
            Debug.Log("Speed +0.2");
        }
        HideUpgradeMenu();
    }

    public void OnHealthUpgrade()
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(playerHealth.maxHealth);
            Debug.Log("Heal");
        }
        HideUpgradeMenu();
    }

    public bool IsUpgrading()
    {
        return isUpgrading;
    }
}
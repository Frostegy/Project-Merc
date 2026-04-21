using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Slider healthSlider;
    public TMP_Text healthText;

    [Header("Damage")]
    public float damageCooldown = 0.5f;

    bool isDead;
    float damageCooldownTimer;

    PlayerController playerController;
    InputManager inputManager;
    CharacterController characterController;
    ActiveWeapon activeWeapon;
    GameManager gameManager;

    void Awake()
    {
        currentHealth = maxHealth;

        playerController = GetComponent<PlayerController>();
        inputManager = GetComponent<InputManager>();
        characterController = GetComponent<CharacterController>();
        activeWeapon = GetComponentInChildren<ActiveWeapon>();
        gameManager = FindFirstObjectByType<GameManager>();

        UpdateUI();
    }

    void Update()
    {
        if (damageCooldownTimer > 0f)
        {
            damageCooldownTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (damageCooldownTimer > 0f)
            return;

        damageCooldownTimer = damageCooldown;

        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        UpdateUI();
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (playerController != null)
            playerController.enabled = false;

        if (inputManager != null)
            inputManager.enabled = false;

        if (characterController != null)
            characterController.enabled = false;

        if (activeWeapon != null)
        {
            activeWeapon.SetAiming(false);
            activeWeapon.SetFiringInput(false, false, false);
        }

        if (gameManager != null)
        {
            gameManager.PlayerDied();
        }
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
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }
}
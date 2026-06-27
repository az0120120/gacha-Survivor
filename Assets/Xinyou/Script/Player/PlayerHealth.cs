using System;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] float invincibilityDuration = 0.1f;

    CharacterStats characterStats;
    int currentHealth;
    float invincibilityTimer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => characterStats != null ? characterStats.MaxHealth : 100;
    public bool IsAlive => currentHealth > 0;
    public bool IsInvincible => invincibilityTimer > 0f;

    public event Action<int, int> OnHealthChanged;

    void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        currentHealth = MaxHealth;
    }

    void Start()
    {
        NotifyHealthChanged();
    }

    void Update()
    {
        if (invincibilityTimer <= 0f)
            return;

        invincibilityTimer -= Time.deltaTime;
        if (invincibilityTimer < 0f)
            invincibilityTimer = 0f;
    }

    public void AddMaxHealthBonus(int amount)
    {
        if (amount <= 0 || characterStats == null)
            return;

        characterStats.AddMaxHealth(amount);
        currentHealth += amount;
        NotifyHealthChanged();
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(StatMath.FloorToInt(damage));
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive || damage <= 0 || IsInvincible)
            return;

        currentHealth -= damage;
        invincibilityTimer = invincibilityDuration;
        NotifyHealthChanged();

        if (currentHealth <= 0)
            Die();
    }

    void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    void Die()
    {
        currentHealth = 0;
        NotifyHealthChanged();

        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        var autoAttack = GetComponent<SimpleAutoAttack>();
        if (autoAttack != null)
            autoAttack.enabled = false;

        var weaponManager = GetComponent<WeaponManager>();
        if (weaponManager != null)
            weaponManager.enabled = false;

        DefeatManager.Instance?.TriggerDefeat();

        enabled = false;
    }
}

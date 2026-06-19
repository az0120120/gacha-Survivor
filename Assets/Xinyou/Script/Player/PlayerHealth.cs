using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    CharacterStats characterStats;
    int currentHealth;

    public bool IsAlive => currentHealth > 0;

    void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        currentHealth = characterStats != null ? characterStats.MaxHealth : 100;
    }

    public void AddMaxHealthBonus(int amount)
    {
        if (amount <= 0 || characterStats == null)
            return;

        characterStats.AddMaxHealth(amount);
        currentHealth += amount;
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(StatMath.FloorToInt(damage));
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive || damage <= 0)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("Player died");

        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        var autoAttack = GetComponent<SimpleAutoAttack>();
        if (autoAttack != null)
            autoAttack.enabled = false;

        enabled = false;
    }
}

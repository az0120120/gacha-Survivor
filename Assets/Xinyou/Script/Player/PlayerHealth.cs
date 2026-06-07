using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] float maxHealth = 100f;

    float currentHealth;

    public bool IsAlive => currentHealth > 0f;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
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

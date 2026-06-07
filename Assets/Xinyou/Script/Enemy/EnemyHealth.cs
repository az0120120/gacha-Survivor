using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] float maxHealth = 30f;
    [SerializeField] float contactDamage = 5f;
    [SerializeField] float contactCooldown = 1f;

    float currentHealth;
    float contactTimer;

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
        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!IsAlive)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        contactTimer -= Time.fixedDeltaTime;
        if (contactTimer > 0f)
            return;

        var damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable == null || !damageable.IsAlive)
            return;

        damageable.TakeDamage(contactDamage);
        contactTimer = contactCooldown;
    }
}

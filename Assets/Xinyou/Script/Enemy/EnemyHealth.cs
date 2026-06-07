using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] float maxHealth = 30f;
    [SerializeField] float contactDamage = 5f;
    [SerializeField] float contactCooldown = 1f;
    [SerializeField] int expDrop = 1;

    float currentHealth;
    float contactTimer;
    ObjectPool pool;

    public bool IsAlive => currentHealth > 0f;

    public void BindPool(ObjectPool objectPool)
    {
        pool = objectPool;
    }

    public void OnGetFromPool()
    {
        currentHealth = maxHealth;
        contactTimer = 0f;
    }

    public void OnReturnToPool()
    {
        currentHealth = 0f;
        contactTimer = 0f;
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
        DropExperience();

        if (pool != null)
            pool.Release(gameObject);
        else
            Destroy(gameObject);
    }

    void DropExperience()
    {
        if (ExperienceManager.Instance == null)
            return;

        ExperienceManager.Instance.SpawnOrb(transform.position, expDrop);
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

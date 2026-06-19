using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] float contactCooldown = 1f;
    [SerializeField] int expDrop = 1;
    [SerializeField] int goldDrop = 1;

    float currentHealth;
    float contactTimer;
    ObjectPool pool;
    EnemyStats enemyStats;

    public bool IsAlive => currentHealth > 0f;
    public EnemyStats Stats => enemyStats;

    void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
        if (enemyStats == null)
            enemyStats = gameObject.AddComponent<EnemyStats>();
    }

    public void BindPool(ObjectPool objectPool)
    {
        pool = objectPool;
    }

    public void OnGetFromPool()
    {
        enemyStats.RefreshFromGameTime();
        currentHealth = enemyStats.MaxHealth;
        contactTimer = 0f;
    }

    public void OnReturnToPool()
    {
        currentHealth = 0f;
        contactTimer = 0f;
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(StatMath.FloorToInt(damage), transform.position, 0f);
    }

    public void TakeDamage(int damage, Vector2 knockbackSource, float knockbackForce, bool isCritical = false)
    {
        if (!IsAlive || damage <= 0)
            return;

        if (DamageNumberManager.Instance != null)
            DamageNumberManager.Instance.Show(damage, transform.position, isCritical);

        currentHealth -= damage;

        if (knockbackForce > 0f)
        {
            var enemyAI = GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                Vector2 direction = (Vector2)transform.position - knockbackSource;
                if (direction.sqrMagnitude < 0.0001f)
                    direction = Vector2.up;

                enemyAI.ApplyKnockback(direction.normalized, knockbackForce);
            }
        }

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        KillCounter.Instance?.RegisterKill();
        DropRewards();

        if (pool != null)
            pool.Release(gameObject);
        else
            Destroy(gameObject);
    }

    void DropRewards()
    {
        DropExperience();
        DropGold();
    }

    void DropGold()
    {
        if (GoldManager.Instance == null)
            return;

        GoldManager.Instance.SpawnCoin(transform.position, goldDrop);
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

        var playerStats = collision.gameObject.GetComponent<CharacterStats>();
        if (playerStats == null)
            return;

        int damage = DamageCalculator.CalculateAgainstPlayer(playerStats, enemyStats.Attack);
        if (damage <= 0)
        {
            contactTimer = contactCooldown;
            return;
        }

        var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth == null || !playerHealth.IsAlive)
            return;

        playerHealth.TakeDamage(damage);
        contactTimer = contactCooldown;
    }
}

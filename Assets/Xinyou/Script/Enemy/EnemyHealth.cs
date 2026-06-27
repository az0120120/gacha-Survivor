using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable, IPoolable
{
    float contactCooldown = 1f;
    int expDrop = 1;
    int goldDrop = 1;

    const float HitSoundCooldown = 0.07f;

    float currentHealth;
    float contactTimer;
    float nextHitSoundTime;
    ObjectPool pool;
    EnemyStats enemyStats;

    public bool IsAlive => currentHealth > 0f;
    public float CurrentHealth => currentHealth;
    public int MaxHealth => enemyStats != null ? enemyStats.MaxHealth : 0;
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
        ApplyDropSettingsFromStats();
        currentHealth = enemyStats.MaxHealth;
        contactTimer = 0f;
        nextHitSoundTime = 0f;
    }

    public void RefreshHealthFromStats()
    {
        ApplyDropSettingsFromStats();
        currentHealth = enemyStats.MaxHealth;
    }

    void ApplyDropSettingsFromStats()
    {
        if (enemyStats == null)
            return;

        contactCooldown = enemyStats.ContactCooldown;
        expDrop = enemyStats.ExpDrop;
        goldDrop = enemyStats.GoldDrop;
    }

    public void OnReturnToPool()
    {
        currentHealth = 0f;
        contactTimer = 0f;
        nextHitSoundTime = 0f;
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(StatMath.FloorToInt(damage), transform.position, 0f);
    }

    public void TakeDamage(int damage, Vector2 knockbackSource, float knockbackForce, bool isCritical = false)
    {
        if (!IsAlive || damage <= 0)
            return;

        var bossBehavior = GetComponent<BossBehavior>();
        if (bossBehavior != null && bossBehavior.TryDodge())
            return;

        if (DamageNumberManager.Instance != null)
            DamageNumberManager.Instance.Show(damage, transform.position, isCritical);

        currentHealth -= damage;
        PlayHitSound(isCritical);

        if (knockbackForce > 0f)
        {
            var enemyAI = GetComponent<EnemyAI>();
            if (enemyAI != null && enemyAI.enabled)
            {
                Vector2 direction = (Vector2)transform.position - knockbackSource;
                if (direction.sqrMagnitude < 0.0001f)
                    direction = Vector2.up;

                enemyAI.ApplyKnockback(direction.normalized, knockbackForce);
            }
            else if (bossBehavior != null && bossBehavior.IsActive)
            {
                Vector2 direction = (Vector2)transform.position - knockbackSource;
                if (direction.sqrMagnitude < 0.0001f)
                    direction = Vector2.up;

                bossBehavior.ApplyKnockback(direction.normalized, knockbackForce);
            }
        }

        if (currentHealth <= 0f)
            Die();
    }

    void PlayHitSound(bool isCritical)
    {
        if (Time.time < nextHitSoundTime)
            return;

        if (EnemyHitSfxManager.Instance == null)
            return;

        var boss = GetComponent<BossEnemy>();
        bool isBoss = boss != null && boss.IsConfigured;
        EnemyHitSfxManager.Instance.PlayHit(transform.position, isCritical, isBoss);
        nextHitSoundTime = Time.time + HitSoundCooldown;
    }

    void Die()
    {
        var boss = GetComponent<BossEnemy>();
        if (boss != null && boss.IsConfigured)
            VictoryManager.Instance?.RegisterBossDefeated(boss);

        KillCounter.Instance?.RegisterKill();
        DropRewards();

        if (pool != null)
        {
            if (boss != null && boss.IsConfigured)
                FindFirstObjectByType<BossSpawner>()?.NotifyBossReleased(gameObject);

            pool.Release(gameObject);
        }
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

        ExperienceManager.Instance.AddExperience(expDrop);
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

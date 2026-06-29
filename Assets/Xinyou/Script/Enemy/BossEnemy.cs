using UnityEngine;

public class BossEnemy : MonoBehaviour, IPoolable
{
    BossDefinition definition;
    int bossIndex = -1;
    EnemyAI enemyAI;
    BossBehavior bossBehavior;

    public int BossIndex => bossIndex;
    public bool IsConfigured => definition != null && bossIndex >= 0;
    public bool IsFinalBoss => bossIndex >= BossCatalogFinalBossIndex.LastBossIndex;
    public string DisplayName => definition != null ? definition.displayName : "Boss";

    void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        bossBehavior = GetComponent<BossBehavior>();
        if (bossBehavior == null)
            bossBehavior = gameObject.AddComponent<BossBehavior>();
    }

    public void Configure(BossDefinition bossDefinition, int index, EnemyCatalog enemyCatalog)
    {
        definition = bossDefinition;
        bossIndex = index;

        var stats = GetComponent<EnemyStats>();
        if (stats != null)
            stats.ApplyBossDefinition(bossDefinition, enemyCatalog);

        if (enemyAI != null)
            enemyAI.enabled = false;

        bossBehavior.Activate(bossDefinition, enemyCatalog);

        var health = GetComponent<EnemyHealth>();
        health?.RefreshHealthFromStats();

        if (definition != null)
            gameObject.name = definition.displayName;
    }

    public void OnGetFromPool()
    {
    }

    public void OnReturnToPool()
    {
        definition = null;
        bossIndex = -1;
        gameObject.name = "Boss";

        if (enemyAI != null)
            enemyAI.enabled = true;

        bossBehavior?.Deactivate();
    }
}

public static class BossCatalogFinalBossIndex
{
    public const int LastBossIndex = 4;
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyStats))]
public class BossBehavior : MonoBehaviour, IPoolable
{
    const float MeleeMoveSpeed = 2.4f;
    const float RangedMoveSpeed = 1.4f;
    const float PreferredRange = 7f;
    const float MinRange = 4.5f;
    const float S1mpleRetreatSpeed = 2f;

    BossDefinition definition;
    EnemyArchetypeDefinition rangedDefinition;
    Transform player;
    Rigidbody2D rb;
    EnemyStats enemyStats;
    SpriteRenderer spriteRenderer;
    Vector2 knockbackVelocity;
    float shootTimer;
    bool isActive;

    public bool IsActive => isActive;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyStats = GetComponent<EnemyStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Activate(BossDefinition bossDefinition, EnemyCatalog enemyCatalog)
    {
        definition = bossDefinition;
        rangedDefinition = enemyCatalog != null
            ? enemyCatalog.GetDefinition(EnemyArchetype.RangedShooter)
            : null;

        isActive = bossDefinition != null;
        shootTimer = bossDefinition != null ? bossDefinition.shootInterval * 0.5f : 0f;
        knockbackVelocity = Vector2.zero;

        if (player == null)
            CachePlayer();
    }

    public void Deactivate()
    {
        isActive = false;
        definition = null;
        rangedDefinition = null;
        knockbackVelocity = Vector2.zero;
        if (rb != null)
            rb.velocity = Vector2.zero;
    }

    public bool TryDodge()
    {
        if (!isActive || definition == null || definition.behaviorType != BossBehaviorType.S1mple)
            return false;

        return Random.value < definition.dodgeChance;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        knockbackVelocity += direction.normalized * force;
    }

    public void OnGetFromPool()
    {
        knockbackVelocity = Vector2.zero;
        shootTimer = 0f;
    }

    public void OnReturnToPool()
    {
        Deactivate();
    }

    void FixedUpdate()
    {
        if (!isActive || definition == null || player == null)
            return;

        if (MapPropStatusEffects.AreEnemiesFrozen)
        {
            rb.velocity = knockbackVelocity;
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, 10f * Time.fixedDeltaTime);
            return;
        }

        Vector2 velocity = ComputeVelocity();
        rb.velocity = velocity + knockbackVelocity;
        knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, 10f * Time.fixedDeltaTime);
    }

    void Update()
    {
        if (!isActive || definition == null || player == null)
            return;

        if (definition.behaviorType == BossBehaviorType.S1mple)
            UpdateS1mpleFacing();
    }

    Vector2 ComputeVelocity()
    {
        switch (definition.behaviorType)
        {
            case BossBehaviorType.Xiaoxia:
                return ComputeChaseVelocity(MeleeMoveSpeed);
            case BossBehaviorType.Fazeniko:
            case BossBehaviorType.G2niko:
                return ComputeShooterVelocity();
            case BossBehaviorType.S1mple:
                return ComputeS1mpleVelocity();
            case BossBehaviorType.FalconNiko:
                return ComputeFalconVelocity();
            default:
                return ComputeChaseVelocity(MeleeMoveSpeed);
        }
    }

    Vector2 ComputeChaseVelocity(float speed)
    {
        Vector2 direction = GetDirectionToPlayer();
        return direction.sqrMagnitude > 0.0001f ? direction.normalized * speed : Vector2.zero;
    }

    Vector2 ComputeShooterVelocity()
    {
        Vector2 offset = GetDirectionToPlayer();
        float distance = offset.magnitude;
        if (distance < 0.0001f)
            return Vector2.zero;

        Vector2 direction = offset / distance;
        shootTimer -= Time.fixedDeltaTime;

        if (distance < MinRange)
            return -direction * RangedMoveSpeed;

        if (distance <= PreferredRange)
        {
            TryShootSingle(direction);
            return Vector2.zero;
        }

        return direction * RangedMoveSpeed;
    }

    Vector2 ComputeS1mpleVelocity()
    {
        Vector2 toPlayer = GetDirectionToPlayer();
        if (toPlayer.sqrMagnitude < 0.0001f)
            return Vector2.zero;

        return -toPlayer.normalized * S1mpleRetreatSpeed;
    }

    Vector2 ComputeFalconVelocity()
    {
        Vector2 offset = GetDirectionToPlayer();
        float distance = offset.magnitude;
        if (distance < 0.0001f)
            return Vector2.zero;

        Vector2 direction = offset / distance;
        shootTimer -= Time.fixedDeltaTime;

        if (distance <= PreferredRange)
        {
            TryShootFan(direction);
            return Vector2.zero;
        }

        return direction * (RangedMoveSpeed * 0.8f);
    }

    void TryShootSingle(Vector2 direction)
    {
        if (shootTimer > 0f)
            return;

        shootTimer = definition.shootInterval;
        FireProjectile(direction);
    }

    void TryShootFan(Vector2 direction)
    {
        if (shootTimer > 0f)
            return;

        shootTimer = definition.shootInterval;
        FireFan(direction);
    }

    void FireProjectile(Vector2 direction)
    {
        if (EnemyProjectilePool.Instance == null || enemyStats == null)
            return;

        EnemyProjectilePool.Instance.Fire(
            transform.position,
            direction,
            GetRangedBulletDamage(),
            GetProjectileSpeed(),
            rangedDefinition);
    }

    void FireFan(Vector2 baseDirection)
    {
        if (EnemyProjectilePool.Instance == null || enemyStats == null)
            return;

        int count = Mathf.Max(1, definition.fanBulletCount);
        float halfAngle = definition.fanHalfAngle;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        int damage = GetRangedBulletDamage();
        float speed = GetProjectileSpeed();

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : i / (float)(count - 1);
            float angleDeg = baseAngle - halfAngle + t * halfAngle * 2f;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            EnemyProjectilePool.Instance.Fire(
                transform.position,
                direction,
                damage,
                speed,
                rangedDefinition);
        }
    }

    int GetRangedBulletDamage()
    {
        if (rangedDefinition == null || enemyStats == null)
            return enemyStats != null ? enemyStats.Attack : 0;

        int gameMinutes = GameTimeManager.Instance != null ? GameTimeManager.Instance.GameMinutes : 0;
        return rangedDefinition.baseAttack + rangedDefinition.attackBonusPerMinute * gameMinutes;
    }

    float GetProjectileSpeed()
    {
        return rangedDefinition != null ? rangedDefinition.projectileSpeed : 7f;
    }

    void UpdateS1mpleFacing()
    {
        if (spriteRenderer == null)
            return;

        Vector2 toPlayer = GetDirectionToPlayer();
        if (toPlayer.sqrMagnitude < 0.0001f)
            return;

        Vector2 awayFromPlayer = -toPlayer;
        spriteRenderer.flipX = awayFromPlayer.x < 0f;
    }

    Vector2 GetDirectionToPlayer()
    {
        return (Vector2)player.position - rb.position;
    }

    void CachePlayer()
    {
        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }
}

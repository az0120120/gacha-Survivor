using UnityEngine;

public abstract class StatProjectileWeapon : WeaponBase
{
    [Header("Visual")]
    [SerializeField] Sprite weaponSprite;

    [Header("Projectile")]
    [Tooltip("该武器专用子弹对象池；留空则尝试用 Projectile Prefab 自动创建，或由 WeaponManager 注入")]
    [SerializeField] protected ObjectPool projectilePool;
    [Tooltip("未指定对象池时，用此 Prefab 在运行时创建专用子弹池")]
    [SerializeField] protected GameObject projectilePrefab;
    [Tooltip("可选：发射时覆盖子弹 Sprite（使用不同 Prefab 时可留空）")]
    [SerializeField] Sprite projectileSprite;

    [Header("Combat")]
    [SerializeField] protected float fireInterval = 0.5f;
    [SerializeField] protected float targetRange = 8f;
    [SerializeField] protected int pierceCount = 2;

    [Header("Audio")]
    [SerializeField] AudioClip attackClip;
    [SerializeField] [Range(0f, 1f)] float attackVolume = 0.85f;

    float fireTimer;
    SpriteRenderer weaponRenderer;

    protected abstract ShopWeaponType WeaponIdentity { get; }

    protected override void OnInitialized()
    {
        fireTimer = 0f;
        EnsureProjectilePool();
        ApplyWeaponVisual();
        ConfigureDefaults();
    }

    protected virtual void ConfigureDefaults()
    {
    }

    public bool HasProjectilePool => projectilePool != null;

    public void AssignProjectilePoolIfEmpty(ObjectPool pool)
    {
        if (projectilePool == null && pool != null)
            projectilePool = pool;
    }

    public void SetProjectilePool(ObjectPool pool)
    {
        if (pool != null)
            projectilePool = pool;
    }

    void EnsureProjectilePool()
    {
        if (projectilePool != null)
            return;

        if (projectilePrefab == null)
            return;

        string poolName = $"{WeaponIdentity}ProjectilePool";
        Transform existing = transform.Find(poolName);
        if (existing != null)
        {
            projectilePool = existing.GetComponent<ObjectPool>();
            if (projectilePool != null)
                return;
        }

        var poolObject = new GameObject(poolName);
        poolObject.transform.SetParent(transform, false);
        projectilePool = poolObject.AddComponent<ObjectPool>();
        projectilePool.Setup(projectilePrefab, GetDefaultPrewarmCount());
    }

    protected virtual int GetDefaultPrewarmCount()
    {
        return 20;
    }

    void Update()
    {
        if (stats == null)
            return;

        if (projectilePool == null)
            EnsureProjectilePool();

        if (projectilePool == null)
            return;

        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f)
            return;

        float effectiveInterval = GetEffectiveFireInterval();
        fireTimer = stats.GetEffectiveCooldown(effectiveInterval);

        float effectiveRange = GetEffectiveRange();
        Vector2 direction = GetAttackDirectionTowardTarget(effectiveRange);
        FireProjectile(direction);
    }

    protected float GetEffectiveFireInterval()
    {
        float interval = fireInterval;
        if (weaponManager != null)
            interval *= weaponManager.GetWeaponCooldownMultiplier(WeaponIdentity);

        return interval;
    }

    protected float GetEffectiveRange()
    {
        float range = targetRange;
        if (weaponManager != null)
            range *= weaponManager.GetRangeMultiplier(WeaponIdentity);

        return range;
    }

    protected float GetEffectiveDamageMultiplier()
    {
        float multiplier = damageMultiplier;
        if (weaponManager != null)
            multiplier *= weaponManager.GetMajorUpgradeDamageMultiplier(WeaponIdentity);

        return multiplier;
    }

    void FireProjectile(Vector2 direction)
    {
        var projectileObject = projectilePool.Get(transform.position, Quaternion.identity);
        if (projectileObject == null)
            return;

        var projectile = projectileObject.GetComponent<Projectile>()
            ?? projectileObject.GetComponentInChildren<Projectile>();
        if (projectile == null)
            return;

        projectile.Launch(
            direction,
            stats,
            GetEffectiveDamageMultiplier(),
            knockbackForce,
            pierceCount,
            projectileSprite);

        PlayAttackSound(attackClip, attackVolume);
    }

    void ApplyWeaponVisual()
    {
        if (weaponSprite == null)
            return;

        if (weaponRenderer == null)
            weaponRenderer = GetComponent<SpriteRenderer>();

        if (weaponRenderer != null)
            weaponRenderer.sprite = weaponSprite;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, GetEffectiveRange());
    }
}

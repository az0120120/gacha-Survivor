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

        float effectiveInterval = fireInterval;
        if (weaponManager != null)
            effectiveInterval *= weaponManager.GetWeaponCooldownMultiplier(WeaponIdentity);

        fireTimer = stats.GetEffectiveCooldown(effectiveInterval);

        float effectiveRange = targetRange;
        if (weaponManager != null)
            effectiveRange *= weaponManager.GetRangeMultiplier(WeaponIdentity);

        var target = FindNearestEnemy(effectiveRange);
        Vector2 direction = target != null
            ? (Vector2)target.transform.position - (Vector2)transform.position
            : Vector2.right;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        FireProjectile(direction.normalized);
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
            damageMultiplier,
            knockbackForce,
            pierceCount,
            projectileSprite);
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
        Gizmos.DrawWireSphere(transform.position, targetRange);
    }
}

public class DesertEagleWeapon : StatProjectileWeapon
{
    protected override ShopWeaponType WeaponIdentity => ShopWeaponType.DesertEagle;

    protected override void ConfigureDefaults()
    {
        damageMultiplier = 6f;
        fireInterval = 0.75f;
        pierceCount = 3;
    }

    protected override int GetDefaultPrewarmCount() => 16;
}

public class AkWeapon : StatProjectileWeapon
{
    protected override ShopWeaponType WeaponIdentity => ShopWeaponType.Ak;

    protected override void ConfigureDefaults()
    {
        damageMultiplier = 7f;
        fireInterval = 0.25f;
        pierceCount = 3;
    }

    protected override int GetDefaultPrewarmCount() => 40;
}

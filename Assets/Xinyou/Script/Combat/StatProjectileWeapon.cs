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

    [Header("Muzzle Flash")]
    [SerializeField] Sprite muzzleFlashSprite;
    [SerializeField] Color muzzleFlashColor = new Color(1f, 0.85f, 0.35f, 0.9f);
    [SerializeField] float muzzleFlashDuration = 0.08f;
    [SerializeField] float muzzleFlashScale = 0.35f;
    [SerializeField] float muzzleFlashOffset = 0.35f;
    [Tooltip("贴图默认朝向与发射方向 (+X) 的夹角")]
    [SerializeField] float muzzleFlashFacingOffset;
    [SerializeField] int muzzleFlashSortingOrder = 13;

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

        SpawnMuzzleFlash(direction);
        PlayAttackSound(attackClip, attackVolume);
    }

    void SpawnMuzzleFlash(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = GetDefaultAttackDirection();

        direction = direction.normalized;

        var flashObject = new GameObject("MuzzleFlash");
        flashObject.transform.position = transform.position + (Vector3)(direction * muzzleFlashOffset);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        flashObject.transform.rotation = Quaternion.Euler(0f, 0f, angle + muzzleFlashFacingOffset);
        flashObject.transform.localScale = Vector3.one * muzzleFlashScale;

        var renderer = flashObject.AddComponent<SpriteRenderer>();
        renderer.sprite = muzzleFlashSprite != null ? muzzleFlashSprite : CreateFallbackMuzzleFlashSprite();
        renderer.color = muzzleFlashColor;
        renderer.sortingOrder = muzzleFlashSortingOrder;

        flashObject.AddComponent<TimedSpriteFade>().Begin(muzzleFlashDuration);
    }

    static Sprite CreateFallbackMuzzleFlashSprite()
    {
        const int width = 24;
        const int height = 16;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = x / (float)(width - 1);
                float ny = (y / (float)(height - 1) - 0.5f) * 2f;
                float core = Mathf.Clamp01(1f - nx * 1.15f);
                float edge = Mathf.Clamp01(1f - Mathf.Abs(ny));
                float alpha = core * edge;
                var color = Color.Lerp(new Color(1f, 0.55f, 0.1f, 1f), new Color(1f, 0.95f, 0.45f, 1f), nx);
                color.a = alpha;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0f, 0.5f), 16f);
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

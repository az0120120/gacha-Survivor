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
    [SerializeField] float multiProjectileSpreadAngle = 12f;

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

        float effectiveInterval = ApplyWeaponCooldown(fireInterval, WeaponIdentity);
        fireTimer = effectiveInterval;

        float effectiveRange = ApplyWeaponRange(targetRange, WeaponIdentity);
        Vector2 direction = GetAttackDirectionTowardTarget(effectiveRange);
        FireProjectile(direction);
    }

    protected float GetEffectiveRange()
    {
        return ApplyWeaponRange(targetRange, WeaponIdentity);
    }

    protected float GetSpecialDamageMultiplier()
    {
        return GetWeaponSpecialMultiplier(WeaponIdentity);
    }

    void FireProjectile(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = GetDefaultAttackDirection();
        direction = direction.normalized;

        int projectileCount = GetProjectileCount(WeaponIdentity);
        if (projectileCount <= 1)
        {
            if (!TryFireSingleProjectile(direction))
                return;
        }
        else
        {
            float halfSpread = multiProjectileSpreadAngle * 0.5f;
            float step = multiProjectileSpreadAngle / (projectileCount - 1);
            float startAngle = -halfSpread;
            bool anyFired = false;

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = startAngle + step * i;
                if (TryFireSingleProjectile(RotateDirection(direction, angle)))
                    anyFired = true;
            }

            if (!anyFired)
                return;
        }

        PlayFireFeedback(direction);
    }

    bool TryFireSingleProjectile(Vector2 direction)
    {
        var projectileObject = projectilePool.Get(transform.position, Quaternion.identity);
        if (projectileObject == null)
            return false;

        var projectile = projectileObject.GetComponent<Projectile>()
            ?? projectileObject.GetComponentInChildren<Projectile>();
        if (projectile == null)
            return false;

        var damageContext = GetWeaponDamageContext(WeaponIdentity);
        projectile.Launch(
            direction,
            stats,
            damageMultiplier,
            knockbackForce,
            pierceCount,
            projectileSprite,
            GetSpecialDamageMultiplier(),
            damageContext,
            true);

        return true;
    }

    void PlayFireFeedback(Vector2 direction)
    {
        Vector3 muzzlePosition = GetMuzzleSpawnPosition(direction);
        PlayAttackSound(attackClip, attackVolume);
        SpawnMuzzleFlash(direction, muzzlePosition);
    }

    static Vector2 RotateDirection(Vector2 direction, float angleDegrees)
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        direction = direction.normalized;
        float radians = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos);
    }

    Vector3 GetMuzzleSpawnPosition(Vector2 direction)
    {
        direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : GetDefaultAttackDirection();

        Vector3 spawnOrigin = transform.position;
        float offsetDistance = muzzleFlashOffset;

        var facingRenderer = ResolveFacingSpriteRenderer();
        if (facingRenderer != null)
        {
            spawnOrigin = facingRenderer.bounds.center;
            Vector2 extents = facingRenderer.bounds.extents;
            float visualRadius = Mathf.Max(0.15f, Mathf.Max(extents.x, extents.y));
            offsetDistance = visualRadius * (0.85f + muzzleFlashOffset * 0.25f);
        }

        return spawnOrigin + (Vector3)(direction * offsetDistance);
    }

    SpriteRenderer ResolveFacingSpriteRenderer()
    {
        Transform facingVisual = transform.Find("FacingVisual");
        if (facingVisual != null)
        {
            var renderer = facingVisual.GetComponent<SpriteRenderer>();
            if (renderer != null)
                return renderer;

            renderer = facingVisual.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
                return renderer;
        }

        var bounceVisual = GetComponent<SpriteBounceVisual>();
        if (bounceVisual != null)
            return bounceVisual.GetSpriteRenderer();

        return GetComponentInChildren<SpriteRenderer>();
    }

    void SpawnMuzzleFlash(Vector2 direction, Vector3 spawnPosition)
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = GetDefaultAttackDirection();

        direction = direction.normalized;

        float flashScale = muzzleFlashScale;
        int sortingOrder = muzzleFlashSortingOrder;
        int sortingLayerId = 0;

        var facingRenderer = ResolveFacingSpriteRenderer();
        if (facingRenderer != null)
        {
            Vector2 extents = facingRenderer.bounds.extents;
            float visualRadius = Mathf.Max(0.15f, Mathf.Max(extents.x, extents.y));
            flashScale = Mathf.Max(muzzleFlashScale, muzzleFlashScale * visualRadius * 0.65f);
            sortingOrder = facingRenderer.sortingOrder + 5;
            sortingLayerId = facingRenderer.sortingLayerID;
        }

        var flashObject = new GameObject("MuzzleFlash");
        flashObject.transform.position = spawnPosition;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        flashObject.transform.rotation = Quaternion.Euler(0f, 0f, angle + muzzleFlashFacingOffset);
        flashObject.transform.localScale = Vector3.one * flashScale;

        var renderer = flashObject.AddComponent<SpriteRenderer>();
        renderer.sprite = muzzleFlashSprite != null ? muzzleFlashSprite : GetFallbackMuzzleFlashSprite();
        renderer.color = muzzleFlashColor;
        renderer.sortingLayerID = sortingLayerId;
        renderer.sortingOrder = sortingOrder;

        flashObject.AddComponent<TimedSpriteFade>().Begin(muzzleFlashDuration);
    }

    static Sprite cachedFallbackMuzzleFlashSprite;

    static Sprite GetFallbackMuzzleFlashSprite()
    {
        if (cachedFallbackMuzzleFlashSprite == null)
            cachedFallbackMuzzleFlashSprite = CreateFallbackMuzzleFlashSprite();

        return cachedFallbackMuzzleFlashSprite;
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

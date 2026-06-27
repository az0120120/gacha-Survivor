using UnityEngine;

public class MolotovWeapon : WeaponBase
{
    [Header("Visual")]
    [SerializeField] Sprite weaponSprite;
    [SerializeField] Sprite throwSprite;
    [SerializeField] Sprite fireZoneSprite;

    [Header("Combat")]
    [SerializeField] float throwInterval = 3f;
    [SerializeField] float throwRange = 7f;
    [SerializeField] float throwFlightTime = 0.55f;
    [SerializeField] float zoneRadius = 2f;
    [SerializeField] float zoneDuration = 5f;
    [SerializeField] float zoneTickInterval = 0.5f;

    float throwTimer;

    protected override void OnInitialized()
    {
        damageMultiplier = 4f;
        throwTimer = 0f;
        ApplyWeaponVisual();
    }

    void Update()
    {
        if (stats == null)
            return;

        throwTimer -= Time.deltaTime;
        if (throwTimer > 0f)
            return;

        float effectiveInterval = throwInterval;
        if (weaponManager != null)
            effectiveInterval *= weaponManager.GetWeaponCooldownMultiplier(ShopWeaponType.Molotov);

        throwTimer = stats.GetEffectiveCooldown(effectiveInterval);

        float effectiveRange = throwRange;
        if (weaponManager != null)
            effectiveRange *= weaponManager.GetRangeMultiplier(ShopWeaponType.Molotov);

        Vector2 landingPoint = GetLandingPoint(effectiveRange);
        LaunchThrow(landingPoint);
    }

    Vector2 GetLandingPoint(float range)
    {
        var target = FindNearestEnemy(range);
        if (target != null)
            return target.transform.position;

        return (Vector2)transform.position + Random.insideUnitCircle.normalized * Mathf.Min(range, 3f);
    }

    void LaunchThrow(Vector2 landingPoint)
    {
        var throwObject = new GameObject("MolotovThrow");
        throwObject.transform.position = transform.position;

        if (throwSprite != null)
        {
            var renderer = throwObject.AddComponent<SpriteRenderer>();
            renderer.sprite = throwSprite;
            renderer.sortingOrder = 8;
        }

        throwObject.AddComponent<MolotovFlight>().Begin(
            transform.position,
            landingPoint,
            throwFlightTime,
            OnMolotovLanded);
    }

    void OnMolotovLanded(Vector2 position)
    {
        var zoneObject = new GameObject("MolotovFireZone");
        zoneObject.transform.position = position;

        var zone = zoneObject.AddComponent<MolotovFireZone>();
        zone.Activate(
            zoneRadius,
            zoneDuration,
            zoneTickInterval,
            stats,
            damageMultiplier,
            knockbackForce,
            fireZoneSprite,
            weaponManager,
            ShopWeaponType.Molotov);
    }

    void ApplyWeaponVisual()
    {
        if (weaponSprite == null)
            return;

        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sprite = weaponSprite;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.45f, 0.1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, throwRange);
    }
}

public class MolotovFlight : MonoBehaviour
{
    Vector3 start;
    Vector3 end;
    float duration;
    float elapsed;
    System.Action<Vector2> onComplete;

    public void Begin(Vector3 from, Vector2 to, float flightTime, System.Action<Vector2> callback)
    {
        start = from;
        end = to;
        duration = Mathf.Max(0.05f, flightTime);
        elapsed = 0f;
        onComplete = callback;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);
        transform.position = Vector3.Lerp(start, end, progress);

        if (progress >= 1f)
        {
            onComplete?.Invoke(end);
            Destroy(gameObject);
        }
    }
}

public class MolotovFireZone : MonoBehaviour
{
    readonly Collider2D[] overlapBuffer = new Collider2D[32];

    float radius;
    float remainingDuration;
    float tickTimer;
    float tickInterval;
    CharacterStats attackerStats;
    float attackMultiplier;
    float knockback;
    WeaponManager weaponManager;
    ShopWeaponType weaponType;
    SpriteRenderer zoneRenderer;

    public void Activate(
        float zoneRadius,
        float duration,
        float interval,
        CharacterStats stats,
        float multiplier,
        float knockbackForce,
        Sprite zoneSprite,
        WeaponManager manager,
        ShopWeaponType type)
    {
        radius = zoneRadius;
        remainingDuration = duration;
        tickInterval = interval;
        tickTimer = 0f;
        attackerStats = stats;
        attackMultiplier = multiplier;
        knockback = knockbackForce;
        weaponManager = manager;
        weaponType = type;

        EnsureVisual(zoneSprite);
        ApplyRadiusVisual();
        DamageTick();
    }

    void Update()
    {
        remainingDuration -= Time.deltaTime;
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            tickTimer = tickInterval;
            DamageTick();
        }

        if (remainingDuration <= 0f)
            Destroy(gameObject);
    }

    void DamageTick()
    {
        if (attackerStats == null)
            return;

        float effectiveRadius = radius;
        if (weaponManager != null)
            effectiveRadius *= weaponManager.GetRangeMultiplier(weaponType);

        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        int count = Physics2D.OverlapCircle(transform.position, effectiveRadius, filter, overlapBuffer);
        Vector2 origin = transform.position;

        for (int i = 0; i < count; i++)
        {
            var enemy = overlapBuffer[i].GetComponent<EnemyHealth>();
            if (enemy == null || !enemy.IsAlive)
                continue;

            var enemyStats = enemy.Stats;
            if (enemyStats == null)
                continue;

            DamageResult result = DamageCalculator.CalculateAgainstEnemy(
                attackerStats,
                enemyStats,
                attackMultiplier);

            if (result.FinalDamage > 0)
                enemy.TakeDamage(result.FinalDamage, origin, knockback, result.IsCritical);
        }

        TryHitShopsInRadius(origin, effectiveRadius);
    }

    void TryHitShopsInRadius(Vector2 center, float hitRadius)
    {
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        int count = Physics2D.OverlapCircle(center, hitRadius, filter, overlapBuffer);
        for (int i = 0; i < count; i++)
        {
            var shop = overlapBuffer[i].GetComponent<ShopWorldEntity>();
            if (shop != null && shop.IsAlive)
                shop.TakeDamage(1f);
        }
    }

    void EnsureVisual(Sprite zoneSprite)
    {
        zoneRenderer = GetComponent<SpriteRenderer>();
        if (zoneRenderer == null)
            zoneRenderer = gameObject.AddComponent<SpriteRenderer>();

        zoneRenderer.sprite = zoneSprite;
        zoneRenderer.color = new Color(1f, 0.45f, 0.1f, 0.55f);
        zoneRenderer.sortingOrder = 2;
    }

    void ApplyRadiusVisual()
    {
        if (zoneRenderer == null || zoneRenderer.sprite == null)
        {
            transform.localScale = Vector3.one * radius * 2f;
            return;
        }

        var bounds = zoneRenderer.sprite.bounds.size;
        float maxAxis = Mathf.Max(bounds.x, bounds.y);
        if (maxAxis <= 0.0001f)
            return;

        float scale = radius * 2f / maxAxis;
        transform.localScale = Vector3.one * scale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.35f, 0f, 0.45f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

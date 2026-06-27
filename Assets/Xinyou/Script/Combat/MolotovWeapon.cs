using UnityEngine;

public class MolotovWeapon : WeaponBase
{
    [Header("Visual")]
    [SerializeField] Sprite weaponSprite;
    [SerializeField] Sprite throwSprite;
    [SerializeField] Sprite fireZoneSprite;
    [SerializeField] float throwArcHeight = 2.2f;
    [SerializeField] float throwSpinSpeed = 540f;
    [SerializeField] float throwSpriteScale = 1f;
    [SerializeField] float throwSpriteFacingOffset = -90f;
    [SerializeField] int throwSortingOrder = 10;

    [Header("Audio")]
    [SerializeField] AudioClip impactClip;
    [SerializeField] [Range(0f, 1f)] float impactVolume = 0.85f;

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

        return (Vector2)transform.position + GetDefaultAttackDirection() * Mathf.Min(range, 3f);
    }

    void LaunchThrow(Vector2 landingPoint)
    {
        if (throwSprite == null)
        {
            OnMolotovLanded(landingPoint);
            return;
        }

        var throwObject = new GameObject("MolotovThrow");
        throwObject.transform.position = transform.position;
        throwObject.transform.localScale = Vector3.one * throwSpriteScale;

        var renderer = throwObject.AddComponent<SpriteRenderer>();
        renderer.sprite = throwSprite;
        renderer.sortingOrder = throwSortingOrder;

        throwObject.AddComponent<MolotovFlight>().Begin(
            transform.position,
            landingPoint,
            throwFlightTime,
            throwArcHeight,
            throwSpinSpeed,
            throwSpriteFacingOffset,
            OnMolotovLanded);
    }

    void OnMolotovLanded(Vector2 position)
    {
        PlayImpactSound(position);
        SpawnFireZone(position);
    }

    void PlayImpactSound(Vector2 position)
    {
        if (impactClip == null)
            return;

        AudioSource.PlayClipAtPoint(impactClip, position, impactVolume);
    }

    void SpawnFireZone(Vector2 position)
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
    float arcHeight;
    float spinSpeed;
    float spriteFacingOffset;
    float elapsed;
    System.Action<Vector2> onComplete;

    public void Begin(
        Vector3 from,
        Vector2 to,
        float flightTime,
        float peakHeight,
        float rotationSpeed,
        float facingOffset,
        System.Action<Vector2> callback)
    {
        start = from;
        end = to;
        duration = Mathf.Max(0.05f, flightTime);
        arcHeight = peakHeight;
        spinSpeed = rotationSpeed;
        spriteFacingOffset = facingOffset;
        elapsed = 0f;
        onComplete = callback;

        UpdatePose(0f);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);
        UpdatePose(progress);

        if (progress >= 1f)
        {
            onComplete?.Invoke(end);
            Destroy(gameObject);
        }
    }

    void UpdatePose(float progress)
    {
        Vector3 position = Vector3.Lerp(start, end, progress);
        position.y += arcHeight * 4f * progress * (1f - progress);
        transform.position = position;

        Vector3 flatVelocity = end - start;
        float baseAngle = flatVelocity.sqrMagnitude > 0.0001f
            ? Mathf.Atan2(flatVelocity.y, flatVelocity.x) * Mathf.Rad2Deg
            : 0f;

        transform.rotation = Quaternion.Euler(0f, 0f, baseAngle + spriteFacingOffset + spinSpeed * elapsed);
    }
}

public class MolotovFireZone : MonoBehaviour
{
    readonly Collider2D[] overlapBuffer = new Collider2D[32];

    float radius;
    float totalDuration;
    float remainingDuration;
    float tickTimer;
    float tickInterval;
    CharacterStats attackerStats;
    float attackMultiplier;
    float knockback;
    WeaponManager weaponManager;
    ShopWeaponType weaponType;
    SpriteRenderer zoneRenderer;
    Color zoneBaseColor = new Color(1f, 1f, 1f, 0.85f);

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
        totalDuration = duration;
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
        UpdateVisualAlpha();
        DamageTick();
    }

    void Update()
    {
        remainingDuration -= Time.deltaTime;
        tickTimer -= Time.deltaTime;
        UpdateVisualAlpha();

        if (tickTimer <= 0f)
        {
            tickTimer = tickInterval;
            DamageTick();
        }

        if (remainingDuration <= 0f)
            Destroy(gameObject);
    }

    void UpdateVisualAlpha()
    {
        if (zoneRenderer == null || totalDuration <= 0f)
            return;

        float lifeRatio = Mathf.Clamp01(remainingDuration / totalDuration);
        float fadeIn = Mathf.Clamp01((totalDuration - remainingDuration) / 0.2f);
        float fadeOut = lifeRatio < 0.25f ? lifeRatio / 0.25f : 1f;
        float alpha = zoneBaseColor.a * fadeIn * fadeOut;

        Color color = zoneRenderer.color;
        color.a = alpha;
        zoneRenderer.color = color;

        float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.04f;
        ApplyRadiusVisual(pulse);
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

            var mapProp = overlapBuffer[i].GetComponent<MapDestructibleProp>();
            if (mapProp != null && mapProp.IsActive)
                mapProp.TakeDamage(1f);
        }
    }

    void EnsureVisual(Sprite zoneSprite)
    {
        zoneRenderer = GetComponent<SpriteRenderer>();
        if (zoneRenderer == null)
            zoneRenderer = gameObject.AddComponent<SpriteRenderer>();

        zoneRenderer.sprite = zoneSprite;
        zoneRenderer.color = zoneBaseColor;
        zoneRenderer.sortingOrder = 3;
    }

    void ApplyRadiusVisual(float scaleMultiplier = 1f)
    {
        if (zoneRenderer == null || zoneRenderer.sprite == null)
        {
            transform.localScale = Vector3.one * radius * 2f * scaleMultiplier;
            return;
        }

        var bounds = zoneRenderer.sprite.bounds.size;
        float maxAxis = Mathf.Max(bounds.x, bounds.y);
        if (maxAxis <= 0.0001f)
            return;

        float scale = radius * 2f / maxAxis * scaleMultiplier;
        transform.localScale = Vector3.one * scale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.35f, 0f, 0.45f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

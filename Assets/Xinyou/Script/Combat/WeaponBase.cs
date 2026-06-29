using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float damageMultiplier = 1f;
    [SerializeField] protected float knockbackForce = 4f;

    protected CharacterStats stats;
    protected WeaponManager weaponManager;
    AudioSource weaponAudioSource;

    public void Initialize(CharacterStats characterStats, WeaponManager manager = null)
    {
        stats = characterStats;
        weaponManager = manager != null ? manager : GetComponent<WeaponManager>();
        OnInitialized();
    }

    protected virtual void OnInitialized()
    {
    }

    protected float GetWeaponSpecialMultiplier(ShopWeaponType weaponType)
    {
        if (weaponManager == null)
            return 1f;

        return weaponManager.GetMajorUpgradeDamageMultiplier(weaponType);
    }

    protected float ApplyWeaponRange(float baseRange, ShopWeaponType weaponType)
    {
        float range = baseRange;
        if (weaponManager != null)
            range *= weaponManager.GetRangeMultiplier(weaponType);

        return range;
    }

    protected float ApplyWeaponCooldown(float baseInterval, ShopWeaponType weaponType)
    {
        float interval = baseInterval;
        if (weaponManager != null)
            interval *= weaponManager.GetWeaponCooldownMultiplier(weaponType);

        if (stats != null)
            interval = stats.GetEffectiveCooldown(interval);

        return interval;
    }

    protected WeaponDamageContext GetWeaponDamageContext(ShopWeaponType weaponType)
    {
        if (weaponManager == null || stats == null)
            return default;

        return weaponManager.GetWeaponDamageContext(stats, weaponType);
    }

    protected int GetProjectileCount(ShopWeaponType weaponType)
    {
        if (weaponManager == null)
            return 1;

        return weaponManager.GetProjectileCount(weaponType);
    }

    protected void HitEnemy(EnemyHealth enemy, Vector2 knockbackSource, ShopWeaponType weaponType)
    {
        HitEnemy(
            enemy,
            knockbackSource,
            damageMultiplier,
            GetWeaponSpecialMultiplier(weaponType),
            GetWeaponDamageContext(weaponType));
    }

    protected void HitEnemy(
        EnemyHealth enemy,
        Vector2 knockbackSource,
        float attackMultiplierK,
        float specialMultiplier,
        WeaponDamageContext weaponContext)
    {
        if (enemy == null || !enemy.IsAlive || stats == null)
            return;

        var enemyStats = enemy.Stats;
        if (enemyStats == null)
            return;

        DamageResult result = DamageCalculator.CalculateAgainstEnemy(
            stats,
            enemyStats,
            attackMultiplierK,
            specialMultiplier,
            true,
            weaponContext);

        if (result.FinalDamage <= 0)
            return;

        enemy.TakeDamage(result.FinalDamage, knockbackSource, knockbackForce, result.IsCritical);
    }

    protected void TryHitShop(Collider2D collider)
    {
        if (collider == null)
            return;

        var shop = collider.GetComponent<ShopWorldEntity>();
        if (shop == null || !shop.IsAlive)
            return;

        shop.TakeDamage(1f);
    }

    protected void TryHitMapProp(Collider2D collider)
    {
        TryHitMapProp(collider, transform.position);
    }

    protected void TryHitMapProp(Collider2D collider, Vector2 knockbackSource)
    {
        if (collider == null)
            return;

        var mapProp = collider.GetComponent<MapDestructibleProp>();
        if (mapProp == null || !mapProp.IsAlive)
            return;

        HitMapProp(mapProp, knockbackSource, damageMultiplier, 1f, default);
    }

    protected void HitMapProp(MapDestructibleProp mapProp, Vector2 knockbackSource, ShopWeaponType weaponType)
    {
        HitMapProp(
            mapProp,
            knockbackSource,
            damageMultiplier,
            GetWeaponSpecialMultiplier(weaponType),
            GetWeaponDamageContext(weaponType));
    }

    protected void HitMapProp(
        MapDestructibleProp mapProp,
        Vector2 knockbackSource,
        float attackMultiplierK,
        float specialMultiplier,
        WeaponDamageContext weaponContext)
    {
        if (mapProp == null || !mapProp.IsAlive || stats == null)
            return;

        int damage = MapPropCombatUtility.CalculateWeaponDamage(
            stats,
            attackMultiplierK,
            specialMultiplier,
            weaponContext,
            true);
        mapProp.TakeDamage(damage, knockbackSource, knockbackForce);
    }

    protected void TryHitShopsInRadius(Vector2 center, float radius)
    {
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        var buffer = new Collider2D[32];
        int count = Physics2D.OverlapCircle(center, radius, filter, buffer);

        for (int i = 0; i < count; i++)
        {
            TryHitShop(buffer[i]);
            TryHitMapProp(buffer[i], center);
        }
    }

    protected WeaponTarget FindNearestTarget(float range)
    {
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        var buffer = new Collider2D[32];
        int count = Physics2D.OverlapCircle(transform.position, range, filter, buffer);
        WeaponTarget nearest = default;
        float nearestSqrDistance = float.MaxValue;
        Vector2 origin = transform.position;

        for (int i = 0; i < count; i++)
        {
            var enemy = buffer[i].GetComponent<EnemyHealth>();
            if (enemy != null && enemy.IsAlive)
            {
                float sqrDistance = ((Vector2)buffer[i].transform.position - origin).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearest = new WeaponTarget { Enemy = enemy };
                }

                continue;
            }

            var mapProp = buffer[i].GetComponent<MapDestructibleProp>();
            if (mapProp == null || !mapProp.IsAlive)
                continue;

            float propSqrDistance = ((Vector2)buffer[i].transform.position - origin).sqrMagnitude;
            if (propSqrDistance >= nearestSqrDistance)
                continue;

            nearestSqrDistance = propSqrDistance;
            nearest = new WeaponTarget { MapProp = mapProp };
        }

        return nearest;
    }

    protected int CollectNearestTargets(float range, int maxCount, WeaponTarget[] results)
    {
        if (results == null || maxCount <= 0)
            return 0;

        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        var buffer = new Collider2D[32];
        int count = Physics2D.OverlapCircle(transform.position, range, filter, buffer);
        Vector2 origin = transform.position;
        int resultCount = 0;

        for (int i = 0; i < count && resultCount < maxCount; i++)
        {
            var enemy = buffer[i].GetComponent<EnemyHealth>();
            if (enemy != null && enemy.IsAlive)
            {
                InsertTargetByDistance(results, ref resultCount, maxCount, new WeaponTarget { Enemy = enemy }, origin);
                continue;
            }

            var mapProp = buffer[i].GetComponent<MapDestructibleProp>();
            if (mapProp != null && mapProp.IsAlive)
                InsertTargetByDistance(results, ref resultCount, maxCount, new WeaponTarget { MapProp = mapProp }, origin);
        }

        return resultCount;
    }

    static void InsertTargetByDistance(WeaponTarget[] results, ref int resultCount, int maxCount, WeaponTarget target, Vector2 origin)
    {
        float sqrDistance = (target.Position - origin).sqrMagnitude;
        int insertIndex = resultCount;

        for (int i = 0; i < resultCount; i++)
        {
            if (IsSameTarget(results[i], target))
                return;

            float existingSqrDistance = (results[i].Position - origin).sqrMagnitude;
            if (sqrDistance < existingSqrDistance)
            {
                insertIndex = i;
                break;
            }
        }

        if (resultCount < maxCount)
            resultCount++;

        for (int i = resultCount - 1; i > insertIndex; i--)
            results[i] = results[i - 1];

        results[insertIndex] = target;
    }

    static bool IsSameTarget(WeaponTarget a, WeaponTarget b)
    {
        if (a.Enemy != null && b.Enemy != null)
            return a.Enemy == b.Enemy;

        if (a.MapProp != null && b.MapProp != null)
            return a.MapProp == b.MapProp;

        return false;
    }

    protected EnemyHealth FindNearestEnemy(float range)
    {
        var target = FindNearestTarget(range);
        return target.Enemy;
    }

    protected EnemyHealth FindNearestEnemyOnly(float range)
    {
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        var buffer = new Collider2D[32];
        int count = Physics2D.OverlapCircle(transform.position, range, filter, buffer);
        EnemyHealth nearest = null;
        float nearestSqrDistance = float.MaxValue;
        Vector2 origin = transform.position;

        for (int i = 0; i < count; i++)
        {
            var enemy = buffer[i].GetComponent<EnemyHealth>();
            if (enemy == null || !enemy.IsAlive)
                continue;

            float sqrDistance = ((Vector2)buffer[i].transform.position - origin).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance)
                continue;

            nearestSqrDistance = sqrDistance;
            nearest = enemy;
        }

        return nearest;
    }

    protected Vector2 GetDefaultAttackDirection()
    {
        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
            return movement.LastFacingDirection;

        return Vector2.right;
    }

    protected Vector2 GetAttackDirectionTowardTarget(float range)
    {
        var target = FindNearestTarget(range);
        if (target.IsValid)
        {
            Vector2 direction = target.Position - (Vector2)transform.position;
            if (direction.sqrMagnitude > 0.0001f)
                return direction.normalized;
        }

        return GetDefaultAttackDirection();
    }

    protected void PlayAttackSound(AudioClip clip, float volume = 0.85f)
    {
        if (clip == null)
            return;

        EnsureWeaponAudioSource();
        weaponAudioSource.PlayOneShot(clip, volume);
    }

    void EnsureWeaponAudioSource()
    {
        if (weaponAudioSource != null)
            return;

        weaponAudioSource = GetComponent<AudioSource>();
        if (weaponAudioSource == null)
            weaponAudioSource = gameObject.AddComponent<AudioSource>();

        weaponAudioSource.playOnAwake = false;
        weaponAudioSource.spatialBlend = 0f;
    }
}

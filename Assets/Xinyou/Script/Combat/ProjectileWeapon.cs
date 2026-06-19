using UnityEngine;

public class ProjectileWeapon : WeaponBase
{
    [SerializeField] ObjectPool projectilePool;
    [SerializeField] float fireInterval = 0.5f;
    [SerializeField] float targetRange = 8f;

    float fireTimer;

    protected override void OnInitialized()
    {
        damageMultiplier = 4f;
        fireTimer = 0f;

        if (projectilePool == null)
            projectilePool = GetComponentInChildren<ObjectPool>();
    }

    public void SetProjectilePool(ObjectPool pool)
    {
        projectilePool = pool;
    }

    void Update()
    {
        if (stats == null || projectilePool == null)
            return;

        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f)
            return;

        float effectiveInterval = fireInterval;
        if (weaponManager != null)
            effectiveInterval *= weaponManager.GetWeaponCooldownMultiplier(ShopWeaponType.Projectile);

        fireTimer = stats.GetEffectiveCooldown(effectiveInterval);

        float effectiveRange = targetRange;
        if (weaponManager != null)
            effectiveRange *= weaponManager.GetRangeMultiplier(ShopWeaponType.Projectile);

        var target = FindNearestEnemy(effectiveRange);
        Vector2 baseDirection = target != null
            ? (Vector2)target.transform.position - (Vector2)transform.position
            : Vector2.right;

        if (baseDirection.sqrMagnitude < 0.0001f)
            baseDirection = Vector2.right;

        if (weaponManager != null && weaponManager.GetMajorUpgradeLevel(ShopWeaponType.Projectile) > 0)
        {
            FireProjectile(baseDirection, 0f);
            FireProjectile(baseDirection, -20f);
            FireProjectile(baseDirection, 20f);
        }
        else
        {
            FireProjectile(baseDirection, 0f);
        }
    }

    void FireProjectile(Vector2 direction, float angleOffset)
    {
        Vector2 fireDirection = Rotate(direction.normalized, angleOffset * Mathf.Deg2Rad);
        var projectileObject = projectilePool.Get(transform.position, Quaternion.identity);
        if (projectileObject == null)
            return;

        var projectile = projectileObject.GetComponent<Projectile>();
        if (projectile == null)
            return;

        projectile.Launch(fireDirection, stats, damageMultiplier, knockbackForce);
    }

    static Vector2 Rotate(Vector2 vector, float radians)
    {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, targetRange);
    }
}

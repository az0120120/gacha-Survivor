using UnityEngine;

public class AreaWeapon : WeaponBase
{
    [SerializeField] float tickInterval = 0.5f;
    [SerializeField] float zoneRadius = 2.5f;
    [SerializeField] SpriteRenderer zoneVisual;

    float tickTimer;

    protected override void OnInitialized()
    {
        damageMultiplier = 4f;
        tickTimer = 0f;
        UpdateZoneVisual();
    }

    void Update()
    {
        if (stats == null)
            return;

        tickTimer -= Time.deltaTime;
        if (tickTimer > 0f)
            return;

        float effectiveInterval = tickInterval;
        if (weaponManager != null)
            effectiveInterval *= weaponManager.GetWeaponCooldownMultiplier(ShopWeaponType.Area);

        tickTimer = stats.GetEffectiveCooldown(effectiveInterval);
        DamageEnemiesInZone();
    }

    void DamageEnemiesInZone()
    {
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        var buffer = new Collider2D[32];
        float radius = GetEffectiveRadius();
        int count = Physics2D.OverlapCircle(transform.position, radius, filter, buffer);
        Vector2 origin = transform.position;

        for (int i = 0; i < count; i++)
        {
            var enemy = buffer[i].GetComponent<EnemyHealth>();
            if (enemy == null || !enemy.IsAlive)
                continue;

            HitEnemy(enemy, origin);
        }

        TryHitShopsInRadius(origin, radius);
    }

    float GetEffectiveRadius()
    {
        if (weaponManager == null)
            return zoneRadius;

        return zoneRadius * weaponManager.GetRangeMultiplier(ShopWeaponType.Area);
    }

    void UpdateZoneVisual()
    {
        if (zoneVisual == null)
            return;

        zoneVisual.enabled = true;
        zoneVisual.transform.localScale = Vector3.one * GetEffectiveRadius() * 2f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, GetEffectiveRadius());
    }
}

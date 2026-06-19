using UnityEngine;

public class DirectTargetWeapon : WeaponBase
{
    [SerializeField] float attackInterval = 1f;
    [SerializeField] float targetRange = 6f;

    float attackTimer;

    protected override void OnInitialized()
    {
        damageMultiplier = 6f;
        attackTimer = 0f;
    }

    void Update()
    {
        if (stats == null)
            return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f)
            return;

        float effectiveInterval = attackInterval;
        if (weaponManager != null)
            effectiveInterval *= weaponManager.GetWeaponCooldownMultiplier(ShopWeaponType.DirectTarget);

        attackTimer = stats.GetEffectiveCooldown(effectiveInterval);

        float effectiveRange = targetRange;
        if (weaponManager != null)
            effectiveRange *= weaponManager.GetRangeMultiplier(ShopWeaponType.DirectTarget);

        if (weaponManager != null && weaponManager.GetMajorUpgradeLevel(ShopWeaponType.DirectTarget) > 0)
            DamageAllInRange(effectiveRange);
        else
            DamageNearest(effectiveRange);
    }

    void DamageNearest(float range)
    {
        var target = FindNearestEnemy(range);
        if (target == null)
            return;

        HitEnemy(target, transform.position);
    }

    void DamageAllInRange(float range)
    {
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        var buffer = new Collider2D[32];
        int count = Physics2D.OverlapCircle(transform.position, range, filter, buffer);

        for (int i = 0; i < count; i++)
        {
            var enemy = buffer[i].GetComponent<EnemyHealth>();
            if (enemy == null || !enemy.IsAlive)
                continue;

            HitEnemy(enemy, transform.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, targetRange);
    }
}

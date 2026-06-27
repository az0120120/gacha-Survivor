using UnityEngine;

public class ClawWeapon : WeaponBase
{
    [Header("Visual")]
    [SerializeField] Sprite weaponSprite;
    [SerializeField] Sprite swingSprite;
    [SerializeField] float swingDuration = 0.18f;
    [SerializeField] float swingScale = 1.4f;

    [Header("Combat")]
    [SerializeField] float attackInterval = 1f;
    [SerializeField] float attackRange = 2.8f;
    [SerializeField] float fanHalfAngle = 55f;

    readonly Collider2D[] overlapBuffer = new Collider2D[32];

    float attackTimer;

    protected override void OnInitialized()
    {
        damageMultiplier = 4f;
        attackTimer = 0f;
        ApplyWeaponVisual();
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
            effectiveInterval *= weaponManager.GetWeaponCooldownMultiplier(ShopWeaponType.Claw);

        attackTimer = stats.GetEffectiveCooldown(effectiveInterval);

        float effectiveRange = attackRange;
        if (weaponManager != null)
            effectiveRange *= weaponManager.GetRangeMultiplier(ShopWeaponType.Claw);

        Vector2 origin = transform.position;
        Vector2 facing = GetAttackDirection(effectiveRange);
        PerformFanAttack(origin, facing, effectiveRange);
        SpawnSwingVisual(origin, facing);
    }

    Vector2 GetAttackDirection(float range)
    {
        var target = FindNearestEnemy(range);
        if (target != null)
        {
            Vector2 direction = (Vector2)target.transform.position - (Vector2)transform.position;
            if (direction.sqrMagnitude > 0.0001f)
                return direction.normalized;
        }

        return Vector2.right;
    }

    void PerformFanAttack(Vector2 origin, Vector2 facing, float range)
    {
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        int count = Physics2D.OverlapCircle(origin, range, filter, overlapBuffer);

        for (int i = 0; i < count; i++)
        {
            var enemy = overlapBuffer[i].GetComponent<EnemyHealth>();
            if (enemy == null || !enemy.IsAlive)
                continue;

            Vector2 enemyPosition = overlapBuffer[i].transform.position;
            if (!IsInsideFan(origin, facing, enemyPosition, range, fanHalfAngle))
                continue;

            HitEnemy(enemy, origin);
        }

        TryHitShopsInFan(origin, facing, range);
    }

    void TryHitShopsInFan(Vector2 origin, Vector2 facing, float range)
    {
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        int count = Physics2D.OverlapCircle(origin, range, filter, overlapBuffer);
        for (int i = 0; i < count; i++)
        {
            var shop = overlapBuffer[i].GetComponent<ShopWorldEntity>();
            if (shop == null || !shop.IsAlive)
                continue;

            if (!IsInsideFan(origin, facing, overlapBuffer[i].transform.position, range, fanHalfAngle))
                continue;

            shop.TakeDamage(1f);
        }
    }

    static bool IsInsideFan(Vector2 origin, Vector2 facing, Vector2 point, float range, float halfAngle)
    {
        Vector2 offset = point - origin;
        if (offset.sqrMagnitude > range * range)
            return false;

        if (offset.sqrMagnitude < 0.0001f)
            return true;

        float angle = Vector2.Angle(facing, offset.normalized);
        return angle <= halfAngle;
    }

    void SpawnSwingVisual(Vector2 origin, Vector2 facing)
    {
        if (swingSprite == null)
            return;

        var swingObject = new GameObject("ClawSwing");
        swingObject.transform.position = origin;
        swingObject.transform.localScale = Vector3.one * swingScale;

        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        swingObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        var renderer = swingObject.AddComponent<SpriteRenderer>();
        renderer.sprite = swingSprite;
        renderer.sortingOrder = 11;

        swingObject.AddComponent<TimedSpriteFade>().Begin(swingDuration);
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
        Vector2 origin = transform.position;
        Vector2 facing = GetAttackDirection(attackRange);
        Gizmos.color = new Color(1f, 0.55f, 0.35f, 0.8f);

        DrawFanGizmo(origin, facing, attackRange, fanHalfAngle);
    }

    static void DrawFanGizmo(Vector2 origin, Vector2 facing, float range, float halfAngle)
    {
        const int segments = 12;
        float baseAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        Vector3 previousPoint = origin;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = (baseAngle - halfAngle + t * halfAngle * 2f) * Mathf.Deg2Rad;
            Vector3 point = (Vector3)origin + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * range;

            if (i > 0)
                Gizmos.DrawLine(previousPoint, point);

            previousPoint = point;
        }

        Gizmos.DrawLine((Vector3)origin, previousPoint);
        Gizmos.DrawLine((Vector3)origin, (Vector3)origin + (Vector3)(facing.normalized * range));
    }
}

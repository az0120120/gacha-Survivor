using System.Collections.Generic;
using UnityEngine;

public class ClawWeapon : WeaponBase
{
    [Header("Visual")]
    [SerializeField] Sprite weaponSprite;
    [SerializeField] Sprite swingSprite;
    [SerializeField] float swingDuration = 0.22f;
    [Tooltip("在按攻击范围自动缩放基础上的额外倍率")]
    [SerializeField] float swingScale = 1f;
    [SerializeField] float swingSpriteFacingOffset = -90f;
    [SerializeField] int swingSortingOrder = 11;

    [Header("Combat")]
    [SerializeField] float attackInterval = 1f;
    [SerializeField] float attackRange = 2.8f;
    [SerializeField] float fanHalfAngle = 55f;

    [Header("Audio")]
    [SerializeField] AudioClip attackClip;
    [SerializeField] [Range(0f, 1f)] float attackVolume = 0.85f;

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
        Vector2 facing = GetAttackDirectionTowardTarget(effectiveRange);
        SpawnSwingHitbox(origin, facing, effectiveRange);
    }

    internal void HandleSwingHit(EnemyHealth enemy, Vector2 knockbackSource)
    {
        HitEnemy(enemy, knockbackSource);
    }

    internal void HandleSwingShopHit(Collider2D collider)
    {
        TryHitShop(collider);
    }

    void SpawnSwingHitbox(Vector2 origin, Vector2 facing, float range)
    {
        Sprite effectSprite = swingSprite != null ? swingSprite : weaponSprite;
        if (effectSprite == null)
            return;

        PlayAttackSound(attackClip, attackVolume);

        var swingObject = new GameObject("ClawSwingHitbox");
        var renderer = swingObject.AddComponent<SpriteRenderer>();
        renderer.sprite = effectSprite;
        renderer.sortingOrder = swingSortingOrder;

        float visualScale = ClawSwingMotion.CalculateScaleForRange(effectSprite, range, swingScale);

        swingObject.AddComponent<ClawSwingMotion>().Begin(
            this,
            origin,
            facing,
            range,
            fanHalfAngle,
            swingDuration,
            visualScale,
            swingSpriteFacingOffset,
            effectSprite);
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
        Vector2 facing = GetAttackDirectionTowardTarget(attackRange);
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

[RequireComponent(typeof(SpriteRenderer))]
public class ClawSwingMotion : MonoBehaviour
{
    ClawWeapon owner;
    Vector2 origin;
    float range;
    float halfAngle;
    float baseAngleDeg;
    float duration;
    float elapsed;
    float visualScale;
    float spriteFacingOffset;
    SpriteRenderer spriteRenderer;
    Color startColor;
    BoxCollider2D hitbox;
    Rigidbody2D rb;
    readonly HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();
    readonly HashSet<ShopWorldEntity> hitShops = new HashSet<ShopWorldEntity>();
    readonly HashSet<EnemyProjectile> blockedProjectiles = new HashSet<EnemyProjectile>();

    public static float CalculateScaleForRange(Sprite sprite, float attackRange, float scaleMultiplier = 1f)
    {
        if (attackRange <= 0f)
            return scaleMultiplier;

        if (sprite == null)
            return attackRange * 2f * scaleMultiplier;

        Vector2 bounds = sprite.bounds.size;
        float maxAxis = Mathf.Max(bounds.x, bounds.y);
        if (maxAxis <= 0.0001f)
            return attackRange * 2f * scaleMultiplier;

        return attackRange * 2f / maxAxis * scaleMultiplier;
    }

    public void Begin(
        ClawWeapon clawWeapon,
        Vector2 attackOrigin,
        Vector2 facing,
        float attackRange,
        float fanHalfAngle,
        float swingDuration,
        float scale,
        float facingOffset,
        Sprite swingSprite)
    {
        owner = clawWeapon;
        origin = attackOrigin;
        range = attackRange;
        halfAngle = fanHalfAngle;
        duration = Mathf.Max(0.05f, swingDuration);
        visualScale = scale;
        spriteFacingOffset = facingOffset;
        elapsed = 0f;
        hitEnemies.Clear();
        hitShops.Clear();
        blockedProjectiles.Clear();

        if (facing.sqrMagnitude < 0.0001f)
            facing = Vector2.right;

        baseAngleDeg = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            startColor = spriteRenderer.color;

        transform.localScale = Vector3.one * visualScale;
        SetupHitbox(swingSprite);
        ApplyPose(0f);
        SyncHitboxTransform();
    }

    void SetupHitbox(Sprite swingSprite)
    {
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        hitbox = gameObject.AddComponent<BoxCollider2D>();
        hitbox.isTrigger = true;

        if (swingSprite != null)
            hitbox.size = swingSprite.bounds.size;
    }

    void SyncHitboxTransform()
    {
        if (hitbox == null || spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        hitbox.offset = spriteRenderer.sprite.bounds.center;
        hitbox.size = spriteRenderer.sprite.bounds.size;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);
        ApplyPose(progress);

        if (spriteRenderer != null)
        {
            Color color = startColor;
            color.a = startColor.a * (1f - progress);
            spriteRenderer.color = color;
        }

        if (progress >= 1f)
            Destroy(gameObject);
    }

    void ApplyPose(float progress)
    {
        float sweepAngleDeg = baseAngleDeg - halfAngle + progress * halfAngle * 2f;
        float sweepAngleRad = sweepAngleDeg * Mathf.Deg2Rad;

        Vector2 direction = new Vector2(Mathf.Cos(sweepAngleRad), Mathf.Sin(sweepAngleRad));
        transform.position = origin + direction * (range * 0.5f);
        transform.rotation = Quaternion.Euler(0f, 0f, sweepAngleDeg + spriteFacingOffset);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ProcessHit(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        ProcessHit(other);
    }

    void ProcessHit(Collider2D other)
    {
        if (owner == null)
            return;

        var enemyProjectile = other.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            if (blockedProjectiles.Add(enemyProjectile))
                enemyProjectile.Block();
            return;
        }

        var mapProp = other.GetComponent<MapDestructibleProp>();
        if (mapProp != null && mapProp.IsActive)
        {
            mapProp.TakeDamage(1f);
            return;
        }

        var shop = other.GetComponent<ShopWorldEntity>();
        if (shop != null && shop.IsAlive)
        {
            if (hitShops.Add(shop))
                owner.HandleSwingShopHit(other);
            return;
        }

        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy == null || !enemy.IsAlive)
            return;

        if (hitEnemies.Contains(enemy))
            return;

        hitEnemies.Add(enemy);
        owner.HandleSwingHit(enemy, origin);
    }
}

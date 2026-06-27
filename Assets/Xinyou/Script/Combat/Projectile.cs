using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] float speed = 12f;
    [SerializeField] float maxLifetime = 3f;
    [Tooltip("子弹贴图默认朝向与右方向 (+X) 的夹角。竖图填 -90，横图填 0")]
    [SerializeField] float spriteFacingOffset;

    CharacterStats attackerStats;
    float attackMultiplierK;
    float knockbackForce;
    Vector2 direction;
    float lifetime;
    int remainingPenetrations;
    ObjectPool pool;
    Sprite defaultSprite;
    readonly HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

    void Awake()
    {
        CacheDefaultSprite();
    }

    public void BindPool(ObjectPool objectPool)
    {
        pool = objectPool;
    }

    public void Launch(
        Vector2 launchDirection,
        CharacterStats stats,
        float multiplierK,
        float projectileKnockback,
        int pierceCount = -1,
        Sprite spriteOverride = null)
    {
        attackerStats = stats;
        attackMultiplierK = multiplierK;
        knockbackForce = projectileKnockback;
        direction = launchDirection.sqrMagnitude > 0.0001f ? launchDirection.normalized : Vector2.right;
        lifetime = maxLifetime;
        remainingPenetrations = pierceCount >= 0
            ? pierceCount
            : stats != null ? stats.BulletPenetration : 1;
        hitEnemies.Clear();

        ApplySprite(spriteOverride);
        ApplyFacingRotation();
    }

    public void OnGetFromPool()
    {
        lifetime = maxLifetime;
        hitEnemies.Clear();
        remainingPenetrations = 0;
        direction = Vector2.zero;
    }

    public void OnReturnToPool()
    {
        attackerStats = null;
        direction = Vector2.zero;
        attackMultiplierK = 0f;
        knockbackForce = 0f;
        lifetime = 0f;
        remainingPenetrations = 0;
        hitEnemies.Clear();
        transform.rotation = Quaternion.identity;
        RestoreDefaultSprite();
    }

    void Update()
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
            Release();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var mapProp = other.GetComponent<MapDestructibleProp>();
        if (mapProp != null && mapProp.IsActive)
        {
            mapProp.TakeDamage(1f);
            return;
        }

        var shop = other.GetComponent<ShopWorldEntity>();
        if (shop != null && shop.IsAlive)
        {
            shop.TakeDamage(1f);
            Release();
            return;
        }

        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy == null || !enemy.IsAlive || attackerStats == null)
            return;

        if (hitEnemies.Contains(enemy))
            return;

        var enemyStats = enemy.Stats;
        if (enemyStats == null)
            return;

        DamageResult result = DamageCalculator.CalculateAgainstEnemy(
            attackerStats,
            enemyStats,
            attackMultiplierK);

        if (result.FinalDamage > 0)
            enemy.TakeDamage(result.FinalDamage, transform.position, knockbackForce, result.IsCritical);

        hitEnemies.Add(enemy);
        remainingPenetrations--;

        if (remainingPenetrations <= 0)
            Release();
    }

    void ApplyFacingRotation()
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + spriteFacingOffset);
    }

    void ApplySprite(Sprite spriteOverride)
    {
        if (spriteOverride == null)
            return;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.sprite = spriteOverride;
    }

    void CacheDefaultSprite()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            defaultSprite = spriteRenderer.sprite;
    }

    void RestoreDefaultSprite()
    {
        if (defaultSprite == null)
            return;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.sprite = defaultSprite;
    }

    void Release()
    {
        if (pool != null)
            pool.Release(gameObject);
        else
            Destroy(gameObject);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] float speed = 12f;
    [SerializeField] float maxLifetime = 3f;

    CharacterStats attackerStats;
    float attackMultiplierK;
    float knockbackForce;
    Vector2 direction;
    float lifetime;
    int remainingPenetrations;
    ObjectPool pool;
    readonly HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

    public void BindPool(ObjectPool objectPool)
    {
        pool = objectPool;
    }

    public void Launch(
        Vector2 launchDirection,
        CharacterStats stats,
        float multiplierK,
        float projectileKnockback)
    {
        attackerStats = stats;
        attackMultiplierK = multiplierK;
        knockbackForce = projectileKnockback;
        direction = launchDirection.sqrMagnitude > 0.0001f ? launchDirection.normalized : Vector2.right;
        lifetime = maxLifetime;
        remainingPenetrations = stats != null ? stats.BulletPenetration : 1;
        hitEnemies.Clear();

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void OnGetFromPool()
    {
        lifetime = maxLifetime;
        hitEnemies.Clear();
        remainingPenetrations = 0;
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
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
            Release();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
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

    void Release()
    {
        if (pool != null)
            pool.Release(gameObject);
        else
            Destroy(gameObject);
    }
}

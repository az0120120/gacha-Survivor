using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float damageMultiplier = 1f;
    [SerializeField] protected float knockbackForce = 4f;

    protected CharacterStats stats;
    protected WeaponManager weaponManager;

    public void Initialize(CharacterStats characterStats, WeaponManager manager = null)
    {
        stats = characterStats;
        weaponManager = manager != null ? manager : GetComponent<WeaponManager>();
        OnInitialized();
    }

    protected virtual void OnInitialized()
    {
    }

    protected void HitEnemy(EnemyHealth enemy, Vector2 knockbackSource, float specialMultiplier = 1f)
    {
        if (enemy == null || !enemy.IsAlive || stats == null)
            return;

        var enemyStats = enemy.Stats;
        if (enemyStats == null)
            return;

        DamageResult result = DamageCalculator.CalculateAgainstEnemy(
            stats,
            enemyStats,
            damageMultiplier,
            specialMultiplier);

        if (result.FinalDamage <= 0)
            return;

        enemy.TakeDamage(result.FinalDamage, knockbackSource, knockbackForce);
    }

    protected EnemyHealth FindNearestEnemy(float range)
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
}

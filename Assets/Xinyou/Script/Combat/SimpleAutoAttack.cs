using UnityEngine;

public class SimpleAutoAttack : MonoBehaviour
{
    [SerializeField] float attackRange = 3f;
    [SerializeField] float damage = 10f;
    [SerializeField] float cooldown = 0.5f;

    readonly Collider2D[] overlapBuffer = new Collider2D[32];
    float cooldownTimer;

    void Update()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f)
            return;

        var nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null)
            return;

        nearestEnemy.TakeDamage(damage);
        cooldownTimer = cooldown;
    }

    EnemyHealth FindNearestEnemy()
    {
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        int count = Physics2D.OverlapCircle(transform.position, attackRange, filter, overlapBuffer);
        EnemyHealth nearest = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var enemy = overlapBuffer[i].GetComponent<EnemyHealth>();
            if (enemy == null || !enemy.IsAlive)
                continue;

            float sqrDistance = ((Vector2)overlapBuffer[i].transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance)
                continue;

            nearestSqrDistance = sqrDistance;
            nearest = enemy;
        }

        return nearest;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

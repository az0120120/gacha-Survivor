using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyAI : MonoBehaviour, IPoolable
{
    enum HarasserState
    {
        Paused,
        Dashing,
        Recovering
    }

    [SerializeField] float knockbackDecay = 10f;

    Transform player;
    Rigidbody2D rb;
    EnemyStats enemyStats;
    EnemyArchetypeDefinition definition;
    Vector2 knockbackVelocity;

    float meleeMoveSpeed = 2.8f;
    float rangedMoveSpeed = 1.6f;
    float preferredRange = 7f;
    float minRange = 4.5f;
    float shootInterval = 1.4f;
    float projectileSpeed = 7f;
    float harasserCruiseSpeed = 1.4f;
    float harasserDashSpeed = 7f;
    float harasserPauseDuration = 0.55f;
    float harasserDashDuration = 0.4f;
    float harasserRecoverDuration = 0.75f;

    HarasserState harasserState = HarasserState.Paused;
    float harasserStateTimer;
    float shootTimer;
    Vector2 dashDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyStats = GetComponent<EnemyStats>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Start()
    {
        CachePlayer();
    }

    public void ApplyArchetypeConfig(EnemyArchetypeDefinition config)
    {
        definition = config;
        ApplyConfigValues();
        shootTimer = shootInterval * 0.5f;
        ResetHarasserState();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        knockbackVelocity += direction.normalized * force;
    }

    public void OnGetFromPool()
    {
        knockbackVelocity = Vector2.zero;
        definition = enemyStats != null ? enemyStats.Definition : null;
        ApplyConfigValues();
        shootTimer = shootInterval * 0.5f;
        ResetHarasserState();

        if (player == null)
            CachePlayer();

        enemyStats?.ApplyArchetypeVisual();
    }

    public void OnReturnToPool()
    {
        knockbackVelocity = Vector2.zero;
        rb.velocity = Vector2.zero;
        harasserState = HarasserState.Paused;
        harasserStateTimer = 0f;
    }

    void ApplyConfigValues()
    {
        if (definition == null)
            return;

        meleeMoveSpeed = definition.meleeMoveSpeed;
        rangedMoveSpeed = definition.rangedMoveSpeed;
        preferredRange = definition.preferredRange;
        minRange = definition.minRange;
        shootInterval = definition.shootInterval;
        projectileSpeed = definition.projectileSpeed;
        harasserCruiseSpeed = definition.harasserCruiseSpeed;
        harasserDashSpeed = definition.harasserDashSpeed;
        harasserPauseDuration = definition.harasserPauseDuration;
        harasserDashDuration = definition.harasserDashDuration;
        harasserRecoverDuration = definition.harasserRecoverDuration;
    }

    void FixedUpdate()
    {
        if (player == null)
            return;

        if (MapPropStatusEffects.AreEnemiesFrozen)
        {
            rb.velocity = knockbackVelocity;
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, knockbackDecay * Time.fixedDeltaTime);
            return;
        }

        Vector2 chaseVelocity = enemyStats != null
            ? ComputeBehaviorVelocity()
            : Vector2.zero;

        rb.velocity = chaseVelocity + knockbackVelocity;
        knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, knockbackDecay * Time.fixedDeltaTime);
    }

    Vector2 ComputeBehaviorVelocity()
    {
        switch (enemyStats.Archetype)
        {
            case EnemyArchetype.MeleeRush:
                return ComputeChaseVelocity(meleeMoveSpeed);
            case EnemyArchetype.RangedShooter:
                return ComputeRangedVelocity();
            case EnemyArchetype.Harasser:
                return ComputeHarasserVelocity();
            default:
                return ComputeChaseVelocity(meleeMoveSpeed);
        }
    }

    Vector2 ComputeChaseVelocity(float speed)
    {
        Vector2 direction = GetDirectionToPlayer();
        return direction.sqrMagnitude > 0.0001f ? direction.normalized * speed : Vector2.zero;
    }

    Vector2 ComputeRangedVelocity()
    {
        Vector2 offset = GetDirectionToPlayer();
        float distance = offset.magnitude;
        if (distance < 0.0001f)
            return Vector2.zero;

        Vector2 direction = offset / distance;
        shootTimer -= Time.fixedDeltaTime;

        if (distance < minRange)
            return -direction * rangedMoveSpeed;

        if (distance <= preferredRange)
        {
            TryShoot(direction);
            return Vector2.zero;
        }

        return direction * rangedMoveSpeed;
    }

    void TryShoot(Vector2 direction)
    {
        if (shootTimer > 0f)
            return;

        shootTimer = shootInterval;

        if (EnemyProjectilePool.Instance == null || enemyStats == null)
            return;

        EnemyProjectilePool.Instance.Fire(
            transform.position,
            direction,
            enemyStats.Attack,
            projectileSpeed,
            definition);
    }

    Vector2 ComputeHarasserVelocity()
    {
        harasserStateTimer -= Time.fixedDeltaTime;
        Vector2 toPlayer = GetDirectionToPlayer();

        switch (harasserState)
        {
            case HarasserState.Paused:
                if (harasserStateTimer <= 0f)
                    BeginHarasserDash(toPlayer);
                return Vector2.zero;

            case HarasserState.Dashing:
                if (harasserStateTimer <= 0f)
                    EnterHarasserRecover();
                return dashDirection * harasserDashSpeed;

            case HarasserState.Recovering:
                if (harasserStateTimer <= 0f)
                    EnterHarasserPause();
                return toPlayer.sqrMagnitude > 0.0001f
                    ? toPlayer.normalized * harasserCruiseSpeed
                    : Vector2.zero;
        }

        return Vector2.zero;
    }

    void ResetHarasserState()
    {
        harasserState = HarasserState.Paused;
        harasserStateTimer = harasserPauseDuration;
        dashDirection = Vector2.up;
    }

    void BeginHarasserDash(Vector2 toPlayer)
    {
        harasserState = HarasserState.Dashing;
        harasserStateTimer = harasserDashDuration;
        dashDirection = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : Vector2.up;
    }

    void EnterHarasserRecover()
    {
        harasserState = HarasserState.Recovering;
        harasserStateTimer = harasserRecoverDuration;
    }

    void EnterHarasserPause()
    {
        harasserState = HarasserState.Paused;
        harasserStateTimer = harasserPauseDuration;
    }

    Vector2 GetDirectionToPlayer()
    {
        return (Vector2)player.position - rb.position;
    }

    void CachePlayer()
    {
        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }
}

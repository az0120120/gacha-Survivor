using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MapDestructibleProp : MonoBehaviour, IDamageable
{
    static readonly Color DefaultColor = new Color(0.72f, 0.55f, 0.38f, 0.95f);

    MapPropGridSpawner spawner;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    int maxHealth;
    int currentHealth;
    bool isActive = true;
    bool wanderEnabled;
    float wanderRadius;
    float wanderSpeed;
    float wanderRetargetInterval;
    Vector2 wanderOrigin;
    Vector2 wanderTarget;
    float wanderRetargetTimer;

    public bool IsActive => isActive && IsAlive;
    public bool IsAlive => isActive && currentHealth > 0;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public Vector2 WorldPosition => transform.position;

    public void Initialize(
        MapPropGridSpawner owner,
        Sprite sprite,
        float colliderRadius,
        int sortingOrder,
        int health,
        float spriteScale,
        bool enableWander,
        float roamRadius,
        float roamSpeed,
        float roamRetargetInterval)
    {
        spawner = owner;
        isActive = true;
        maxHealth = Mathf.Max(1, health);
        currentHealth = maxHealth;
        wanderEnabled = enableWander;
        wanderRadius = Mathf.Max(0.05f, roamRadius);
        wanderSpeed = Mathf.Max(0f, roamSpeed);
        wanderRetargetInterval = Mathf.Max(0.25f, roamRetargetInterval);
        wanderOrigin = transform.position;
        wanderRetargetTimer = Random.Range(0f, wanderRetargetInterval);

        float scale = Mathf.Max(0.01f, spriteScale);
        EnsureComponents(colliderRadius, sortingOrder);
        transform.localScale = Vector3.one * scale;

        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = sprite != null ? sprite : CreatePlaceholderSprite();
        spriteRenderer.color = sprite != null ? Color.white : DefaultColor;

        if (wanderEnabled)
            PickWanderTarget();
    }

    void EnsureComponents(float colliderRadius, int sortingOrder)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = sortingOrder;
        }

        var collider = GetComponent<CircleCollider2D>();
        collider.isTrigger = false;
        collider.radius = colliderRadius;

        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.mass = 0.8f;
        rb.drag = 4f;
        rb.angularDrag = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void FixedUpdate()
    {
        if (!IsAlive || !wanderEnabled || rb == null || wanderSpeed <= 0f)
            return;

        if (MapPropStatusEffects.AreEnemiesFrozen)
            return;

        wanderRetargetTimer -= Time.fixedDeltaTime;
        Vector2 position = rb.position;
        Vector2 offset = position - wanderOrigin;

        if (wanderRetargetTimer <= 0f || offset.sqrMagnitude > wanderRadius * wanderRadius)
            PickWanderTarget();

        Vector2 toTarget = wanderTarget - position;
        if (toTarget.sqrMagnitude < 0.04f)
            PickWanderTarget();

        Vector2 desiredVelocity = toTarget.sqrMagnitude > 0.0001f
            ? toTarget.normalized * wanderSpeed
            : Vector2.zero;

        rb.velocity = Vector2.Lerp(rb.velocity, desiredVelocity, Time.fixedDeltaTime * 4f);
    }

    void PickWanderTarget()
    {
        wanderRetargetTimer = wanderRetargetInterval + Random.Range(-0.35f, 0.35f);

        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = wanderOrigin + randomOffset;
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(StatMath.FloorToInt(damage), transform.position, 0f);
    }

    public void TakeDamage(int damage, Vector2 knockbackSource, float knockbackForce)
    {
        if (!IsAlive || damage <= 0)
            return;

        currentHealth -= damage;
        ApplyKnockback(knockbackSource, knockbackForce);

        if (currentHealth <= 0)
            Break();
    }

    public void ApplyKnockback(Vector2 knockbackSource, float knockbackForce)
    {
        if (rb == null || knockbackForce <= 0f)
            return;

        Vector2 direction = (Vector2)transform.position - knockbackSource;
        if (direction.sqrMagnitude < 0.0001f)
            direction = Random.insideUnitCircle;

        rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);
    }

    public void Break()
    {
        if (!isActive)
            return;

        isActive = false;
        currentHealth = 0;

        if (spawner != null)
        {
            spawner.PlayBreakSound(transform.position);
            spawner.SpawnPickup(transform.position);
        }

        Destroy(gameObject);
    }

    static Sprite CreatePlaceholderSprite()
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}

public static class MapPropCombatUtility
{
    public static int CalculateWeaponDamage(
        CharacterStats stats,
        float attackMultiplierK,
        float specialMultiplier = 1f,
        WeaponDamageContext weaponContext = default,
        bool useWeaponDamageContext = false)
    {
        if (stats == null)
            return 1;

        int elementalAttack = useWeaponDamageContext
            ? weaponContext.ElementalAttack
            : stats.ElementalAttack;

        float attackPower = attackMultiplierK
                            * stats.Attack
                            * (1f + stats.AttackDamageBonus);

        float elementalPower = attackMultiplierK
                               * elementalAttack
                               * (1f + stats.ElementalDamageBonus);

        float damage = (attackPower + elementalPower) * specialMultiplier;
        damage *= MapPropStatusEffects.OutgoingDamageMultiplier;
        return Mathf.Max(1, StatMath.FloorToInt(damage));
    }
}

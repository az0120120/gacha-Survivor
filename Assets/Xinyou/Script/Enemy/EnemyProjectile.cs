using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IPoolable
{
    [SerializeField] float maxLifetime = 4f;

    Vector2 direction;
    float speed;
    int damage;
    float lifetime;
    EnemyProjectilePool pool;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void BindPool(EnemyProjectilePool projectilePool)
    {
        pool = projectilePool;
    }

    public void Launch(
        Vector2 launchDirection,
        int projectileDamage,
        float projectileSpeed,
        EnemyArchetypeDefinition visualDefinition)
    {
        damage = projectileDamage;
        speed = projectileSpeed;
        direction = launchDirection.sqrMagnitude > 0.0001f ? launchDirection.normalized : Vector2.right;
        lifetime = maxLifetime;
        ApplyVisual(visualDefinition);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void ApplyVisual(EnemyArchetypeDefinition visualDefinition)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            return;

        if (visualDefinition != null && visualDefinition.projectileSprite != null)
            spriteRenderer.sprite = visualDefinition.projectileSprite;

        spriteRenderer.color = visualDefinition != null
            ? visualDefinition.projectileColor
            : new Color(1f, 0.35f, 0.35f, 0.95f);

        float scale = visualDefinition != null ? visualDefinition.projectileScale : 1f;
        transform.localScale = Vector3.one * scale;
    }

    public void OnGetFromPool()
    {
        lifetime = maxLifetime;
    }

    public void OnReturnToPool()
    {
        direction = Vector2.zero;
        speed = 0f;
        damage = 0;
        lifetime = 0f;
        transform.localScale = Vector3.one;
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
        if (!other.CompareTag("Player"))
            return;

        var playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsAlive && damage > 0)
            playerHealth.TakeDamage(damage);

        Release();
    }

    void Release()
    {
        if (pool != null)
            pool.Release(this);
        else
            Destroy(gameObject);
    }
}

using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("GachaSurvivor/Enemy Projectile Pool")]
public class EnemyProjectilePool : MonoBehaviour
{
    public static EnemyProjectilePool Instance { get; private set; }

    [SerializeField] int prewarmCount = 40;

    readonly Queue<EnemyProjectile> available = new Queue<EnemyProjectile>();
    readonly HashSet<EnemyProjectile> active = new HashSet<EnemyProjectile>();
    Transform poolRoot;
    Sprite bulletSprite;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        var rootObject = new GameObject("EnemyProjectileRoot");
        poolRoot = rootObject.transform;
        poolRoot.SetParent(transform, false);

        bulletSprite = CreateBulletSprite();
        Prewarm();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Fire(
        Vector2 position,
        Vector2 direction,
        int damage,
        float speed,
        EnemyArchetypeDefinition visualDefinition)
    {
        EnemyProjectile projectile = available.Count > 0
            ? available.Dequeue()
            : CreateProjectile();

        projectile.transform.SetParent(null, false);
        projectile.transform.position = position;
        projectile.gameObject.SetActive(true);
        projectile.OnGetFromPool();
        projectile.Launch(direction, damage, speed, visualDefinition);
        active.Add(projectile);
    }

    public void Release(EnemyProjectile projectile)
    {
        if (projectile == null || !active.Contains(projectile))
            return;

        projectile.OnReturnToPool();
        projectile.gameObject.SetActive(false);
        projectile.transform.SetParent(poolRoot, false);
        active.Remove(projectile);
        available.Enqueue(projectile);
    }

    void Prewarm()
    {
        for (int i = 0; i < prewarmCount; i++)
        {
            EnemyProjectile projectile = CreateProjectile();
            projectile.gameObject.SetActive(false);
            available.Enqueue(projectile);
        }
    }

    EnemyProjectile CreateProjectile()
    {
        var projectileObject = new GameObject("EnemyProjectile");
        projectileObject.transform.SetParent(poolRoot, false);

        var spriteRenderer = projectileObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = bulletSprite;
        spriteRenderer.color = new Color(1f, 0.35f, 0.35f, 0.95f);
        spriteRenderer.sortingOrder = 4;

        var collider = projectileObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.25f;

        var rb = projectileObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        var projectile = projectileObject.AddComponent<EnemyProjectile>();
        projectile.BindPool(this);
        return projectile;
    }

    static Sprite CreateBulletSprite()
    {
        const int size = 8;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
                texture.SetPixel(x, y, Color.white);
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}

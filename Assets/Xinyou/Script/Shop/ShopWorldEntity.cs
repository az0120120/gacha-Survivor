using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[AddComponentMenu("GachaSurvivor/Shop World Entity")]
public class ShopWorldEntity : MonoBehaviour, IDamageable
{
    static readonly List<ShopWorldEntity> activeEntities = new List<ShopWorldEntity>();

    public static IReadOnlyList<ShopWorldEntity> ActiveEntities => activeEntities;

    [SerializeField] ShopSizeType shopSize = ShopSizeType.Small;
    [SerializeField] SpriteRenderer spriteRenderer;

    ShopWorldVisualSettings visualSettings;
    bool isActive = true;

    public ShopSizeType ShopSize => shopSize;
    public bool IsAlive => isActive;

    public void Initialize(ShopSizeType size, ShopWorldVisualSettings visuals = null)
    {
        shopSize = size;
        visualSettings = visuals;
        EnsureComponents();
        ApplyVisual();
    }

    void OnEnable()
    {
        if (!activeEntities.Contains(this))
            activeEntities.Add(this);
    }

    void OnDisable()
    {
        activeEntities.Remove(this);
    }

    void Awake()
    {
        EnsureComponents();
        ApplyVisual();
    }

    void EnsureComponents()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = visualSettings != null ? visualSettings.sortingOrder : 8;
        }

        var collider = GetComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = visualSettings != null ? visualSettings.colliderRadius : 0.75f;

        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
    }

    void ApplyVisual()
    {
        if (spriteRenderer == null)
            return;

        Sprite sprite = visualSettings != null ? visualSettings.GetSprite(shopSize) : null;
        spriteRenderer.sprite = sprite != null ? sprite : CreatePlaceholderSprite();
        spriteRenderer.color = visualSettings != null
            ? visualSettings.GetColor(shopSize)
            : shopSize == ShopSizeType.Large
                ? new Color(1f, 0.82f, 0.2f, 0.95f)
                : new Color(0.3f, 0.75f, 1f, 0.95f);
        spriteRenderer.sortingOrder = visualSettings != null ? visualSettings.sortingOrder : 8;

        float scale = visualSettings != null ? visualSettings.GetScale(shopSize) : 1f;
        if (visualSettings == null && shopSize == ShopSizeType.Large)
            scale = 1.4f;

        transform.localScale = Vector3.one * scale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || !other.CompareTag("Player"))
            return;

        TryOpenShop();
    }

    void TryOpenShop()
    {
        if (!isActive)
            return;

        isActive = false;

        if (ShopManager.Instance != null)
            ShopManager.Instance.OpenWorldShop(shopSize, this);

        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
    }

    static Sprite CreatePlaceholderSprite()
    {
        const int width = 32;
        const int height = 28;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool roof = y >= height - 8 && x >= 4 && x <= width - 5;
                bool body = y >= 6 && y < height - 8 && x >= 6 && x <= width - 7;
                bool door = y >= 6 && y <= 14 && x >= 12 && x <= 19;
                bool alpha = roof || (body && !door);
                texture.SetPixel(x, y, alpha ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.15f), 16f);
    }
}

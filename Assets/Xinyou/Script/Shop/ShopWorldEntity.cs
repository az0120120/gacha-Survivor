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
    [SerializeField] float colliderRadius = 0.75f;

    static readonly Color SmallColor = new Color(0.3f, 0.75f, 1f, 0.95f);
    static readonly Color LargeColor = new Color(1f, 0.82f, 0.2f, 0.95f);

    bool isActive = true;

    public ShopSizeType ShopSize => shopSize;
    public bool IsAlive => isActive;

    public void Initialize(ShopSizeType size)
    {
        shopSize = size;
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
            spriteRenderer.sprite = CreatePlaceholderSprite();
            spriteRenderer.sortingOrder = 5;
        }

        var collider = GetComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = colliderRadius;
    }

    void ApplyVisual()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = shopSize == ShopSizeType.Large ? LargeColor : SmallColor;
        transform.localScale = shopSize == ShopSizeType.Large ? Vector3.one * 1.4f : Vector3.one;
    }

    public void TakeDamage(float damage)
    {
        if (!isActive || damage <= 0f)
            return;

        isActive = false;

        if (ShopManager.Instance != null)
            ShopManager.Instance.OpenWorldShop(shopSize, this);

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

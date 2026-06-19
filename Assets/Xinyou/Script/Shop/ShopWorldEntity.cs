using UnityEngine;

[AddComponentMenu("GachaSurvivor/Shop World Entity")]
public class ShopWorldEntity : MonoBehaviour
{
    [SerializeField] ShopSizeType shopSize = ShopSizeType.Small;
    [SerializeField] SpriteRenderer spriteRenderer;

    static readonly Color SmallColor = new Color(0.3f, 0.75f, 1f, 0.95f);
    static readonly Color LargeColor = new Color(1f, 0.82f, 0.2f, 0.95f);

    public ShopSizeType ShopSize => shopSize;

    public void Initialize(ShopSizeType size)
    {
        shopSize = size;
        EnsureVisual();
        ApplyVisual();
    }

    void Awake()
    {
        EnsureVisual();
        ApplyVisual();
    }

    void EnsureVisual()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreatePlaceholderSprite();
        }

        var collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.8f;
        }
    }

    void ApplyVisual()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = shopSize == ShopSizeType.Large ? LargeColor : SmallColor;
        transform.localScale = shopSize == ShopSizeType.Large ? Vector3.one * 1.4f : Vector3.one;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

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

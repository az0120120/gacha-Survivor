using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class MapDestructibleProp : MonoBehaviour, IDamageable
{
    static readonly Color DefaultColor = new Color(0.72f, 0.55f, 0.38f, 0.95f);

    MapPropGridSpawner spawner;
    SpriteRenderer spriteRenderer;
    bool isActive = true;

    public bool IsActive => isActive;
    public bool IsAlive => isActive;

    public void Initialize(MapPropGridSpawner owner, Sprite sprite, float colliderRadius, int sortingOrder)
    {
        spawner = owner;
        isActive = true;
        EnsureComponents(colliderRadius, sortingOrder);

        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = sprite != null ? sprite : CreatePlaceholderSprite();
        spriteRenderer.color = sprite != null ? Color.white : DefaultColor;
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
        collider.isTrigger = true;
        collider.radius = colliderRadius;

        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isActive || damage <= 0f)
            return;

        Break();
    }

    public void Break()
    {
        if (!isActive)
            return;

        isActive = false;

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

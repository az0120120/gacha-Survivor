using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class MapPropPickup : MonoBehaviour
{
    MapPropDropType dropType;
    bool isCollected;

    public void Initialize(
        MapPropDropType type,
        Sprite sprite,
        float spriteScale,
        float colliderRadius,
        int sortingOrder)
    {
        dropType = type;
        isCollected = false;
        float scale = Mathf.Max(0.01f, spriteScale);
        EnsureComponents(colliderRadius, sortingOrder);
        transform.localScale = Vector3.one * scale;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = sprite != null ? sprite : CreatePlaceholderSprite(type);
        spriteRenderer.color = sprite != null ? Color.white : GetPlaceholderColor(type);
    }

    void EnsureComponents(float colliderRadius, int sortingOrder)
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
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

    void Start()
    {
        TryCollectOverlappingPlayer();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected || !other.CompareTag("Player"))
            return;

        Collect(other.GetComponent<PlayerHealth>());
    }

    void TryCollectOverlappingPlayer()
    {
        var collider = GetComponent<CircleCollider2D>();
        if (collider == null)
            return;

        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        var buffer = new Collider2D[4];
        int count = Physics2D.OverlapCircle(
            transform.position,
            collider.radius + 0.05f,
            filter,
            buffer);

        for (int i = 0; i < count; i++)
        {
            if (!buffer[i].CompareTag("Player"))
                continue;

            Collect(buffer[i].GetComponent<PlayerHealth>());
            return;
        }
    }

    void Collect(PlayerHealth playerHealth)
    {
        if (isCollected)
            return;

        isCollected = true;

        if (MapPropStatusEffects.Instance != null)
            MapPropStatusEffects.Instance.ApplyPickup(dropType, playerHealth);

        Destroy(gameObject);
    }

    static Color GetPlaceholderColor(MapPropDropType type)
    {
        switch (type)
        {
            case MapPropDropType.MedicalNeedle:
                return new Color(0.95f, 0.3f, 0.45f, 0.95f);
            case MapPropDropType.UsbDrive:
                return new Color(0.35f, 0.75f, 1f, 0.95f);
            case MapPropDropType.Ufo:
                return new Color(0.65f, 0.45f, 1f, 0.95f);
            case MapPropDropType.SitTight:
                return new Color(0.45f, 0.95f, 1f, 0.95f);
            default:
                return Color.white;
        }
    }

    static Sprite CreatePlaceholderSprite(MapPropDropType type)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, GetPlaceholderColor(type));
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}

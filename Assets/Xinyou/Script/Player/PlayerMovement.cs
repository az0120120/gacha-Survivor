using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CharacterStats))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Transform facingVisual;
    [SerializeField] Sprite rightFacingSprite;
    [SerializeField] Sprite leftFacingSprite;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    CharacterStats characterStats;
    Vector2 moveInput;
    Vector2 lastFacingDirection = Vector2.right;

    public Vector2 LastFacingDirection =>
        lastFacingDirection.sqrMagnitude > 0.0001f ? lastFacingDirection : Vector2.right;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        characterStats = GetComponent<CharacterStats>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        transform.rotation = Quaternion.identity;

        EnsureFacingVisual();
        EnsureFacingSprites();
        EnsureBounceVisual();
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        UpdateFacingDirection();
    }

    void UpdateFacingDirection()
    {
        if (facingVisual == null)
            return;

        if (moveInput.sqrMagnitude > 0.0001f)
            lastFacingDirection = moveInput.normalized;

        bool faceLeft = lastFacingDirection.x < 0f;
        float angle = CalculateFacingAngle(lastFacingDirection, faceLeft);
        facingVisual.localRotation = Quaternion.Euler(0f, 0f, angle);

        if (spriteRenderer != null)
            spriteRenderer.sprite = faceLeft && leftFacingSprite != null
                ? leftFacingSprite
                : rightFacingSprite;
    }

    float CalculateFacingAngle(Vector2 direction, bool faceLeft)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return 0f;

        if (faceLeft && leftFacingSprite != null)
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 180f;

        if (faceLeft)
        {
            Vector2 visualDirection = direction;
            visualDirection.x = -visualDirection.x;
            return Mathf.Atan2(visualDirection.y, visualDirection.x) * Mathf.Rad2Deg;
        }

        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    void FixedUpdate()
    {
        float speed = characterStats != null ? characterStats.MoveSpeed : 5f;
        rb.velocity = moveInput * speed;
    }

    void EnsureFacingVisual()
    {
        if (facingVisual != null)
        {
            if (spriteRenderer == null)
                spriteRenderer = facingVisual.GetComponent<SpriteRenderer>();
            return;
        }

        Transform existing = transform.Find("FacingVisual");
        if (existing != null)
        {
            facingVisual = existing;
            spriteRenderer = facingVisual.GetComponent<SpriteRenderer>();
            return;
        }

        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            facingVisual = CreateVisualChildFromSprite(rootRenderer);
            spriteRenderer = facingVisual.GetComponent<SpriteRenderer>();
            return;
        }

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            facingVisual = spriteRenderer.transform;
    }

    void EnsureFacingSprites()
    {
        if (spriteRenderer == null)
            return;

        if (rightFacingSprite == null)
            rightFacingSprite = spriteRenderer.sprite;

        if (rightFacingSprite != null && spriteRenderer.sprite == null)
            spriteRenderer.sprite = rightFacingSprite;
    }

    void EnsureBounceVisual()
    {
        if (facingVisual == null)
            return;

        var bounceVisual = GetComponent<SpriteBounceVisual>();
        if (bounceVisual == null)
            bounceVisual = gameObject.AddComponent<SpriteBounceVisual>();

        bounceVisual.BindVisual(facingVisual);
    }

    Transform CreateVisualChildFromSprite(SpriteRenderer sourceRenderer)
    {
        var visualObject = new GameObject("FacingVisual");
        visualObject.transform.SetParent(transform, false);
        visualObject.transform.localPosition = Vector3.zero;
        visualObject.transform.localRotation = Quaternion.identity;
        visualObject.transform.localScale = Vector3.one;

        var newRenderer = visualObject.AddComponent<SpriteRenderer>();
        newRenderer.sprite = sourceRenderer.sprite;
        newRenderer.color = sourceRenderer.color;
        newRenderer.flipX = false;
        newRenderer.flipY = sourceRenderer.flipY;
        newRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        newRenderer.sortingOrder = sourceRenderer.sortingOrder;
        newRenderer.material = sourceRenderer.material;

        Destroy(sourceRenderer);
        return visualObject.transform;
    }
}

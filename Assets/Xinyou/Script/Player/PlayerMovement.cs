using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CharacterStats))]
public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    CharacterStats characterStats;
    Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        characterStats = GetComponent<CharacterStats>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        if (spriteRenderer != null && Mathf.Abs(moveInput.x) > 0.01f)
            spriteRenderer.flipX = moveInput.x < 0f;
    }

    void FixedUpdate()
    {
        float speed = characterStats != null ? characterStats.MoveSpeed : 5f;
        rb.velocity = moveInput * speed;
    }
}

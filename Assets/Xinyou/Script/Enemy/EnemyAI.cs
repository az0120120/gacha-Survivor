using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour, IPoolable
{
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float knockbackDecay = 10f;

    Transform player;
    Rigidbody2D rb;
    Vector2 knockbackVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Start()
    {
        CachePlayer();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        knockbackVelocity += direction.normalized * force;
    }

    public void OnGetFromPool()
    {
        knockbackVelocity = Vector2.zero;

        if (player == null)
            CachePlayer();
    }

    public void OnReturnToPool()
    {
        knockbackVelocity = Vector2.zero;
        rb.velocity = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (player == null)
            return;

        Vector2 direction = (Vector2)player.position - rb.position;
        Vector2 chaseVelocity = Vector2.zero;

        if (direction.sqrMagnitude >= 0.0001f)
            chaseVelocity = direction.normalized * moveSpeed;

        rb.velocity = chaseVelocity + knockbackVelocity;
        knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, knockbackDecay * Time.fixedDeltaTime);
    }

    void CachePlayer()
    {
        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }
}

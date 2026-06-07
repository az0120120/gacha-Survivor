using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] float moveSpeed = 2f;

    Transform player;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Start()
    {
        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    void FixedUpdate()
    {
        if (player == null)
            return;

        Vector2 direction = (Vector2)player.position - rb.position;
        if (direction.sqrMagnitude < 0.0001f)
            return;

        rb.velocity = direction.normalized * moveSpeed;
    }
}

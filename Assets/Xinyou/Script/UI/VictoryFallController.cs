using System;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("GachaSurvivor/Victory Fall Controller")]
public class VictoryFallController : MonoBehaviour
{
    Rigidbody2D rb;
    PlayerMovement movement;
    SpriteBounceVisual bounceVisual;

    float gravityScale = 2.5f;
    float upwardImpulse = 10f;
    float terminalFallSpeed = 8f;

    bool isActive;
    bool terminalReached;

    public event Action OnTerminalSpeedReached;

    public void Begin(float gravity, float impulse, float terminalSpeed)
    {
        gravityScale = gravity;
        upwardImpulse = impulse;
        terminalFallSpeed = Mathf.Max(0.1f, terminalSpeed);

        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        bounceVisual = GetComponent<SpriteBounceVisual>();

        if (movement != null)
            movement.enabled = false;

        if (bounceVisual != null)
            bounceVisual.enabled = false;

        rb.gravityScale = gravityScale;
        rb.velocity = Vector2.up * upwardImpulse;

        isActive = true;
        terminalReached = false;
        enabled = true;
    }

    void FixedUpdate()
    {
        if (!isActive || rb == null)
            return;

        if (terminalReached)
        {
            MaintainTerminalSpeed();
            return;
        }

        if (rb.velocity.y <= -terminalFallSpeed)
        {
            terminalReached = true;
            rb.velocity = new Vector2(rb.velocity.x, -terminalFallSpeed);
            OnTerminalSpeedReached?.Invoke();
        }
    }

    void MaintainTerminalSpeed()
    {
        rb.velocity = new Vector2(rb.velocity.x, -terminalFallSpeed);
    }
}

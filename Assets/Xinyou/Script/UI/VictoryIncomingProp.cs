using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("GachaSurvivor/Victory Incoming Prop")]
[DefaultExecutionOrder(100)]
public class VictoryIncomingProp : MonoBehaviour
{
    enum Phase
    {
        Rushing,
        CameraLocked
    }

    Camera targetCamera;
    VictoryIncomingPropSettings settings;
    Phase phase;
    Vector2 spawnViewport;
    float rushProgress;
    float rushProgressVelocity;
    float worldPathLengthAtLaunch;

    public void Launch(Camera camera, VictoryIncomingPropSettings config, GoldenParticleTrailSettings trailSettings)
    {
        targetCamera = camera;
        settings = config;

        ApplyVisual();
        var trail = gameObject.AddComponent<GoldenParticleTrail>();
        trailSettings.ApplyTo(trail);

        spawnViewport = settings.spawnViewport;
        rushProgress = 0f;
        rushProgressVelocity = 0f;
        worldPathLengthAtLaunch = Vector2.Distance(
            ViewportToWorld(spawnViewport),
            ViewportToWorld(settings.targetViewport));
        worldPathLengthAtLaunch = Mathf.Max(0.01f, worldPathLengthAtLaunch);

        transform.position = ViewportToWorld(spawnViewport);
        phase = Phase.Rushing;
        enabled = true;
    }

    void ApplyVisual()
    {
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = gameObject.AddComponent<SpriteRenderer>();

        renderer.sprite = settings.sprite != null ? settings.sprite : CreateFallbackSprite();
        renderer.color = settings.tintColor;
        renderer.sortingOrder = settings.sortingOrder;
        transform.localScale = Vector3.one * Mathf.Max(0.01f, settings.scale);
    }

    void LateUpdate()
    {
        if (targetCamera == null || settings == null)
            return;

        if (phase == Phase.Rushing)
            UpdateRush(Time.deltaTime);
        else
            FollowCamera();
    }

    void UpdateRush(float deltaTime)
    {
        float remainingWorld = (1f - rushProgress) * worldPathLengthAtLaunch;
        if (remainingWorld <= settings.arriveDistance)
        {
            LockToCamera();
            return;
        }

        float progressAcceleration = settings.rushAcceleration / worldPathLengthAtLaunch;
        float maxProgressSpeed = settings.maxRushSpeed / worldPathLengthAtLaunch;

        rushProgressVelocity += progressAcceleration * deltaTime;
        rushProgressVelocity = Mathf.Min(rushProgressVelocity, maxProgressSpeed);
        rushProgress += rushProgressVelocity * deltaTime;

        if (rushProgress >= 1f)
        {
            LockToCamera();
            return;
        }

        Vector2 viewport = Vector2.LerpUnclamped(spawnViewport, settings.targetViewport, rushProgress);
        transform.position = ViewportToWorld(viewport);
    }

    void LockToCamera()
    {
        phase = Phase.CameraLocked;
        rushProgress = 1f;
        rushProgressVelocity = 0f;
        transform.position = ViewportToWorld(settings.targetViewport);
    }

    void FollowCamera()
    {
        transform.position = ViewportToWorld(settings.targetViewport);
    }

    Vector3 ViewportToWorld(Vector2 viewport)
    {
        float depth = Mathf.Abs(targetCamera.transform.position.z);
        Vector3 world = targetCamera.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, depth));
        world.z = 0f;
        return world;
    }

    static Sprite CreateFallbackSprite()
    {
        const int size = 16;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
                texture.SetPixel(x, y, Color.white);
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 16f);
    }
}

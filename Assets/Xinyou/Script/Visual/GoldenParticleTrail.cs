using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("GachaSurvivor/Golden Particle Trail")]
public class GoldenParticleTrail : MonoBehaviour
{
    [SerializeField] GoldenParticleTrailSettings settings = new GoldenParticleTrailSettings();

    float timeSinceLastSpawn;
    float distanceSinceLastSpawn;
    float nextSpawnDistance;
    Vector3 lastSamplePosition;
    Vector2 lastMoveDirection = Vector2.down;
    bool initialized;
    static Sprite cachedParticleSprite;

    public void ApplySettings(GoldenParticleTrailSettings newSettings)
    {
        settings = newSettings ?? new GoldenParticleTrailSettings();
    }

    void OnEnable()
    {
        timeSinceLastSpawn = settings.minSpawnInterval;
        distanceSinceLastSpawn = 0f;
        nextSpawnDistance = GetNextSpawnDistance();
        lastSamplePosition = transform.position;
        lastMoveDirection = Vector2.down;
        initialized = true;
    }

    void Update()
    {
        if (!initialized)
            return;

        Vector3 currentPosition = transform.position;
        Vector2 delta = currentPosition - lastSamplePosition;
        float moved = delta.magnitude;

        if (moved > 0.0001f)
        {
            lastMoveDirection = delta / moved;
            distanceSinceLastSpawn += moved;
            lastSamplePosition = currentPosition;
        }

        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn < settings.minSpawnInterval)
            return;

        if (distanceSinceLastSpawn < nextSpawnDistance)
            return;

        if (Random.value <= settings.spawnChance)
            SpawnParticle(currentPosition, lastMoveDirection);

        distanceSinceLastSpawn = 0f;
        timeSinceLastSpawn = 0f;
        nextSpawnDistance = GetNextSpawnDistance();
    }

    float GetNextSpawnDistance()
    {
        return Mathf.Max(0.06f, settings.spawnStepDistance + Random.Range(-settings.spawnStepJitter, settings.spawnStepJitter));
    }

    void SpawnParticle(Vector3 position, Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.down;

        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        position += (Vector3)(perpendicular * Random.Range(-settings.positionJitter, settings.positionJitter));
        position += (Vector3)(direction * Random.Range(-settings.positionJitter * 0.35f, settings.positionJitter * 0.35f));

        var particleObject = new GameObject("GoldenTrailParticle");
        particleObject.transform.position = position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        angle += Random.Range(-settings.rotationJitter, settings.rotationJitter);
        particleObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float sizeMultiplier = 1f + Random.Range(-settings.sizeRandomness, settings.sizeRandomness);
        particleObject.transform.localScale = new Vector3(
            settings.particleScale * settings.widthStretch * sizeMultiplier,
            settings.particleScale * settings.lengthStretch * sizeMultiplier,
            1f);

        var renderer = particleObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetParticleSprite();
        renderer.color = settings.particleColor;
        renderer.sortingOrder = settings.sortingOrder + Random.Range(-1, 2);

        particleObject.AddComponent<TimedSpriteFade>().Begin(
            settings.particleLifetime * Random.Range(0.75f, 1.15f));
    }

    static Sprite GetParticleSprite()
    {
        if (cachedParticleSprite != null)
            return cachedParticleSprite;

        const int size = 8;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        float center = (size - 1) * 0.5f;
        float radius = center;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = Mathf.Abs(x - center) / radius;
                float ny = Mathf.Abs(y - center) / radius;
                float edge = nx + ny;
                float alpha = edge <= 0.88f ? 1f : edge <= 1f ? 1f - (edge - 0.88f) / 0.12f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        cachedParticleSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            16f);
        return cachedParticleSprite;
    }
}

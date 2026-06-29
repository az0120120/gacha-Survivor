using System;
using UnityEngine;

[Serializable]
public class GoldenParticleTrailSettings
{
    public Color particleColor = new Color(1f, 0.82f, 0.15f, 0.95f);
    public float minSpawnInterval = 0.1f;
    public float spawnStepDistance = 0.2f;
    public float spawnStepJitter = 0.1f;
    [Range(0f, 1f)] public float spawnChance = 0.82f;
    public float particleLifetime = 0.28f;
    public float particleScale = 0.88f;
    public float lengthStretch = 1.05f;
    public float widthStretch = 0.65f;
    [Range(0f, 1f)] public float sizeRandomness = 0.4f;
    public float rotationJitter = 28f;
    public float positionJitter = 0.07f;
    public int sortingOrder = 14;

    public GoldenParticleTrailSettings Clone()
    {
        return new GoldenParticleTrailSettings
        {
            particleColor = particleColor,
            minSpawnInterval = minSpawnInterval,
            spawnStepDistance = spawnStepDistance,
            spawnStepJitter = spawnStepJitter,
            spawnChance = spawnChance,
            particleLifetime = particleLifetime,
            particleScale = particleScale,
            lengthStretch = lengthStretch,
            widthStretch = widthStretch,
            sizeRandomness = sizeRandomness,
            rotationJitter = rotationJitter,
            positionJitter = positionJitter,
            sortingOrder = sortingOrder
        };
    }

    public void ApplyTo(GoldenParticleTrail trail)
    {
        if (trail == null)
            return;

        trail.ApplySettings(this);
    }
}

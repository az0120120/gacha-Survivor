using UnityEngine;

[AddComponentMenu("GachaSurvivor/Game Flow Manager")]
public class GameFlowManager : MonoBehaviour
{
    [SerializeField] WaveSpawner waveSpawner;

    void Awake()
    {
        if (waveSpawner == null)
            waveSpawner = FindFirstObjectByType<WaveSpawner>();
    }

    void Start()
    {
        if (waveSpawner == null)
            return;

        waveSpawner.OnWaveCompleted += HandleWaveCompleted;
        waveSpawner.StartNextWave();
    }

    void OnDestroy()
    {
        if (waveSpawner != null)
            waveSpawner.OnWaveCompleted -= HandleWaveCompleted;
    }

    void HandleWaveCompleted(int wave)
    {
        if (waveSpawner != null)
            waveSpawner.StartNextWave();
    }
}

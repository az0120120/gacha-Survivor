using UnityEngine;

[AddComponentMenu("GachaSurvivor/Game Flow Manager")]
public class GameFlowManager : MonoBehaviour
{
    [SerializeField] WaveSpawner waveSpawner;

    void Awake()
    {
        if (waveSpawner == null)
            waveSpawner = FindFirstObjectByType<WaveSpawner>();

        EnsureBossSystems();
    }

    void EnsureBossSystems()
    {
        if (waveSpawner != null && waveSpawner.GetComponent<BossSpawner>() == null)
            waveSpawner.gameObject.AddComponent<BossSpawner>();

        if (GetComponent<VictoryManager>() == null)
            gameObject.AddComponent<VictoryManager>();
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
        if (VictoryManager.Instance != null && VictoryManager.Instance.IsVictory)
            return;

        if (waveSpawner != null)
            waveSpawner.StartNextWave();
    }
}

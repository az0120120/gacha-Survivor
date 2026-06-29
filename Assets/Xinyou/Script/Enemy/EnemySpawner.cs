using System;
using UnityEngine;

[AddComponentMenu("GachaSurvivor/Wave Spawner")]
public class WaveSpawner : MonoBehaviour
{
    [SerializeField] ObjectPool enemyPool;
    [SerializeField] EnemyCatalog enemyCatalog;
    [SerializeField] Transform player;

    [Header("Spawn Cycle")]
    [Tooltip("每多少秒触发一批刷怪")]
    [SerializeField] float waveCycleInterval = 5f;

    [Tooltip("刷怪数量 = 基础 + 每分钟线性增长 + sin(分钟×2)×振幅")]
    [SerializeField] int spawnCountBase = 6;
    [SerializeField] int spawnCountPerMinute = 3;
    [SerializeField] float spawnCountSinAmplitude = 30f;

    [Header("Spawn Placement")]
    [SerializeField] float spawnInterval = 0.6f;
    [SerializeField] float spawnMinRadius = 11f;
    [SerializeField] float spawnMaxRadius = 16f;
    [SerializeField] EnemyArchetype[] spawnArchetypes =
    {
        EnemyArchetype.MeleeRush,
        EnemyArchetype.RangedShooter,
        EnemyArchetype.Harasser
    };

    int currentWave;
    int enemiesToSpawn;
    int enemiesSpawned;
    float spawnTimer;
    float waveCycleTimer;
    bool waveActive;

    public int CurrentWave => currentWave;
    public bool IsWaveActive => waveActive;
    public EnemyCatalog EnemyCatalog => enemyCatalog;

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;

    void Awake()
    {
        if (enemyPool == null)
            enemyPool = GetComponent<ObjectPool>();

        if (enemyCatalog == null)
            enemyCatalog = Resources.Load<EnemyCatalog>("EnemyCatalog");

        EnsureProjectilePool();
    }

    void Start()
    {
        if (player == null)
        {
            var playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    void Update()
    {
        if (!waveActive || enemyPool == null || player == null)
            return;

        if (enemiesSpawned < enemiesToSpawn)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = spawnInterval;
                SpawnEnemy();
                enemiesSpawned++;

                if (enemiesSpawned >= enemiesToSpawn)
                    OnWaveCompleted?.Invoke(currentWave);
            }
        }

        waveCycleTimer -= Time.deltaTime;
        if (waveCycleTimer <= 0f)
        {
            waveCycleTimer = waveCycleInterval;
            BeginSpawnBatch();
        }
    }

    public void StopWaves()
    {
        waveActive = false;
    }

    public void StartNextWave()
    {
        waveActive = true;
        waveCycleTimer = waveCycleInterval;
        BeginSpawnBatch();
    }

    public static int CalculateSpawnCount(int gameMinutes, int spawnBase, int perMinute, float sinAmplitude)
    {
        float count = spawnBase
            + perMinute * gameMinutes
            + Mathf.Sin(gameMinutes * 2f) * sinAmplitude;
        return Mathf.Max(0, Mathf.RoundToInt(count));
    }

    int GetGameMinutes()
    {
        return GameTimeManager.Instance != null ? GameTimeManager.Instance.GameMinutes : 0;
    }

    void BeginSpawnBatch()
    {
        currentWave++;
        enemiesToSpawn = CalculateSpawnCount(
            GetGameMinutes(),
            spawnCountBase,
            spawnCountPerMinute,
            spawnCountSinAmplitude);
        enemiesSpawned = 0;
        spawnTimer = 0f;
        OnWaveStarted?.Invoke(currentWave);
    }

    void SpawnEnemy()
    {
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float distance = UnityEngine.Random.Range(spawnMinRadius, spawnMaxRadius);
        var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        var spawnPosition = (Vector2)player.position + offset;

        GameObject enemyObject = enemyPool.Get(spawnPosition, Quaternion.identity);
        if (enemyObject == null)
            return;

        var enemyStats = enemyObject.GetComponent<EnemyStats>();
        var enemyAI = enemyObject.GetComponent<EnemyAI>();
        EnemyArchetype archetype = PickSpawnArchetype();

        if (enemyStats != null)
        {
            enemyStats.SetArchetype(archetype, enemyCatalog);
            enemyAI?.ApplyArchetypeConfig(enemyStats.Definition);
        }
    }

    EnemyArchetype PickSpawnArchetype()
    {
        if (spawnArchetypes == null || spawnArchetypes.Length == 0)
            return EnemyArchetype.MeleeRush;

        int index = UnityEngine.Random.Range(0, spawnArchetypes.Length);
        return spawnArchetypes[index];
    }

    void EnsureProjectilePool()
    {
        if (FindAnyObjectByType<EnemyProjectilePool>() != null)
            return;

        gameObject.AddComponent<EnemyProjectilePool>();
    }
}

[System.Obsolete("请改用 Wave Spawner 组件。")]
[AddComponentMenu("GachaSurvivor/Legacy Enemy Spawner")]
public class EnemySpawner : WaveSpawner
{
}

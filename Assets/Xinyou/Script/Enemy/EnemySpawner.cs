using System;
using UnityEngine;

[AddComponentMenu("GachaSurvivor/Wave Spawner")]
public class WaveSpawner : MonoBehaviour
{
    [SerializeField] ObjectPool enemyPool;
    [SerializeField] EnemyCatalog enemyCatalog;
    [SerializeField] Transform player;
    [SerializeField] int baseEnemiesPerWave = 6;
    [SerializeField] int enemiesIncreasePerWave = 3;
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
            }

            return;
        }

        if (enemyPool.ActiveCount <= 0)
            CompleteWave();
    }

    public void StopWaves()
    {
        waveActive = false;
    }

    public void StartNextWave()
    {
        currentWave++;
        enemiesToSpawn = baseEnemiesPerWave + (currentWave - 1) * enemiesIncreasePerWave;
        enemiesSpawned = 0;
        spawnTimer = 0f;
        waveActive = true;
        OnWaveStarted?.Invoke(currentWave);
    }

    void CompleteWave()
    {
        waveActive = false;
        OnWaveCompleted?.Invoke(currentWave);
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

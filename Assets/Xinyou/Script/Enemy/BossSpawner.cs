using UnityEngine;

[AddComponentMenu("GachaSurvivor/Boss Spawner")]
public class BossSpawner : MonoBehaviour
{
    [SerializeField] ObjectPool bossPool;
    [SerializeField] BossCatalog bossCatalog;
    [SerializeField] EnemyCatalog enemyCatalog;
    [SerializeField] Transform player;
    [SerializeField] float spawnIntervalSeconds = 180f;
    [SerializeField] int totalBossCount = 5;
    [SerializeField] float spawnMinRadius = 10f;
    [SerializeField] float spawnMaxRadius = 14f;

    int spawnedBossCount;
    bool spawningEnabled = true;
    GameObject activeBossObject;

    public int SpawnedBossCount => spawnedBossCount;
    public int TotalBossCount => totalBossCount;
    public bool HasActiveBoss => activeBossObject != null && activeBossObject.activeInHierarchy;
    public BossEnemy ActiveBoss => HasActiveBoss ? activeBossObject.GetComponent<BossEnemy>() : null;
    public EnemyHealth ActiveBossHealth => HasActiveBoss ? activeBossObject.GetComponent<EnemyHealth>() : null;
    public bool IsComplete => spawnedBossCount >= totalBossCount && !HasActiveBoss;

    void Awake()
    {
        EnsureBossPool();
        EnsureCatalogs();
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
        if (!spawningEnabled || player == null || bossPool == null)
            return;

        if (VictoryManager.Instance != null && VictoryManager.Instance.IsVictory)
            return;

        if (spawnedBossCount >= totalBossCount)
            return;

        if (HasActiveBoss)
            return;

        if (GameTimeManager.Instance == null)
            return;

        float requiredTime = spawnIntervalSeconds * (spawnedBossCount + 1);
        if (GameTimeManager.Instance.ElapsedSeconds < requiredTime)
            return;

        SpawnBoss(spawnedBossCount);
    }

    public void StopSpawning()
    {
        spawningEnabled = false;
    }

    public void NotifyBossReleased(GameObject bossObject)
    {
        if (activeBossObject == bossObject)
            activeBossObject = null;
    }

    void SpawnBoss(int bossIndex)
    {
        BossDefinition definition = bossCatalog != null ? bossCatalog.GetBoss(bossIndex) : null;
        if (definition == null)
            return;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(spawnMinRadius, spawnMaxRadius);
        var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        var spawnPosition = (Vector2)player.position + offset;

        GameObject bossObject = bossPool.Get(spawnPosition, Quaternion.identity);
        if (bossObject == null)
            return;

        var bossEnemy = bossObject.GetComponent<BossEnemy>();
        if (bossEnemy == null)
            bossEnemy = bossObject.AddComponent<BossEnemy>();

        bossEnemy.Configure(definition, bossIndex, enemyCatalog);
        activeBossObject = bossObject;
        spawnedBossCount++;
    }

    void EnsureBossPool()
    {
        if (bossPool != null)
            return;

        var poolObject = new GameObject("BossPool");
        poolObject.transform.SetParent(transform, false);
        bossPool = poolObject.AddComponent<ObjectPool>();

        ObjectPool sourcePool = GetComponent<ObjectPool>();
        if (sourcePool == null)
            sourcePool = FindFirstObjectByType<WaveSpawner>()?.GetComponent<ObjectPool>();

        if (sourcePool != null && sourcePool.Prefab != null)
            bossPool.Setup(sourcePool.Prefab, 5);
    }

    void EnsureCatalogs()
    {
        if (bossCatalog == null)
        {
            bossCatalog = Resources.Load<BossCatalog>("BossCatalog");
            if (bossCatalog == null)
            {
                var runtimeCatalog = ScriptableObject.CreateInstance<BossCatalog>();
                runtimeCatalog.SetBosses(BossCatalog.CreateDefaultDefinitions());
                bossCatalog = runtimeCatalog;
            }
        }

        if (enemyCatalog == null)
        {
            var waveSpawner = GetComponent<WaveSpawner>() ?? FindFirstObjectByType<WaveSpawner>();
            if (waveSpawner != null)
                enemyCatalog = waveSpawner.EnemyCatalog;
        }
    }
}

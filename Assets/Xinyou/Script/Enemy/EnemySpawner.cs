using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] ObjectPool enemyPool;
    [SerializeField] Transform player;
    [SerializeField] float spawnInterval = 2f;
    [SerializeField] float spawnMinRadius = 8f;
    [SerializeField] float spawnMaxRadius = 12f;
    [SerializeField] int maxEnemies = 50;

    float spawnTimer;

    void Awake()
    {
        if (enemyPool == null)
            enemyPool = GetComponent<ObjectPool>();
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
        if (enemyPool == null || player == null)
            return;

        if (enemyPool.ActiveCount >= maxEnemies)
            return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f)
            return;

        spawnTimer = spawnInterval;
        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(spawnMinRadius, spawnMaxRadius);
        var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        var spawnPosition = (Vector2)player.position + offset;

        enemyPool.Get(spawnPosition, Quaternion.identity);
    }
}

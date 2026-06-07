using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Transform player;
    [SerializeField] float spawnInterval = 2f;
    [SerializeField] float spawnMinRadius = 8f;
    [SerializeField] float spawnMaxRadius = 12f;
    [SerializeField] int maxEnemies = 50;

    float spawnTimer;

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
        if (enemyPrefab == null || player == null)
            return;

        if (GetAliveEnemyCount() >= maxEnemies)
            return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f)
            return;

        spawnTimer = spawnInterval;
        SpawnEnemy();
    }

    int GetAliveEnemyCount()
    {
        var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        int count = 0;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].IsAlive)
                count++;
        }

        return count;
    }

    void SpawnEnemy()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(spawnMinRadius, spawnMaxRadius);
        var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        var spawnPosition = (Vector2)player.position + offset;

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}

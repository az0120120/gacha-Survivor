using UnityEngine;

[AddComponentMenu("GachaSurvivor/Shop Spawner")]
public class ShopSpawner : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float spawnInterval = 90f;
    [SerializeField] float spawnMinRadius = 6f;
    [SerializeField] float spawnMaxRadius = 14f;

    float spawnTimer;
    int spawnCycleIndex;

    void Start()
    {
        if (player == null)
        {
            var playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        spawnTimer = spawnInterval;
    }

    void Update()
    {
        if (player == null || Time.timeScale <= 0f)
            return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f)
            return;

        spawnTimer = spawnInterval;
        SpawnShop();
        spawnCycleIndex = (spawnCycleIndex + 1) % 3;
    }

    void SpawnShop()
    {
        ShopSizeType shopSize = spawnCycleIndex == 2 ? ShopSizeType.Large : ShopSizeType.Small;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(spawnMinRadius, spawnMaxRadius);
        var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        var spawnPosition = (Vector2)player.position + offset;

        var shopObject = new GameObject(shopSize == ShopSizeType.Large ? "LargeShop" : "SmallShop");
        shopObject.transform.position = spawnPosition;
        var entity = shopObject.AddComponent<ShopWorldEntity>();
        entity.Initialize(shopSize);
    }
}

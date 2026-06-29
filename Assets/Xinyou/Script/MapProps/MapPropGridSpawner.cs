using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("GachaSurvivor/Map Prop Grid Spawner")]
public class MapPropGridSpawner : MonoBehaviour
{
    [Header("Chunks")]
    [SerializeField] MapChunkGenerator mapChunkGenerator;
    [SerializeField] Vector2 chunkSize = new Vector2(10f, 10f);
    [SerializeField] bool autoChunkSizeFromCamera = true;
    [SerializeField] float cameraChunkPadding = 0f;
    [SerializeField] float chunkCheckInterval = 0.2f;
    [SerializeField] bool preloadAdjacentChunks = true;
    [Tooltip("以当前区块为中心，向四周预加载的区块层数（1 = 3×3）")]
    [SerializeField] int adjacentChunkRadius = 1;

    [Header("Grid")]
    [Tooltip("网格间距，15cm = 0.15")]
    [SerializeField] float gridStep = 0.15f;
    [Tooltip("每个网格格子生成可破坏物的概率")]
    [SerializeField] [Range(0f, 1f)] float cellSpawnChance = 0.35f;
    [Tooltip("在格子内部随机偏移位置")]
    [SerializeField] bool randomizePositionInCell = true;
    [SerializeField] float playerSpawnClearRadius = 1.2f;

    [Header("Destructible")]
    [SerializeField] Sprite destructibleSprite;
    [SerializeField] float destructibleSpriteScale = 1f;
    [SerializeField] float destructibleColliderRadius = 0.08f;
    [SerializeField] int destructibleSortingOrder = 2;
    [SerializeField] int destructibleMaxHealth = 20;

    [Header("Destructible Wander")]
    [SerializeField] bool destructibleWanderEnabled = true;
    [Tooltip("以生成点为中心的可游荡半径")]
    [SerializeField] float destructibleWanderRadius = 0.45f;
    [SerializeField] float destructibleWanderSpeed = 0.35f;
    [SerializeField] float destructibleWanderRetargetInterval = 2f;

    [Header("Audio")]
    [SerializeField] AudioClip breakClip;
    [SerializeField] [Range(0f, 1f)] float breakVolume = 0.7f;
    [SerializeField] float breakSoundMinInterval = 0.03f;

    [Header("Pickup")]
    [SerializeField] float pickupColliderRadius = 0.12f;
    [SerializeField] int pickupSortingOrder = 6;

    [Header("Drops")]
    [SerializeField] MapPropDropEntry[] dropEntries =
    {
        MapPropDropEntry.CreateDefault(MapPropDropType.MedicalNeedle, 70f),
        MapPropDropEntry.CreateDefault(MapPropDropType.UsbDrive, 15f),
        MapPropDropEntry.CreateDefault(MapPropDropType.Ufo, 5f),
        MapPropDropEntry.CreateDefault(MapPropDropType.SitTight, 10f)
    };

    readonly HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();

    Transform propsRoot;
    Transform player;
    float lastBreakSoundTime;
    float chunkCheckTimer;
    Vector2Int currentPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);

    void Start()
    {
        EnsureStatusEffects();
        EnsurePropsRoot();
        ResolveChunkSize();
        CachePlayer();
        UpdateActiveChunk();
    }

    void Update()
    {
        chunkCheckTimer -= Time.deltaTime;
        if (chunkCheckTimer > 0f)
            return;

        chunkCheckTimer = chunkCheckInterval;
        UpdateActiveChunk();
    }

    void EnsureStatusEffects()
    {
        if (FindAnyObjectByType<MapPropStatusEffects>() == null)
            gameObject.AddComponent<MapPropStatusEffects>();
    }

    void EnsurePropsRoot()
    {
        if (propsRoot != null)
            return;

        var rootObject = new GameObject("MapProps");
        propsRoot = rootObject.transform;
        propsRoot.SetParent(transform, false);
    }

    void CachePlayer()
    {
        if (player != null)
            return;

        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    void ResolveChunkSize()
    {
        if (mapChunkGenerator != null)
        {
            mapChunkGenerator.ResolveChunkSize();
            chunkSize = mapChunkGenerator.ChunkSize;
            return;
        }

        if (!autoChunkSizeFromCamera)
            return;

        chunkSize = MapChunkGrid.ResolveChunkSizeFromCamera(chunkSize, cameraChunkPadding);
    }

    void UpdateActiveChunk()
    {
        CachePlayer();
        ResolveChunkSize();
        if (player == null || chunkSize.x <= 0f || chunkSize.y <= 0f || gridStep <= 0f)
            return;

        Vector2Int playerChunk = WorldToChunk(player.position);
        currentPlayerChunk = playerChunk;
        EnsureChunksAround(playerChunk);
    }

    void EnsureChunksAround(Vector2Int centerChunk)
    {
        int radius = GetAdjacentChunkRadius();

        for (int offsetY = -radius; offsetY <= radius; offsetY++)
        {
            for (int offsetX = -radius; offsetX <= radius; offsetX++)
            {
                var chunkCoord = new Vector2Int(centerChunk.x + offsetX, centerChunk.y + offsetY);
                TryGenerateChunk(chunkCoord);
            }
        }
    }

    int GetAdjacentChunkRadius()
    {
        if (mapChunkGenerator != null)
            return mapChunkGenerator.PreloadAdjacentChunks
                ? Mathf.Max(0, mapChunkGenerator.AdjacentChunkRadius)
                : 0;

        return preloadAdjacentChunks ? Mathf.Max(0, adjacentChunkRadius) : 0;
    }

    void TryGenerateChunk(Vector2Int chunkCoord)
    {
        if (generatedChunks.Contains(chunkCoord))
            return;

        GenerateChunk(chunkCoord);
        generatedChunks.Add(chunkCoord);
    }

    void GenerateChunk(Vector2Int chunkCoord)
    {
        Vector2 chunkMin = ChunkToWorldMin(chunkCoord);
        Vector2 playerPosition = player != null ? (Vector2)player.position : chunkMin;
        float clearRadiusSqr = playerSpawnClearRadius * playerSpawnClearRadius;

        var chunkRoot = new GameObject($"MapChunk_{chunkCoord.x}_{chunkCoord.y}");
        chunkRoot.transform.SetParent(propsRoot, false);
        chunkRoot.transform.position = chunkMin;

        Random.State randomBackup = Random.state;
        Random.InitState(GetChunkSeed(chunkCoord));

        for (float y = 0f; y <= chunkSize.y + 0.0001f; y += gridStep)
        {
            for (float x = 0f; x <= chunkSize.x + 0.0001f; x += gridStep)
            {
                if (Random.value > cellSpawnChance)
                    continue;

                Vector2 position = chunkMin + new Vector2(x, y);
                if (randomizePositionInCell)
                {
                    float halfCell = gridStep * 0.45f;
                    position.x += Random.Range(-halfCell, halfCell);
                    position.y += Random.Range(-halfCell, halfCell);
                }

                position.x = Mathf.Clamp(position.x, chunkMin.x, chunkMin.x + chunkSize.x);
                position.y = Mathf.Clamp(position.y, chunkMin.y, chunkMin.y + chunkSize.y);

                if ((position - playerPosition).sqrMagnitude <= clearRadiusSqr)
                    continue;

                SpawnDestructible(position, chunkRoot.transform);
            }
        }

        Random.state = randomBackup;
    }

    Vector2Int WorldToChunk(Vector2 worldPosition)
    {
        if (mapChunkGenerator != null)
            return mapChunkGenerator.WorldToChunk(worldPosition);

        return MapChunkGrid.WorldToChunk(worldPosition, chunkSize);
    }

    Vector2 ChunkToWorldMin(Vector2Int chunkCoord)
    {
        if (mapChunkGenerator != null)
            return mapChunkGenerator.ChunkToWorldMin(chunkCoord);

        return MapChunkGrid.ChunkToWorldMin(chunkCoord, chunkSize);
    }

    static int GetChunkSeed(Vector2Int chunkCoord)
    {
        unchecked
        {
            return chunkCoord.x * 73856093 ^ chunkCoord.y * 19349663;
        }
    }

    void SpawnDestructible(Vector2 position, Transform chunkParent)
    {
        var propObject = new GameObject("MapDestructible");
        propObject.transform.SetParent(chunkParent, false);
        propObject.transform.position = position;

        var prop = propObject.AddComponent<MapDestructibleProp>();
        prop.Initialize(
            this,
            destructibleSprite,
            destructibleColliderRadius,
            destructibleSortingOrder,
            destructibleMaxHealth,
            destructibleSpriteScale,
            destructibleWanderEnabled,
            destructibleWanderRadius,
            destructibleWanderSpeed,
            destructibleWanderRetargetInterval);
    }

    public void PlayBreakSound(Vector3 position)
    {
        if (breakClip == null)
            return;

        if (Time.unscaledTime - lastBreakSoundTime < breakSoundMinInterval)
            return;

        AudioSource.PlayClipAtPoint(breakClip, position, breakVolume);
        lastBreakSoundTime = Time.unscaledTime;
    }

    public void SpawnPickup(Vector3 position)
    {
        MapPropDropType dropType = MapPropDropTable.Roll(dropEntries);
        MapPropDropEntry entry = MapPropDropTable.GetEntry(dropEntries, dropType);

        var pickupObject = new GameObject($"MapPickup_{dropType}");
        pickupObject.transform.SetParent(propsRoot, false);
        pickupObject.transform.position = position;

        var pickup = pickupObject.AddComponent<MapPropPickup>();
        pickup.Initialize(
            dropType,
            entry.sprite,
            entry.spriteScale,
            pickupColliderRadius,
            pickupSortingOrder);
    }

    void OnDrawGizmosSelected()
    {
        Vector2 size = chunkSize;
        if (mapChunkGenerator == null && autoChunkSizeFromCamera && Camera.main != null && Camera.main.orthographic)
            size = MapChunkGrid.ResolveChunkSizeFromCamera(chunkSize, cameraChunkPadding);

        if (size.x <= 0f || size.y <= 0f)
            return;

        Vector2Int centerChunk = Application.isPlaying && player != null
            ? WorldToChunk(player.position)
            : Vector2Int.zero;

        int radius = GetAdjacentChunkRadius();
        Gizmos.color = new Color(0.95f, 0.75f, 0.25f, 0.35f);

        for (int offsetY = -radius; offsetY <= radius; offsetY++)
        {
            for (int offsetX = -radius; offsetX <= radius; offsetX++)
            {
                Vector2 chunkMin = ChunkToWorldMin(new Vector2Int(
                    centerChunk.x + offsetX,
                    centerChunk.y + offsetY));
                Gizmos.DrawWireCube(chunkMin + size * 0.5f, new Vector3(size.x, size.y, 0f));
            }
        }
    }
}

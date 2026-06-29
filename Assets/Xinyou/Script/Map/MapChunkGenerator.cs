using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("GachaSurvivor/Map Chunk Generator")]
public class MapChunkGenerator : MonoBehaviour
{
    [Header("Chunks")]
    [SerializeField] Vector2 chunkSize = new Vector2(20f, 20f);
    [SerializeField] bool autoChunkSizeFromCamera = true;
    [SerializeField] float cameraChunkPadding = 0f;
    [SerializeField] float chunkCheckInterval = 0.2f;
    [SerializeField] bool preloadAdjacentChunks = true;
    [Tooltip("以当前区块为中心，向四周预加载的区块层数（1 = 3×3）")]
    [SerializeField] int adjacentChunkRadius = 1;

    [Header("Theme")]
    [SerializeField] MapTheme mapTheme;
    [SerializeField] Sprite chunkSprite;
    [SerializeField] Color chunkColor = Color.white;
    [SerializeField] int sortingOrder = -10;
    [SerializeField] float spriteScale = 1f;

    readonly HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();
    readonly Dictionary<Vector2Int, GameObject> chunkObjects = new Dictionary<Vector2Int, GameObject>();

    Transform chunksRoot;
    Transform player;
    float chunkCheckTimer;
    Vector2 resolvedChunkSize;
    Sprite cachedFallbackSprite;

    public Vector2 ChunkSize => resolvedChunkSize;
    public bool AutoChunkSizeFromCamera => autoChunkSizeFromCamera;
    public float CameraChunkPadding => cameraChunkPadding;
    public int AdjacentChunkRadius => adjacentChunkRadius;
    public bool PreloadAdjacentChunks => preloadAdjacentChunks;

    public Vector2Int WorldToChunk(Vector2 worldPosition)
    {
        return MapChunkGrid.WorldToChunk(worldPosition, resolvedChunkSize);
    }

    public Vector2 ChunkToWorldMin(Vector2Int chunkCoord)
    {
        return MapChunkGrid.ChunkToWorldMin(chunkCoord, resolvedChunkSize);
    }

    void Start()
    {
        EnsureChunksRoot();
        ResolveChunkSize();
        CachePlayer();
        UpdateActiveChunks();
    }

    void Update()
    {
        chunkCheckTimer -= Time.deltaTime;
        if (chunkCheckTimer > 0f)
            return;

        chunkCheckTimer = chunkCheckInterval;
        UpdateActiveChunks();
    }

    void EnsureChunksRoot()
    {
        if (chunksRoot != null)
            return;

        var rootObject = new GameObject("MapGround");
        chunksRoot = rootObject.transform;
        chunksRoot.SetParent(transform, false);
    }

    void CachePlayer()
    {
        if (player != null)
            return;

        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    public void ResolveChunkSize()
    {
        resolvedChunkSize = autoChunkSizeFromCamera
            ? MapChunkGrid.ResolveChunkSizeFromCamera(chunkSize, cameraChunkPadding)
            : chunkSize;
    }

    void UpdateActiveChunks()
    {
        CachePlayer();
        ResolveChunkSize();

        if (player == null || resolvedChunkSize.x <= 0f || resolvedChunkSize.y <= 0f)
            return;

        EnsureChunksAround(WorldToChunk(player.position));
    }

    void EnsureChunksAround(Vector2Int centerChunk)
    {
        int radius = preloadAdjacentChunks ? Mathf.Max(0, adjacentChunkRadius) : 0;

        for (int offsetY = -radius; offsetY <= radius; offsetY++)
        {
            for (int offsetX = -radius; offsetX <= radius; offsetX++)
            {
                var chunkCoord = new Vector2Int(centerChunk.x + offsetX, centerChunk.y + offsetY);
                TryGenerateChunk(chunkCoord);
            }
        }
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
        Sprite sprite = ResolveChunkSprite();
        if (sprite == null)
            return;

        Color color = ResolveChunkColor();
        int order = ResolveSortingOrder();
        float scaleMultiplier = ResolveSpriteScale();

        var chunkObject = new GameObject($"GroundChunk_{chunkCoord.x}_{chunkCoord.y}");
        chunkObject.transform.SetParent(chunksRoot, false);
        chunkObject.transform.position = MapChunkGrid.ChunkToWorldCenter(chunkCoord, resolvedChunkSize);

        var renderer = chunkObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = order;

        Vector2 spriteSize = sprite.bounds.size;
        if (spriteSize.x > 0.0001f && spriteSize.y > 0.0001f)
        {
            chunkObject.transform.localScale = new Vector3(
                resolvedChunkSize.x / spriteSize.x * scaleMultiplier,
                resolvedChunkSize.y / spriteSize.y * scaleMultiplier,
                1f);
        }

        chunkObjects[chunkCoord] = chunkObject;
    }

    Sprite ResolveChunkSprite()
    {
        if (mapTheme != null && mapTheme.chunkSprite != null)
            return mapTheme.chunkSprite;

        if (chunkSprite != null)
            return chunkSprite;

        if (cachedFallbackSprite == null)
            cachedFallbackSprite = CreateFallbackGroundSprite();

        return cachedFallbackSprite;
    }

    Color ResolveChunkColor()
    {
        return mapTheme != null ? mapTheme.chunkColor : chunkColor;
    }

    int ResolveSortingOrder()
    {
        return mapTheme != null ? mapTheme.sortingOrder : sortingOrder;
    }

    float ResolveSpriteScale()
    {
        return mapTheme != null ? mapTheme.spriteScale : spriteScale;
    }

    static Sprite CreateFallbackGroundSprite()
    {
        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool checker = ((x / 8) + (y / 8)) % 2 == 0;
                var color = checker
                    ? new Color(0.22f, 0.28f, 0.2f, 1f)
                    : new Color(0.18f, 0.24f, 0.17f, 1f);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void OnDrawGizmosSelected()
    {
        Vector2 size = resolvedChunkSize;
        if (size.x <= 0f || size.y <= 0f)
        {
            size = autoChunkSizeFromCamera
                ? MapChunkGrid.ResolveChunkSizeFromCamera(chunkSize, cameraChunkPadding)
                : chunkSize;
        }

        if (size.x <= 0f || size.y <= 0f)
            return;

        Vector2Int centerChunk = Application.isPlaying && player != null
            ? WorldToChunk(player.position)
            : Vector2Int.zero;

        int radius = preloadAdjacentChunks ? Mathf.Max(0, adjacentChunkRadius) : 0;
        Gizmos.color = new Color(0.35f, 0.75f, 0.45f, 0.35f);

        for (int offsetY = -radius; offsetY <= radius; offsetY++)
        {
            for (int offsetX = -radius; offsetX <= radius; offsetX++)
            {
                Vector2 chunkMin = MapChunkGrid.ChunkToWorldMin(
                    new Vector2Int(centerChunk.x + offsetX, centerChunk.y + offsetY),
                    size);
                Gizmos.DrawWireCube(chunkMin + size * 0.5f, new Vector3(size.x, size.y, 0f));
            }
        }
    }
}

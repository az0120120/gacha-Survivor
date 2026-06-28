using UnityEngine;

[AddComponentMenu("GachaSurvivor/Map Prop Grid Spawner")]
public class MapPropGridSpawner : MonoBehaviour
{
    [Header("Grid")]
    [Tooltip("网格间距，15cm = 0.15")]
    [SerializeField] float gridStep = 0.15f;
    [Tooltip("每个网格格子生成可破坏物的概率")]
    [SerializeField] [Range(0f, 1f)] float cellSpawnChance = 0.35f;
    [Tooltip("在格子内部随机偏移位置")]
    [SerializeField] bool randomizePositionInCell = true;
    [SerializeField] bool useCameraViewArea = true;
    [SerializeField] float cameraAreaPadding = 1f;
    [SerializeField] Vector2 areaSize = new Vector2(8f, 8f);
    [SerializeField] Vector2 areaCenterOffset = Vector2.zero;
    [SerializeField] float playerSpawnClearRadius = 1.2f;
    [SerializeField] Transform spawnCenter;

    [Header("Destructible")]
    [SerializeField] Sprite destructibleSprite;
    [SerializeField] float destructibleColliderRadius = 0.08f;
    [SerializeField] int destructibleSortingOrder = 2;

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

    Transform propsRoot;
    float lastBreakSoundTime;

    void Start()
    {
        EnsureStatusEffects();
        EnsurePropsRoot();
        SpawnGrid();
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

    void SpawnGrid()
    {
        if (gridStep <= 0f)
            return;

        Vector2 spawnArea = ResolveAreaSize();
        if (spawnArea.x <= 0f || spawnArea.y <= 0f)
            return;

        Vector2 center = GetSpawnCenter();
        Vector2 min = center - spawnArea * 0.5f;
        Vector2 playerPosition = GetPlayerPosition(center);
        float clearRadiusSqr = playerSpawnClearRadius * playerSpawnClearRadius;

        for (float y = min.y; y <= min.y + spawnArea.y + 0.0001f; y += gridStep)
        {
            for (float x = min.x; x <= min.x + spawnArea.x + 0.0001f; x += gridStep)
            {
                if (Random.value > cellSpawnChance)
                    continue;

                Vector2 position = new Vector2(x, y);
                if (randomizePositionInCell)
                {
                    float halfCell = gridStep * 0.45f;
                    position.x += Random.Range(-halfCell, halfCell);
                    position.y += Random.Range(-halfCell, halfCell);
                }

                if ((position - playerPosition).sqrMagnitude <= clearRadiusSqr)
                    continue;

                SpawnDestructible(position);
            }
        }
    }

    Vector2 ResolveAreaSize()
    {
        if (!useCameraViewArea)
            return areaSize;

        var camera = Camera.main;
        if (camera == null || !camera.orthographic)
            return areaSize;

        float height = camera.orthographicSize * 2f + cameraAreaPadding * 2f;
        float width = height * camera.aspect;
        return new Vector2(width, height);
    }

    Vector2 GetSpawnCenter()
    {
        if (spawnCenter != null)
            return spawnCenter.position;

        if (useCameraViewArea && Camera.main != null)
            return Camera.main.transform.position;

        return GetPlayerPosition((Vector2)transform.position + areaCenterOffset);
    }

    static Vector2 GetPlayerPosition(Vector2 fallback)
    {
        var player = GameObject.FindWithTag("Player");
        return player != null ? (Vector2)player.transform.position : fallback;
    }

    void SpawnDestructible(Vector2 position)
    {
        var propObject = new GameObject("MapDestructible");
        propObject.transform.SetParent(propsRoot, false);
        propObject.transform.position = position;

        var prop = propObject.AddComponent<MapDestructibleProp>();
        prop.Initialize(this, destructibleSprite, destructibleColliderRadius, destructibleSortingOrder);
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
        Vector2 center = spawnCenter != null ? (Vector2)spawnCenter.position : (Vector2)transform.position + areaCenterOffset;
        Vector2 size = useCameraViewArea && Camera.main != null && Camera.main.orthographic
            ? ResolveAreaSize()
            : areaSize;
        Gizmos.color = new Color(0.95f, 0.75f, 0.25f, 0.35f);
        Gizmos.DrawWireCube(center, new Vector3(size.x, size.y, 0f));
    }
}

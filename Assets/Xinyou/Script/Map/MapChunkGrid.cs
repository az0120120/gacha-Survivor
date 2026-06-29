using UnityEngine;

public static class MapChunkGrid
{
    public static Vector2 ResolveChunkSizeFromCamera(Vector2 manualSize, float cameraPadding)
    {
        var camera = Camera.main;
        if (camera == null || !camera.orthographic)
            return manualSize;

        float height = camera.orthographicSize * 2f + cameraPadding * 2f;
        float width = height * camera.aspect;
        return new Vector2(width, height);
    }

    public static Vector2Int WorldToChunk(Vector2 worldPosition, Vector2 chunkSize)
    {
        if (chunkSize.x <= 0f || chunkSize.y <= 0f)
            return Vector2Int.zero;

        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / chunkSize.x),
            Mathf.FloorToInt(worldPosition.y / chunkSize.y));
    }

    public static Vector2 ChunkToWorldMin(Vector2Int chunkCoord, Vector2 chunkSize)
    {
        return new Vector2(chunkCoord.x * chunkSize.x, chunkCoord.y * chunkSize.y);
    }

    public static Vector2 ChunkToWorldCenter(Vector2Int chunkCoord, Vector2 chunkSize)
    {
        return ChunkToWorldMin(chunkCoord, chunkSize) + chunkSize * 0.5f;
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "MapTheme", menuName = "GachaSurvivor/Map Theme")]
public class MapTheme : ScriptableObject
{
    [Header("Chunk Visual")]
    [Tooltip("每个地图区块使用的地面贴图")]
    public Sprite chunkSprite;
    public Color chunkColor = Color.white;
    public int sortingOrder = -10;
    [Tooltip("在自动适配区块大小基础上的额外缩放")]
    public float spriteScale = 1f;
}

using UnityEngine;

[CreateAssetMenu(fileName = "ShopWorldVisualSettings", menuName = "GachaSurvivor/Shop World Visual Settings")]
public class ShopWorldVisualSettings : ScriptableObject
{
    [Header("Small Shop")]
    public Sprite smallShopSprite;
    public Color smallShopColor = new Color(0.3f, 0.75f, 1f, 0.95f);
    public float smallShopScale = 1f;

    [Header("Large Shop")]
    public Sprite largeShopSprite;
    public Color largeShopColor = new Color(1f, 0.82f, 0.2f, 0.95f);
    public float largeShopScale = 1.4f;

    [Header("Shared")]
    public int sortingOrder = 8;
    public float colliderRadius = 0.75f;

    public Sprite GetSprite(ShopSizeType shopSize)
    {
        return shopSize == ShopSizeType.Large ? largeShopSprite : smallShopSprite;
    }

    public Color GetColor(ShopSizeType shopSize)
    {
        return shopSize == ShopSizeType.Large ? largeShopColor : smallShopColor;
    }

    public float GetScale(ShopSizeType shopSize)
    {
        return shopSize == ShopSizeType.Large ? largeShopScale : smallShopScale;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ShopItemIconUtility
{
    static ShopItemIconDatabase iconDatabase;
    static readonly Dictionary<string, Sprite> fallbackCache = new Dictionary<string, Sprite>();

    public static Sprite ResolveIcon(ShopItemDefinition item)
    {
        if (item == null)
            return null;

        if (item.Icon != null)
            return item.Icon;

        Sprite databaseIcon = GetDatabase()?.GetIcon(item.ItemId);
        if (databaseIcon != null)
            return databaseIcon;

        Sprite resourceIcon = Resources.Load<Sprite>($"ShopIcons/{item.ItemId}");
        if (resourceIcon != null)
            return resourceIcon;

        return GetOrCreateFallbackSprite(item);
    }

    public static void ApplyIcon(Image image, ShopItemDefinition item)
    {
        if (image == null)
            return;

        Sprite sprite = ResolveIcon(item);
        image.sprite = sprite;
        image.enabled = sprite != null;
        image.color = Color.white;
        image.preserveAspect = true;
    }

    public static Image CreateIconImage(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        var iconObject = new GameObject(objectName);
        iconObject.transform.SetParent(parent, false);

        var image = iconObject.AddComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;
        image.enabled = false;

        var rectTransform = image.rectTransform;
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        return image;
    }

    public static void SetDatabase(ShopItemIconDatabase database)
    {
        iconDatabase = database;
    }

    static ShopItemIconDatabase GetDatabase()
    {
        if (iconDatabase != null)
            return iconDatabase;

        iconDatabase = Resources.Load<ShopItemIconDatabase>("ShopItemIconDatabase");
        return iconDatabase;
    }

    static Sprite GetOrCreateFallbackSprite(ShopItemDefinition item)
    {
        string key = item.ItemId;
        if (fallbackCache.TryGetValue(key, out Sprite cached))
            return cached;

        Sprite sprite = CreateColoredSprite(GetFallbackColor(item), 64);
        fallbackCache[key] = sprite;
        return sprite;
    }

    static Color GetFallbackColor(ShopItemDefinition item)
    {
        if (item.IsWeapon)
        {
            switch (item.WeaponType)
            {
                case ShopWeaponType.Projectile:
                    return new Color(0.45f, 0.7f, 1f);
                case ShopWeaponType.Area:
                    return new Color(0.55f, 0.85f, 0.45f);
                case ShopWeaponType.DirectTarget:
                    return new Color(0.85f, 0.45f, 1f);
                case ShopWeaponType.DesertEagle:
                    return new Color(1f, 0.55f, 0.25f);
                case ShopWeaponType.Molotov:
                    return new Color(1f, 0.4f, 0.15f);
                case ShopWeaponType.Kunai:
                    return new Color(1f, 0.92f, 0.25f);
                case ShopWeaponType.Claw:
                    return new Color(1f, 0.5f, 0.45f);
                case ShopWeaponType.Ak:
                    return new Color(0.75f, 0.55f, 0.35f);
            }
        }

        if (item.Category == ShopItemCategory.WeaponMinorUpgrade)
            return new Color(0.35f, 0.65f, 0.95f);

        if (item.Category == ShopItemCategory.WeaponMajorUpgrade)
            return new Color(1f, 0.72f, 0.2f);

        switch (item.EffectType)
        {
            case ItemEffectType.AttackUp:
            case ItemEffectType.AttackDamageBonusUp:
                return new Color(0.95f, 0.45f, 0.35f);
            case ItemEffectType.ElementalAttackUp:
            case ItemEffectType.ElementalDamageBonusUp:
                return new Color(0.55f, 0.45f, 0.95f);
            case ItemEffectType.ArmorPenetrationUp:
                return new Color(0.85f, 0.55f, 0.25f);
            case ItemEffectType.MaxHealthUp:
                return new Color(0.95f, 0.35f, 0.4f);
            case ItemEffectType.MoveSpeedUp:
            case ItemEffectType.MoveSpeedPercentUp:
                return new Color(0.4f, 0.9f, 0.85f);
            case ItemEffectType.CritRateUp:
            case ItemEffectType.CritEffectUp:
                return new Color(1f, 0.85f, 0.25f);
            case ItemEffectType.DefenseUp:
                return new Color(0.65f, 0.7f, 0.8f);
            case ItemEffectType.CooldownReductionUp:
                return new Color(0.45f, 0.8f, 0.95f);
            case ItemEffectType.ExpBonusUp:
                return new Color(0.75f, 0.55f, 1f);
            case ItemEffectType.GoldBonusUp:
                return new Color(1f, 0.82f, 0.2f);
            case ItemEffectType.WeaponRangeUp:
                return new Color(0.5f, 0.85f, 0.55f);
            case ItemEffectType.WeaponCooldownUp:
                return new Color(0.95f, 0.6f, 0.35f);
            case ItemEffectType.WeaponMajorUpgrade:
                return new Color(1f, 0.55f, 0.15f);
            default:
                return new Color(0.7f, 0.7f, 0.75f);
        }
    }

    static Sprite CreateColoredSprite(Color color, int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = (size - 1) * 0.5f;
        float radius = size * 0.38f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = distance <= radius ? 1f : 0f;
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}

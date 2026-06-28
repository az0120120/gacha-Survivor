using System;
using UnityEngine;

public enum MapPropDropType
{
    MedicalNeedle,
    UsbDrive,
    Ufo,
    SitTight
}

[Serializable]
public struct MapPropDropEntry
{
    public MapPropDropType type;
    public Sprite sprite;
    [Min(0.01f)] public float spriteScale;
    [Min(0f)] public float weight;

    public static MapPropDropEntry CreateDefault(MapPropDropType type, float weight)
    {
        return new MapPropDropEntry
        {
            type = type,
            spriteScale = 1f,
            weight = weight
        };
    }
}

public static class MapPropDropTable
{
    public static MapPropDropType Roll(MapPropDropEntry[] entries)
    {
        if (entries == null || entries.Length == 0)
            return MapPropDropType.MedicalNeedle;

        float totalWeight = 0f;
        for (int i = 0; i < entries.Length; i++)
            totalWeight += Mathf.Max(0f, entries[i].weight);

        if (totalWeight <= 0f)
            return entries[0].type;

        float roll = UnityEngine.Random.value * totalWeight;
        float cumulative = 0f;

        for (int i = 0; i < entries.Length; i++)
        {
            cumulative += Mathf.Max(0f, entries[i].weight);
            if (roll <= cumulative)
                return entries[i].type;
        }

        return entries[^1].type;
    }

    public static Sprite GetSprite(MapPropDropEntry[] entries, MapPropDropType type)
    {
        return GetEntry(entries, type).sprite;
    }

    public static MapPropDropEntry GetEntry(MapPropDropEntry[] entries, MapPropDropType type)
    {
        if (entries == null || entries.Length == 0)
            return MapPropDropEntry.CreateDefault(type, 1f);

        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].type == type)
                return entries[i];
        }

        return MapPropDropEntry.CreateDefault(type, 1f);
    }
}

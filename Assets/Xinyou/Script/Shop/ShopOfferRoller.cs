using System;
using System.Collections.Generic;
using UnityEngine;

public static class ShopOfferRoller
{
    public const int OfferCount = 4;
    public const int MaxShopTier = 10;

    public static ShopItemDefinition[] RollOffers(
        IList<ShopItemDefinition> pool,
        int shopTier,
        int count,
        Predicate<ShopItemDefinition> filter)
    {
        if (pool == null || pool.Count == 0)
            return Array.Empty<ShopItemDefinition>();

        var available = new List<ShopItemDefinition>();
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null && filter(pool[i]))
                available.Add(pool[i]);
        }

        if (available.Count == 0)
            return Array.Empty<ShopItemDefinition>();

        int pickCount = Mathf.Min(count, available.Count);
        var results = new ShopItemDefinition[pickCount];

        for (int i = 0; i < pickCount; i++)
        {
            int index = PickWeightedIndex(available, shopTier);
            results[i] = available[index];
            available.RemoveAt(index);
        }

        return results;
    }

    public static int GetShopTier(int playerLevel)
    {
        return Mathf.Clamp(playerLevel, 1, MaxShopTier);
    }

    static int PickWeightedIndex(List<ShopItemDefinition> items, int shopTier)
    {
        float totalWeight = 0f;
        var weights = new float[items.Count];

        for (int i = 0; i < items.Count; i++)
        {
            weights[i] = CalculateWeight(items[i].ItemTier, shopTier);
            totalWeight += weights[i];
        }

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative)
                return i;
        }

        return weights.Length - 1;
    }

    static float CalculateWeight(int itemTier, int shopTier)
    {
        if (itemTier <= shopTier)
            return 1f + itemTier * 0.15f;

        int tierGap = itemTier - shopTier;
        return Mathf.Max(0.05f, 0.35f - tierGap * 0.08f);
    }
}

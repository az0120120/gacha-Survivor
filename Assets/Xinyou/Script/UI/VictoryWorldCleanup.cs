using UnityEngine;

public static class VictoryWorldCleanup
{
    public static void Apply()
    {
        DisableWorldSystems();
        ClearWorldEntities();
        HideGameplayHud();
    }

    static void DisableWorldSystems()
    {
        var mapGenerator = Object.FindFirstObjectByType<MapChunkGenerator>();
        mapGenerator?.StopGeneration();

        var propSpawner = Object.FindFirstObjectByType<MapPropGridSpawner>();
        propSpawner?.StopGenerationAndClearProps();

        var shopSpawner = Object.FindFirstObjectByType<ShopSpawner>();
        if (shopSpawner != null)
            shopSpawner.enabled = false;
    }

    static void ClearWorldEntities()
    {
        var pools = Object.FindObjectsByType<ObjectPool>(FindObjectsSortMode.None);
        for (int i = 0; i < pools.Length; i++)
            pools[i].ReleaseAllActive();

        if (EnemyProjectilePool.Instance != null)
            EnemyProjectilePool.Instance.ReleaseAllActive();

        ClearActiveShops();
        DestroyAll<MapPropPickup>();
        DestroyAll<MapDestructibleProp>();
        DestroyAll<MolotovFireZone>();
        DestroyAll<MolotovFlight>();
        DestroyAll<ClawSwingMotion>();
    }

    static void ClearActiveShops()
    {
        var shops = ShopWorldEntity.ActiveEntities;
        for (int i = shops.Count - 1; i >= 0; i--)
        {
            if (shops[i] != null)
                Object.Destroy(shops[i].gameObject);
        }
    }

    static void DestroyAll<T>() where T : MonoBehaviour
    {
        var items = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
                Object.Destroy(items[i].gameObject);
        }
    }

    static void HideGameplayHud()
    {
        var gameHud = Object.FindFirstObjectByType<GameHUD>();
        gameHud?.SetGameplayHudVisible(false);

        var experienceUi = Object.FindFirstObjectByType<ExperienceUI>();
        experienceUi?.SetGameplayHudVisible(false);

        if (DamageNumberManager.Instance != null)
            DamageNumberManager.Instance.SetGameplayHudVisible(false);

        var goldUi = Object.FindFirstObjectByType<GoldUI>();
        goldUi?.SetGameplayHudVisible(false);
    }
}

using UnityEngine;

public class GoldCoin : MonoBehaviour, IPoolable
{
    int goldValue;
    ObjectPool pool;
    Transform player;
    bool isMagnetized;

    public void BindPool(ObjectPool objectPool)
    {
        pool = objectPool;
    }

    public void Setup(int amount)
    {
        goldValue = amount;
        isMagnetized = false;
        CachePlayer();
    }

    public void OnGetFromPool()
    {
        isMagnetized = false;
        CachePlayer();
    }

    public void OnReturnToPool()
    {
        goldValue = 0;
        isMagnetized = false;
        player = null;
    }

    void Update()
    {
        var magnet = ExperienceMagnet.Instance;
        if (magnet == null)
            return;

        if (player == null)
            CachePlayer();

        if (player == null)
            return;

        float pickupRangeRatio = GetGoldPickupRangeRatio();
        float magnetRadius = magnet.MagnetRadius * pickupRangeRatio;
        float collectRadius = magnet.CollectRadius * pickupRangeRatio;

        Vector2 coinPosition = transform.position;
        Vector2 playerPosition = player.position;
        float distance = Vector2.Distance(coinPosition, playerPosition);

        if (distance <= collectRadius)
        {
            Collect();
            return;
        }

        if (distance <= magnetRadius)
            isMagnetized = true;

        if (!isMagnetized)
            return;

        transform.position = Vector2.MoveTowards(
            coinPosition,
            playerPosition,
            magnet.MagnetSpeed * Time.deltaTime);
    }

    float GetGoldPickupRangeRatio()
    {
        if (player == null)
            return 1f;

        var stats = player.GetComponent<CharacterStats>();
        return stats != null ? stats.GoldPickupRangeRatio : 1f;
    }

    void Collect()
    {
        if (GoldWallet.Instance != null)
            GoldWallet.Instance.AddGold(goldValue);

        if (pool != null)
            pool.Release(gameObject);
        else
            Destroy(gameObject);
    }

    public void ForceCollect()
    {
        Collect();
    }

    void CachePlayer()
    {
        if (ExperienceMagnet.Instance != null)
            player = ExperienceMagnet.Instance.transform;
    }
}

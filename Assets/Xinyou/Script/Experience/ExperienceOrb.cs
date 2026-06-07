using UnityEngine;

public class ExperienceOrb : MonoBehaviour, IPoolable
{
    int expValue;
    ObjectPool pool;
    Transform player;
    bool isMagnetized;

    public void BindPool(ObjectPool objectPool)
    {
        pool = objectPool;
    }

    public void Setup(int amount)
    {
        expValue = amount;
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
        expValue = 0;
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

        Vector2 orbPosition = transform.position;
        Vector2 playerPosition = player.position;
        float distance = Vector2.Distance(orbPosition, playerPosition);

        if (distance <= magnet.CollectRadius)
        {
            Collect();
            return;
        }

        if (distance <= magnet.MagnetRadius)
            isMagnetized = true;

        if (!isMagnetized)
            return;

        Vector2 nextPosition = Vector2.MoveTowards(
            orbPosition,
            playerPosition,
            magnet.MagnetSpeed * Time.deltaTime);

        transform.position = nextPosition;
    }

    void Collect()
    {
        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.AddExperience(expValue);

        if (pool != null)
            pool.Release(gameObject);
        else
            Destroy(gameObject);
    }

    void CachePlayer()
    {
        if (ExperienceMagnet.Instance != null)
            player = ExperienceMagnet.Instance.transform;
    }
}

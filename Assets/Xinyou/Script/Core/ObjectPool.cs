using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] int prewarmCount = 20;
    [SerializeField] Transform poolRoot;

    readonly Queue<GameObject> available = new Queue<GameObject>();
    readonly HashSet<GameObject> active = new HashSet<GameObject>();

    public int ActiveCount => active.Count;

    void Awake()
    {
        if (poolRoot == null)
        {
            var rootObject = new GameObject("PoolRoot");
            poolRoot = rootObject.transform;
            poolRoot.SetParent(transform, false);
        }

        Prewarm();
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        GameObject instance = available.Count > 0
            ? available.Dequeue()
            : CreateInstance();

        instance.transform.SetParent(null, true);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        active.Add(instance);
        NotifyGetFromPool(instance);
        return instance;
    }

    public void Release(GameObject instance)
    {
        if (instance == null || !active.Contains(instance))
            return;

        NotifyReturnToPool(instance);
        instance.SetActive(false);
        instance.transform.SetParent(poolRoot, false);
        active.Remove(instance);
        available.Enqueue(instance);
    }

    void Prewarm()
    {
        for (int i = 0; i < prewarmCount; i++)
        {
            var instance = CreateInstance();
            instance.SetActive(false);
            available.Enqueue(instance);
        }
    }

    GameObject CreateInstance()
    {
        var instance = Instantiate(prefab, poolRoot);
        instance.SetActive(false);

        var enemyHealth = instance.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.BindPool(this);

        var experienceOrb = instance.GetComponent<ExperienceOrb>();
        if (experienceOrb != null)
            experienceOrb.BindPool(this);

        var projectile = instance.GetComponent<Projectile>();
        if (projectile != null)
            projectile.BindPool(this);

        var goldCoin = instance.GetComponent<GoldCoin>();
        if (goldCoin != null)
            goldCoin.BindPool(this);

        return instance;
    }

    void NotifyGetFromPool(GameObject instance)
    {
        var poolables = instance.GetComponentsInChildren<IPoolable>(true);
        for (int i = 0; i < poolables.Length; i++)
            poolables[i].OnGetFromPool();
    }

    void NotifyReturnToPool(GameObject instance)
    {
        var poolables = instance.GetComponentsInChildren<IPoolable>(true);
        for (int i = 0; i < poolables.Length; i++)
            poolables[i].OnReturnToPool();
    }
}

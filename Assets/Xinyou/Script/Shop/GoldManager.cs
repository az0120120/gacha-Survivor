using UnityEngine;

[DefaultExecutionOrder(-100)]
[AddComponentMenu("GachaSurvivor/Gold Manager")]
public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    [SerializeField] ObjectPool coinPool;
    [SerializeField] int defaultCoinValue = 1;
    [SerializeField] float dropScatterRadius = 0.2f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (coinPool == null)
            coinPool = GetComponent<ObjectPool>();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SpawnCoin(Vector3 position, int amount)
    {
        if (coinPool == null)
            return;

        if (amount <= 0)
            amount = defaultCoinValue;

        if (dropScatterRadius > 0f)
        {
            var scatter = Random.insideUnitCircle * dropScatterRadius;
            position += new Vector3(scatter.x, scatter.y, 0f);
        }

        var coinObject = coinPool.Get(position, Quaternion.identity);
        if (coinObject == null)
            return;

        var coin = coinObject.GetComponent<GoldCoin>();
        if (coin != null)
            coin.Setup(amount);
    }
}

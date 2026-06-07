using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [SerializeField] ObjectPool orbPool;
    [SerializeField] int defaultOrbValue = 1;
    [SerializeField] float dropScatterRadius = 0.25f;

    public int TotalExperience { get; private set; }

    public event Action<int> OnExperienceChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (orbPool == null)
            orbPool = GetComponent<ObjectPool>();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SpawnOrb(Vector3 position, int amount)
    {
        if (orbPool == null)
            return;

        if (amount <= 0)
            amount = defaultOrbValue;

        if (dropScatterRadius > 0f)
        {
            var scatter = UnityEngine.Random.insideUnitCircle * dropScatterRadius;
            position += new Vector3(scatter.x, scatter.y, 0f);
        }

        var orbObject = orbPool.Get(position, Quaternion.identity);
        if (orbObject == null)
            return;

        var orb = orbObject.GetComponent<ExperienceOrb>();
        if (orb != null)
            orb.Setup(amount);
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        TotalExperience += amount;
        OnExperienceChanged?.Invoke(TotalExperience);
    }
}

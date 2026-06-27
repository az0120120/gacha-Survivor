using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [SerializeField] ObjectPool orbPool;
    [SerializeField] int defaultOrbValue = 1;
    [SerializeField] float dropScatterRadius = 0.25f;
    [SerializeField] int baseExpRequired = 15;
    [SerializeField] float expGrowth = 1.18f;

    int level = 1;
    int currentExp;

    public int Level => level;
    public int CurrentExp => currentExp;
    public int ExpToNextLevel => CalculateExpRequired(level);

    public event Action<int> OnExperienceChanged;
    public event Action<int> OnLevelChanged;
    public event Action<int, int> OnExpProgressChanged;

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

    void Start()
    {
        NotifyExpProgress();
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

        amount = ApplyExpBonus(amount);
        currentExp += amount;

        int levelUps = 0;
        while (currentExp >= ExpToNextLevel)
        {
            currentExp -= ExpToNextLevel;
            level++;
            levelUps++;
            OnLevelChanged?.Invoke(level);
        }

        OnExperienceChanged?.Invoke(currentExp);
        NotifyExpProgress();

        if (levelUps > 0 && LevelUpManager.Instance != null)
            LevelUpManager.Instance.QueueLevelUps(levelUps);
    }

    static int ApplyExpBonus(int amount)
    {
        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            return amount;

        var stats = playerObject.GetComponent<CharacterStats>();
        if (stats == null)
            return amount;

        return StatMath.FloorToInt(amount * stats.ExpBonusRatio);
    }

    int CalculateExpRequired(int currentLevel)
    {
        return StatMath.FloorToInt(baseExpRequired * Mathf.Pow(expGrowth, currentLevel));
    }

    void NotifyExpProgress()
    {
        OnExpProgressChanged?.Invoke(currentExp, ExpToNextLevel);
    }
}

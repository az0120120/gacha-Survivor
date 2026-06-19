using System;
using UnityEngine;

public class GoldWallet : MonoBehaviour
{
    public static GoldWallet Instance { get; private set; }

    [SerializeField] int startingGold;

    int gold;

    public int Gold => gold;

    public event Action<int> OnGoldChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gold = startingGold;
    }

    void Start()
    {
        OnGoldChanged?.Invoke(gold);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        amount = ApplyGoldBonus(amount);
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    static int ApplyGoldBonus(int amount)
    {
        if (Instance == null)
            return amount;

        var stats = Instance.GetComponent<CharacterStats>();
        if (stats == null)
        {
            var playerObject = GameObject.FindWithTag("Player");
            stats = playerObject != null ? playerObject.GetComponent<CharacterStats>() : null;
        }

        if (stats == null)
            return amount;

        return StatMath.FloorToInt(amount * stats.GoldBonusRatio);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || gold < amount)
            return false;

        gold -= amount;
        OnGoldChanged?.Invoke(gold);
        return true;
    }
}

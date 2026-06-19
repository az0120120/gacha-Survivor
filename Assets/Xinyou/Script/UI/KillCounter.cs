using System;
using UnityEngine;

[AddComponentMenu("GachaSurvivor/Kill Counter")]
public class KillCounter : MonoBehaviour
{
    public static KillCounter Instance { get; private set; }

    int killCount;

    public int KillCount => killCount;

    public event Action<int> OnKillCountChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RegisterKill()
    {
        killCount++;
        OnKillCountChanged?.Invoke(killCount);
    }
}

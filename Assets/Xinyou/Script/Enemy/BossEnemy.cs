using UnityEngine;

public class BossEnemy : MonoBehaviour, IPoolable
{
    BossDefinition definition;
    int bossIndex = -1;

    public int BossIndex => bossIndex;
    public bool IsConfigured => definition != null && bossIndex >= 0;
    public string DisplayName => definition != null ? definition.displayName : "Boss";

    public void Configure(BossDefinition bossDefinition, int index, EnemyCatalog enemyCatalog)
    {
        definition = bossDefinition;
        bossIndex = index;

        var stats = GetComponent<EnemyStats>();
        var ai = GetComponent<EnemyAI>();
        if (stats != null)
        {
            stats.ApplyBossDefinition(bossDefinition, enemyCatalog);
            ai?.ApplyArchetypeConfig(stats.Definition);
        }

        var health = GetComponent<EnemyHealth>();
        health?.RefreshHealthFromStats();

        if (definition != null)
            gameObject.name = definition.displayName;
    }

    public void OnGetFromPool()
    {
    }

    public void OnReturnToPool()
    {
        definition = null;
        bossIndex = -1;
        gameObject.name = "Boss";
    }
}

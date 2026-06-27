using System;
using UnityEngine;

[Serializable]
public class BossDefinition
{
    public string displayName = "Boss";
    public Sprite sprite;
    public Color tintColor = Color.white;
    public Vector3 localScale = new Vector3(0.45f, 0.45f, 0.45f);
    public EnemyArchetype behaviorArchetype = EnemyArchetype.MeleeRush;
    public float healthMultiplier = 25f;
    public int bonusAttack = 12;
    public int bonusDefense = 250;
    public int expDrop = 40;
    public int goldDrop = 60;
}

[CreateAssetMenu(fileName = "BossCatalog", menuName = "GachaSurvivor/Boss Catalog")]
public class BossCatalog : ScriptableObject
{
    [SerializeField] BossDefinition[] bosses;

    public int BossCount => bosses != null ? bosses.Length : 0;

    public BossDefinition GetBoss(int index)
    {
        if (bosses == null || index < 0 || index >= bosses.Length)
            return null;

        return bosses[index];
    }

    public void SetBosses(BossDefinition[] definitions)
    {
        bosses = definitions;
    }

    public static BossDefinition[] CreateDefaultDefinitions()
    {
        return new[]
        {
            CreateDefault("Boss I - 裂地巨兽", new Color(1f, 0.35f, 0.35f), 22f, EnemyArchetype.MeleeRush),
            CreateDefault("Boss II - 炽焰射手", new Color(1f, 0.55f, 0.2f), 24f, EnemyArchetype.RangedShooter),
            CreateDefault("Boss III - 疾风掠影", new Color(0.55f, 0.85f, 1f), 26f, EnemyArchetype.Harasser),
            CreateDefault("Boss IV - 终焉领主", new Color(0.75f, 0.35f, 1f), 30f, EnemyArchetype.MeleeRush)
        };
    }

    static BossDefinition CreateDefault(string name, Color tint, float healthMultiplier, EnemyArchetype archetype)
    {
        return new BossDefinition
        {
            displayName = name,
            tintColor = tint,
            localScale = new Vector3(0.5f, 0.5f, 0.5f),
            behaviorArchetype = archetype,
            healthMultiplier = healthMultiplier,
            bonusAttack = 10 + StatMath.FloorToInt(healthMultiplier * 0.5f),
            bonusDefense = 200 + StatMath.FloorToInt(healthMultiplier * 10f),
            expDrop = 30 + StatMath.FloorToInt(healthMultiplier),
            goldDrop = 45 + StatMath.FloorToInt(healthMultiplier * 1.5f)
        };
    }
}

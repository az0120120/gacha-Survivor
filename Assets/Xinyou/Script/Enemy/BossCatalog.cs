using System;
using UnityEngine;

public enum BossBehaviorType
{
    Xiaoxia,
    Fazeniko,
    S1mple,
    G2niko,
    FalconNiko
}

[Serializable]
public class BossDefinition
{
    public string displayName = "Boss";
    public Sprite sprite;
    public Color tintColor = Color.white;
    public Vector3 localScale = new Vector3(0.45f, 0.45f, 0.45f);
    public BossBehaviorType behaviorType = BossBehaviorType.Xiaoxia;
    public float healthMultiplier = 15f;
    [Tooltip("相对普通怪物的金币掉落倍率")]
    public int goldDropMultiplier = 10;
    public float shootInterval = 3f;
    [Tooltip("扇形弹幕半角（度）")]
    public float fanHalfAngle = 35f;
    public int fanBulletCount = 5;
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
            Create("小虾", BossBehaviorType.Xiaoxia, 15f, 3f),
            Create("fazeniko", BossBehaviorType.Fazeniko, 15f, 3f),
            Create("s1mple", BossBehaviorType.S1mple, 10f, 0f),
            Create("g2niko", BossBehaviorType.G2niko, 25f, 2f),
            Create("猎鹰niko", BossBehaviorType.FalconNiko, 40f, 0.5f)
        };
    }

    static BossDefinition Create(string name, BossBehaviorType type, float healthMultiplier, float shootInterval)
    {
        var definition = new BossDefinition
        {
            displayName = name,
            behaviorType = type,
            healthMultiplier = healthMultiplier,
            goldDropMultiplier = 10,
            shootInterval = shootInterval,
            localScale = new Vector3(0.55f, 0.55f, 0.55f)
        };

        switch (type)
        {
            case BossBehaviorType.Xiaoxia:
                definition.tintColor = new Color(1f, 0.55f, 0.45f);
                break;
            case BossBehaviorType.Fazeniko:
                definition.tintColor = new Color(1f, 0.75f, 0.25f);
                break;
            case BossBehaviorType.S1mple:
                definition.tintColor = new Color(0.55f, 0.85f, 1f);
                definition.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                break;
            case BossBehaviorType.G2niko:
                definition.tintColor = new Color(0.85f, 0.35f, 0.35f);
                definition.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                break;
            case BossBehaviorType.FalconNiko:
                definition.tintColor = new Color(0.65f, 0.35f, 1f);
                definition.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                definition.fanHalfAngle = 40f;
                definition.fanBulletCount = 7;
                break;
        }

        return definition;
    }
}

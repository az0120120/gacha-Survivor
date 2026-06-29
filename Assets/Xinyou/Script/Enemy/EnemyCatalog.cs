using System;
using UnityEngine;

[Serializable]
public class EnemyArchetypeDefinition
{
    [Header("Identity")]
    public EnemyArchetype archetype = EnemyArchetype.MeleeRush;
    public string displayName = "怪物";

    [Header("Visual")]
    public Sprite sprite;
    public Color tintColor = Color.white;
    public Vector3 localScale = Vector3.one;

    [Header("Combat Stats")]
    public float baseMaxHealth = 80f;
    public float healthScalePerMinute = 1.18f;
    public int baseAttack = 3;
    public int attackBonusPerMinute = 1;
    public int baseDefense;
    public int defensePerMinute = 50;

    [Header("Drops & Contact")]
    public int expDrop = 3;
    public int goldDrop = 5;
    public float contactCooldown = 1f;

    [Header("Melee Rush")]
    public float meleeMoveSpeed = 2.8f;

    [Header("Ranged Shooter")]
    public float rangedMoveSpeed = 1.6f;
    public float preferredRange = 7f;
    public float minRange = 4.5f;
    public float shootInterval = 1.4f;
    public float projectileSpeed = 7f;
    public Sprite projectileSprite;
    public Color projectileColor = new Color(1f, 0.35f, 0.35f, 0.95f);
    [Tooltip("子弹在世界中的缩放倍率。可在 EnemyCatalog 里按怪物类型分别调节，数值越小子弹越小。")]
    [Range(0.05f, 2f)]
    public float projectileScale = 0.15f;

    [Header("Harasser")]
    public float harasserCruiseSpeed = 1.4f;
    public float harasserDashSpeed = 7f;
    public float harasserPauseDuration = 0.55f;
    public float harasserDashDuration = 0.4f;
    public float harasserRecoverDuration = 0.75f;

    public Color GetFallbackTint()
    {
        switch (archetype)
        {
            case EnemyArchetype.MeleeRush:
                return new Color(1f, 0.45f, 0.45f);
            case EnemyArchetype.RangedShooter:
                return new Color(0.45f, 0.65f, 1f);
            case EnemyArchetype.Harasser:
                return new Color(1f, 0.85f, 0.3f);
            default:
                return Color.white;
        }
    }
}

[CreateAssetMenu(fileName = "EnemyCatalog", menuName = "GachaSurvivor/Enemy Catalog")]
public class EnemyCatalog : ScriptableObject
{
    [SerializeField] EnemyArchetypeDefinition[] archetypes;

    public EnemyArchetypeDefinition GetDefinition(EnemyArchetype type)
    {
        if (archetypes == null)
            return null;

        for (int i = 0; i < archetypes.Length; i++)
        {
            if (archetypes[i] != null && archetypes[i].archetype == type)
                return archetypes[i];
        }

        return null;
    }

    public void SetArchetypes(EnemyArchetypeDefinition[] definitions)
    {
        archetypes = definitions;
    }

    public static EnemyArchetypeDefinition[] CreateDefaultDefinitions()
    {
        return new[]
        {
            CreateDefault(EnemyArchetype.MeleeRush, "近战直冲", 80f, new Color(1f, 0.45f, 0.45f)),
            CreateRangedDefault(),
            CreateHarasserDefault()
        };
    }

    static EnemyArchetypeDefinition CreateDefault(EnemyArchetype type, string name, float health, Color tint)
    {
        return new EnemyArchetypeDefinition
        {
            archetype = type,
            displayName = name,
            tintColor = tint,
            baseMaxHealth = health,
            baseAttack = 3,
            expDrop = 3,
            goldDrop = 5
        };
    }

    static EnemyArchetypeDefinition CreateRangedDefault()
    {
        var definition = CreateDefault(EnemyArchetype.RangedShooter, "远程射击", 40f, new Color(0.45f, 0.65f, 1f));
        definition.projectileScale = 0.15f;
        return definition;
    }

    static EnemyArchetypeDefinition CreateHarasserDefault()
    {
        var definition = CreateDefault(EnemyArchetype.Harasser, "高速骚扰", 55f, new Color(1f, 0.85f, 0.3f));
        definition.meleeMoveSpeed = 1.4f;
        definition.harasserDashSpeed = 12f;
        definition.harasserDashDuration = 0.6f;
        return definition;
    }
}

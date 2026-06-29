using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] EnemyArchetype archetype = EnemyArchetype.MeleeRush;

    EnemyArchetypeDefinition definition;
    int maxHealth;
    int attack;
    int defense;
    int goldDrop;
    int expDrop;

    public EnemyArchetype Archetype => archetype;
    public EnemyArchetypeDefinition Definition => definition;
    public int MaxHealth => maxHealth;
    public int Attack => attack;
    public int Defense => defense;
    public int ExpDrop => expDrop;
    public int GoldDrop => goldDrop;
    public float ContactCooldown => definition != null ? definition.contactCooldown : 1f;

    public void SetArchetype(EnemyArchetype newArchetype, EnemyCatalog catalog)
    {
        archetype = newArchetype;
        definition = catalog != null ? catalog.GetDefinition(newArchetype) : null;
        RefreshFromGameTime();
        ApplyArchetypeVisual();
    }

    public void RefreshFromGameTime()
    {
        int gameMinutes = GameTimeManager.Instance != null ? GameTimeManager.Instance.GameMinutes : 0;

        if (definition != null)
        {
            maxHealth = StatMath.FloorToInt(
                definition.baseMaxHealth * Mathf.Pow(definition.healthScalePerMinute, gameMinutes));
            attack = definition.baseAttack + definition.attackBonusPerMinute * gameMinutes;
            defense = definition.baseDefense + definition.defensePerMinute * gameMinutes;
        }
        else
        {
            maxHealth = EnemyStatScaler.GetMaxHealth(archetype, gameMinutes);
            attack = EnemyStatScaler.GetAttack(gameMinutes);
            defense = EnemyStatScaler.GetDefense(gameMinutes);
        }

        goldDrop = EnemyStatScaler.RollGoldDrop(gameMinutes);
        expDrop = EnemyStatScaler.GetExpDropFromGold(goldDrop);
    }

    public void ApplyBossDefinition(BossDefinition bossDefinition, EnemyCatalog catalog)
    {
        if (bossDefinition == null)
            return;

        int gameMinutes = GameTimeManager.Instance != null ? GameTimeManager.Instance.GameMinutes : 0;
        bool usesRangedAttack = bossDefinition.behaviorType == BossBehaviorType.Fazeniko
            || bossDefinition.behaviorType == BossBehaviorType.G2niko
            || bossDefinition.behaviorType == BossBehaviorType.FalconNiko;

        archetype = usesRangedAttack ? EnemyArchetype.RangedShooter : EnemyArchetype.MeleeRush;
        definition = catalog != null ? catalog.GetDefinition(archetype) : null;

        maxHealth = StatMath.FloorToInt(
            EnemyStatScaler.GetMaxHealth(EnemyArchetype.MeleeRush, gameMinutes) * bossDefinition.healthMultiplier);
        attack = usesRangedAttack
            ? GetRangedSmallEnemyAttack(catalog, gameMinutes)
            : EnemyStatScaler.GetAttack(gameMinutes);
        defense = EnemyStatScaler.GetDefense(gameMinutes);

        int normalGold = EnemyStatScaler.RollGoldDrop(gameMinutes);
        goldDrop = normalGold * Mathf.Max(1, bossDefinition.goldDropMultiplier);
        expDrop = EnemyStatScaler.GetExpDropFromGold(goldDrop);

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            return;

        if (bossDefinition.sprite != null)
            spriteRenderer.sprite = bossDefinition.sprite;
        else if (definition != null && definition.sprite != null)
            spriteRenderer.sprite = definition.sprite;

        spriteRenderer.color = bossDefinition.tintColor;
        transform.localScale = bossDefinition.localScale;
    }

    static int GetRangedSmallEnemyAttack(EnemyCatalog catalog, int gameMinutes)
    {
        var rangedDefinition = catalog != null ? catalog.GetDefinition(EnemyArchetype.RangedShooter) : null;
        if (rangedDefinition == null)
            return EnemyStatScaler.GetAttack(gameMinutes);

        return rangedDefinition.baseAttack + rangedDefinition.attackBonusPerMinute * gameMinutes;
    }

    public void ApplyArchetypeVisual()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            return;

        if (definition != null && definition.sprite != null)
            spriteRenderer.sprite = definition.sprite;

        spriteRenderer.color = definition != null ? definition.tintColor : GetFallbackTint();
        transform.localScale = definition != null ? definition.localScale : Vector3.one;
    }

    Color GetFallbackTint()
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

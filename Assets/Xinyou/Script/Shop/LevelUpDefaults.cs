using UnityEngine;

public static class LevelUpDefaults
{
    public static ShopItemDefinition[] CreateRuntimeCatalog()
    {
        return new[]
        {
            CreateStat("lv_attack", "强攻", "攻击力 +15。", ItemEffectType.AttackUp, 1, 15f),
            CreateStat("lv_element", "元素之力", "元素攻击力 +5。", ItemEffectType.ElementalAttackUp, 1, 5f),
            CreateStat("lv_armor_pen", "破甲", "破防 +25。", ItemEffectType.ArmorPenetrationUp, 2, 25f),
            CreateStat("lv_defense", "护甲", "防御 +1。", ItemEffectType.DefenseUp, 1, 1f),
            CreateStat("lv_health", "生命", "最大生命 +15，并立即恢复。", ItemEffectType.MaxHealthUp, 1, 15f),
            CreateStat("lv_crit_rate", "精准", "暴击率 +3%。", ItemEffectType.CritRateUp, 2, 3f),
            CreateStat("lv_crit_effect", "致命", "暴击效果 +10%。", ItemEffectType.CritEffectUp, 2, 10f),
            CreateStat("lv_speed_pct", "迅捷", "移动速度 +3%。", ItemEffectType.MoveSpeedPercentUp, 1, 3f)
        };
    }

    static ShopItemDefinition CreateStat(
        string id,
        string name,
        string desc,
        ItemEffectType effectType,
        int tier,
        float value)
    {
        var item = ScriptableObject.CreateInstance<ShopItemDefinition>();
        item.ConfigureRuntime(id, name, desc, ItemPoolType.LevelUp, ShopItemCategory.StatUpgrade, effectType, tier, 0, ShopSizeType.Small, false, ShopWeaponType.DesertEagle, value);
        return item;
    }
}

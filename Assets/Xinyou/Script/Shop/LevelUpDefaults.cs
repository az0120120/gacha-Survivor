using UnityEngine;

public static class LevelUpDefaults
{
    public static ShopItemDefinition[] CreateRuntimeCatalog()
    {
        return new[]
        {
            CreateStat("lv_attack", "力量提升", "攻击力 +3。", ItemEffectType.AttackUp, 1, 3f),
            CreateStat("lv_attack2", "强力打击", "攻击力 +6。", ItemEffectType.AttackUp, 3, 6f),
            CreateStat("lv_element", "元素亲和", "元素攻击力 +4。", ItemEffectType.ElementalAttackUp, 2, 4f),
            CreateStat("lv_health", "生命强化", "最大生命 +15，并立即恢复。", ItemEffectType.MaxHealthUp, 1, 15f),
            CreateStat("lv_health2", "坚韧体魄", "最大生命 +30，并立即恢复。", ItemEffectType.MaxHealthUp, 4, 30f),
            CreateStat("lv_speed", "迅捷", "移动速度 +0.5。", ItemEffectType.MoveSpeedUp, 1, 0.5f),
            CreateStat("lv_crit_rate", "精准", "暴击率 +3%。", ItemEffectType.CritRateUp, 2, 3f),
            CreateStat("lv_crit_effect", "致命", "暴击效果 +15%。", ItemEffectType.CritEffectUp, 3, 15f),
            CreateStat("lv_defense", "铁壁", "防御 +5。", ItemEffectType.DefenseUp, 2, 5f),
            CreateStat("lv_cdr", "冷却精通", "冷却缩减 +3%。", ItemEffectType.CooldownReductionUp, 2, 3f),
            CreateStat("lv_exp", "学者", "经验获取 +10%。", ItemEffectType.ExpBonusUp, 2, 10f),
            CreateStat("lv_gold", "贪婪", "金币获取 +10%。", ItemEffectType.GoldBonusUp, 2, 10f),
            CreateStat("lv_ad_bonus", "攻势", "攻击增伤 +5%。", ItemEffectType.AttackDamageBonusUp, 3, 5f),
            CreateStat("lv_elem_bonus", "元素增幅", "元素攻击增伤 +8%。", ItemEffectType.ElementalDamageBonusUp, 4, 8f)
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
        item.ConfigureRuntime(id, name, desc, ItemPoolType.LevelUp, ShopItemCategory.StatUpgrade, effectType, tier, 0, ShopSizeType.Small, false, ShopWeaponType.Projectile, value);
        return item;
    }
}

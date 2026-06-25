using System.Collections.Generic;
using UnityEngine;

public enum GameItemKind
{
    [InspectorName("商店 - 武器")]
    ShopWeapon,
    [InspectorName("商店 - 武器范围强化")]
    ShopWeaponRangeUp,
    [InspectorName("商店 - 武器冷却强化")]
    ShopWeaponCooldownUp,
    [InspectorName("商店 - 武器大强化")]
    ShopWeaponMajorUpgrade,
    [InspectorName("升级 - 属性强化")]
    LevelUpStat
}

public enum LevelUpStatType
{
    [InspectorName("攻击力")]
    Attack,
    [InspectorName("元素攻击")]
    ElementalAttack,
    [InspectorName("最大生命")]
    MaxHealth,
    [InspectorName("移动速度")]
    MoveSpeed,
    [InspectorName("暴击率")]
    CritRate,
    [InspectorName("暴击效果")]
    CritEffect,
    [InspectorName("防御")]
    Defense,
    [InspectorName("冷却缩减")]
    CooldownReduction,
    [InspectorName("经验加成")]
    ExpBonus,
    [InspectorName("金币加成")]
    GoldBonus,
    [InspectorName("攻击增伤")]
    AttackDamageBonus,
    [InspectorName("元素增伤")]
    ElementalDamageBonus
}

[System.Serializable]
public class GameItemEntry
{
    [Tooltip("唯一 ID，建议英文，如 weapon_projectile")]
    public string id = "new_item";

    public string displayName = "新道具";

    [TextArea(2, 4)]
    public string description = "道具说明";

    public Sprite icon;

    public GameItemKind kind = GameItemKind.ShopWeapon;

    [Tooltip("仅「升级 - 属性强化」需要选择")]
    public LevelUpStatType statType = LevelUpStatType.Attack;

    [Tooltip("商店价格；升级道具可填 0")]
    public int price = 50;

    [Tooltip("品级，影响商店刷新权重")]
    public int tier = 1;

    [Tooltip("关联武器（武器/武器强化时填写）")]
    public ShopWeaponType weapon = ShopWeaponType.Projectile;

    [Tooltip("强化数值：范围%、冷却%、属性加成等")]
    public float effectValue = 10f;

    public bool purchaseOnce = true;

    public ShopItemDefinition ToDefinition()
    {
        var item = ScriptableObject.CreateInstance<ShopItemDefinition>();
        ApplyToDefinition(item);
        return item;
    }

    public void ApplyToDefinition(ShopItemDefinition item)
    {
        if (item == null)
            return;

        ResolveMapping(
            out ItemPoolType pool,
            out ShopItemCategory category,
            out ItemEffectType effect,
            out ShopSizeType shopSize,
            out bool once);

        item.ConfigureRuntime(
            id,
            displayName,
            description,
            pool,
            category,
            effect,
            tier,
            price,
            shopSize,
            once,
            weapon,
            effectValue,
            0f,
            icon);
    }

    void ResolveMapping(
        out ItemPoolType pool,
        out ShopItemCategory category,
        out ItemEffectType effect,
        out ShopSizeType shopSize,
        out bool once)
    {
        switch (kind)
        {
            case GameItemKind.ShopWeapon:
                pool = ItemPoolType.Shop;
                category = ShopItemCategory.Weapon;
                effect = ItemEffectType.EquipWeapon;
                shopSize = ShopSizeType.Large;
                once = purchaseOnce;
                return;

            case GameItemKind.ShopWeaponRangeUp:
                pool = ItemPoolType.Shop;
                category = ShopItemCategory.WeaponMinorUpgrade;
                effect = ItemEffectType.WeaponRangeUp;
                shopSize = ShopSizeType.Small;
                once = false;
                return;

            case GameItemKind.ShopWeaponCooldownUp:
                pool = ItemPoolType.Shop;
                category = ShopItemCategory.WeaponMinorUpgrade;
                effect = ItemEffectType.WeaponCooldownUp;
                shopSize = ShopSizeType.Small;
                once = false;
                return;

            case GameItemKind.ShopWeaponMajorUpgrade:
                pool = ItemPoolType.Shop;
                category = ShopItemCategory.WeaponMajorUpgrade;
                effect = ItemEffectType.WeaponMajorUpgrade;
                shopSize = ShopSizeType.Large;
                once = false;
                return;

            case GameItemKind.LevelUpStat:
                pool = ItemPoolType.LevelUp;
                category = ShopItemCategory.StatUpgrade;
                effect = MapStatType(statType);
                shopSize = ShopSizeType.Small;
                once = false;
                return;
        }

        pool = ItemPoolType.Shop;
        category = ShopItemCategory.Weapon;
        effect = ItemEffectType.EquipWeapon;
        shopSize = ShopSizeType.Large;
        once = purchaseOnce;
    }

    static ItemEffectType MapStatType(LevelUpStatType statType)
    {
        switch (statType)
        {
            case LevelUpStatType.Attack: return ItemEffectType.AttackUp;
            case LevelUpStatType.ElementalAttack: return ItemEffectType.ElementalAttackUp;
            case LevelUpStatType.MaxHealth: return ItemEffectType.MaxHealthUp;
            case LevelUpStatType.MoveSpeed: return ItemEffectType.MoveSpeedUp;
            case LevelUpStatType.CritRate: return ItemEffectType.CritRateUp;
            case LevelUpStatType.CritEffect: return ItemEffectType.CritEffectUp;
            case LevelUpStatType.Defense: return ItemEffectType.DefenseUp;
            case LevelUpStatType.CooldownReduction: return ItemEffectType.CooldownReductionUp;
            case LevelUpStatType.ExpBonus: return ItemEffectType.ExpBonusUp;
            case LevelUpStatType.GoldBonus: return ItemEffectType.GoldBonusUp;
            case LevelUpStatType.AttackDamageBonus: return ItemEffectType.AttackDamageBonusUp;
            case LevelUpStatType.ElementalDamageBonus: return ItemEffectType.ElementalDamageBonusUp;
            default: return ItemEffectType.AttackUp;
        }
    }
}

[CreateAssetMenu(fileName = "GameItemCatalog", menuName = "GachaSurvivor/Game Item Catalog")]
public class GameItemCatalog : ScriptableObject
{
    [SerializeField] List<GameItemEntry> entries = new List<GameItemEntry>();

    public IReadOnlyList<GameItemEntry> Entries => entries;

    public ShopItemDefinition[] BuildShopCatalog()
    {
        return BuildFiltered(item => item.kind != GameItemKind.LevelUpStat);
    }

    public ShopItemDefinition[] BuildLevelUpCatalog()
    {
        return BuildFiltered(item => item.kind == GameItemKind.LevelUpStat);
    }

    public ShopItemDefinition[] BuildAll()
    {
        return BuildFiltered(_ => true);
    }

    ShopItemDefinition[] BuildFiltered(System.Func<GameItemEntry, bool> predicate)
    {
        if (entries == null || entries.Count == 0)
            return System.Array.Empty<ShopItemDefinition>();

        var result = new List<ShopItemDefinition>();
        for (int i = 0; i < entries.Count; i++)
        {
            GameItemEntry entry = entries[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.id) || !predicate(entry))
                continue;

            result.Add(entry.ToDefinition());
        }

        return result.ToArray();
    }

    public void SetEntries(List<GameItemEntry> newEntries)
    {
        entries = newEntries ?? new List<GameItemEntry>();
    }

    public static List<GameItemEntry> CreateDefaultEntries()
    {
        return new List<GameItemEntry>
        {
            Entry("weapon_projectile", "魔弹杖", "装备后每 0.5 秒发射子弹，造成 4 倍伤害。", GameItemKind.ShopWeapon, 75, 2, ShopWeaponType.Projectile),
            Entry("weapon_area", "旋转法阵", "装备后以自身为中心持续造成区域伤害。", GameItemKind.ShopWeapon, 80, 3, ShopWeaponType.Area),
            Entry("weapon_direct", "虚空指", "装备后锁定最近敌人，每秒造成 6 倍瞬发伤害。", GameItemKind.ShopWeapon, 90, 3, ShopWeaponType.DirectTarget),

            Minor("w_range_projectile", "延伸弹匣", "魔弹杖攻击范围 +20%。", ShopWeaponType.Projectile, GameItemKind.ShopWeaponRangeUp, 35, 1, 20f),
            Minor("w_cd_projectile", "速射核心", "魔弹杖冷却缩减 +10%。", ShopWeaponType.Projectile, GameItemKind.ShopWeaponCooldownUp, 35, 1, 10f),
            Minor("w_range_area", "法阵扩张", "旋转法阵范围 +25%。", ShopWeaponType.Area, GameItemKind.ShopWeaponRangeUp, 40, 2, 25f),
            Minor("w_cd_area", "法阵加速", "旋转法阵冷却缩减 +10%。", ShopWeaponType.Area, GameItemKind.ShopWeaponCooldownUp, 40, 2, 10f),
            Minor("w_range_direct", "虚空延伸", "虚空指索敌范围 +20%。", ShopWeaponType.DirectTarget, GameItemKind.ShopWeaponRangeUp, 45, 2, 20f),
            Minor("w_cd_direct", "虚空加速", "虚空指冷却缩减 +10%。", ShopWeaponType.DirectTarget, GameItemKind.ShopWeaponCooldownUp, 45, 2, 10f),

            Entry("w_major_projectile", "三重散射", "魔弹杖改为三向散射攻击。", GameItemKind.ShopWeaponMajorUpgrade, 120, 1, ShopWeaponType.Projectile),
            Entry("w_major_area", "双环法阵", "旋转法阵频率提升，伤害范围扩大。", GameItemKind.ShopWeaponMajorUpgrade, 130, 2, ShopWeaponType.Area),
            Entry("w_major_direct", "连锁虚空", "虚空指改为范围连锁打击。", GameItemKind.ShopWeaponMajorUpgrade, 140, 3, ShopWeaponType.DirectTarget),

            Stat("lv_attack", "力量提升", "攻击力 +3。", LevelUpStatType.Attack, 1, 3f),
            Stat("lv_attack2", "强力打击", "攻击力 +6。", LevelUpStatType.Attack, 3, 6f),
            Stat("lv_element", "元素亲和", "元素攻击力 +4。", LevelUpStatType.ElementalAttack, 2, 4f),
            Stat("lv_health", "生命强化", "最大生命 +15，并立即恢复。", LevelUpStatType.MaxHealth, 1, 15f),
            Stat("lv_health2", "坚韧体魄", "最大生命 +30，并立即恢复。", LevelUpStatType.MaxHealth, 4, 30f),
            Stat("lv_speed", "迅捷", "移动速度 +0.5。", LevelUpStatType.MoveSpeed, 1, 0.5f),
            Stat("lv_crit_rate", "精准", "暴击率 +3%。", LevelUpStatType.CritRate, 2, 3f),
            Stat("lv_crit_effect", "致命", "暴击效果 +15%。", LevelUpStatType.CritEffect, 3, 15f),
            Stat("lv_defense", "铁壁", "防御 +5。", LevelUpStatType.Defense, 2, 5f),
            Stat("lv_cdr", "冷却精通", "冷却缩减 +3%。", LevelUpStatType.CooldownReduction, 2, 3f),
            Stat("lv_exp", "学者", "经验获取 +10%。", LevelUpStatType.ExpBonus, 2, 10f),
            Stat("lv_gold", "贪婪", "金币获取 +10%。", LevelUpStatType.GoldBonus, 2, 10f),
            Stat("lv_ad_bonus", "攻势", "攻击增伤 +5%。", LevelUpStatType.AttackDamageBonus, 3, 5f),
            Stat("lv_elem_bonus", "元素增幅", "元素攻击增伤 +8%。", LevelUpStatType.ElementalDamageBonus, 4, 8f)
        };
    }

    static GameItemEntry Entry(
        string id,
        string name,
        string desc,
        GameItemKind kind,
        int price,
        int tier,
        ShopWeaponType weapon)
    {
        return new GameItemEntry
        {
            id = id,
            displayName = name,
            description = desc,
            kind = kind,
            price = price,
            tier = tier,
            weapon = weapon,
            effectValue = 1f,
            purchaseOnce = kind == GameItemKind.ShopWeapon
        };
    }

    static GameItemEntry Minor(
        string id,
        string name,
        string desc,
        ShopWeaponType weapon,
        GameItemKind kind,
        int price,
        int tier,
        float value)
    {
        return new GameItemEntry
        {
            id = id,
            displayName = name,
            description = desc,
            kind = kind,
            price = price,
            tier = tier,
            weapon = weapon,
            effectValue = value,
            purchaseOnce = false
        };
    }

    static GameItemEntry Stat(
        string id,
        string name,
        string desc,
        LevelUpStatType statType,
        int tier,
        float value)
    {
        return new GameItemEntry
        {
            id = id,
            displayName = name,
            description = desc,
            kind = GameItemKind.LevelUpStat,
            statType = statType,
            price = 0,
            tier = tier,
            effectValue = value,
            purchaseOnce = false
        };
    }
}

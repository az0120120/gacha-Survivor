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
    LevelUpStat,
    [InspectorName("商店 - 武器发射数量")]
    ShopWeaponProjectileCountUp,
    [InspectorName("商店 - 武器破防强化")]
    ShopWeaponArmorPenUp,
    [InspectorName("商店 - 武器元素攻击强化")]
    ShopWeaponElementalAttackUp
}

public enum LevelUpStatType
{
    [InspectorName("攻击力")]
    Attack,
    [InspectorName("元素攻击")]
    ElementalAttack,
    [InspectorName("破防")]
    ArmorPenetration,
    [InspectorName("最大生命")]
    MaxHealth,
    [InspectorName("移动速度(固定值)")]
    MoveSpeed,
    [InspectorName("移动速度(%)")]
    MoveSpeedPercent,
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
    [Tooltip("唯一 ID，建议英文，如 weapon_desert_eagle")]
    public string id = "new_item";

    public string displayName = "新道具";

    [TextArea(2, 4)]
    public string description = "道具说明";

    public Sprite icon;

    public GameItemKind kind = GameItemKind.ShopWeapon;

    [Tooltip("仅「升级 - 属性强化」需要选择")]
    public LevelUpStatType statType = LevelUpStatType.Attack;

    [Tooltip("商店价格（基础价，35-50）；升级道具可填 0")]
    public int price = 40;

    [Tooltip("品级，影响商店刷新权重")]
    public int tier = 1;

    [Tooltip("关联武器（武器/武器强化时填写）")]
    public ShopWeaponType weapon = ShopWeaponType.DesertEagle;

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

            case GameItemKind.ShopWeaponProjectileCountUp:
                pool = ItemPoolType.Shop;
                category = ShopItemCategory.WeaponMinorUpgrade;
                effect = ItemEffectType.WeaponProjectileCountUp;
                shopSize = ShopSizeType.Small;
                once = false;
                return;

            case GameItemKind.ShopWeaponArmorPenUp:
                pool = ItemPoolType.Shop;
                category = ShopItemCategory.WeaponMinorUpgrade;
                effect = ItemEffectType.WeaponArmorPenUp;
                shopSize = ShopSizeType.Small;
                once = false;
                return;

            case GameItemKind.ShopWeaponElementalAttackUp:
                pool = ItemPoolType.Shop;
                category = ShopItemCategory.WeaponMinorUpgrade;
                effect = ItemEffectType.WeaponElementalAttackUp;
                shopSize = ShopSizeType.Small;
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
            case LevelUpStatType.ArmorPenetration: return ItemEffectType.ArmorPenetrationUp;
            case LevelUpStatType.MaxHealth: return ItemEffectType.MaxHealthUp;
            case LevelUpStatType.MoveSpeed: return ItemEffectType.MoveSpeedUp;
            case LevelUpStatType.MoveSpeedPercent: return ItemEffectType.MoveSpeedPercentUp;
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
            Entry("weapon_desert_eagle", "火沙鹰", "每 0.75 秒发射子弹，6 倍伤害，穿透 3。", GameItemKind.ShopWeapon, 48, 2, ShopWeaponType.DesertEagle),
            Entry("weapon_molotov", "燃烧瓶", "每 3 秒投掷，落点形成持续 5 秒的区域伤害。", GameItemKind.ShopWeapon, 50, 2, ShopWeaponType.Molotov),
            Entry("weapon_kunai", "苦无", "每秒锁定单体造成 6 倍瞬发伤害。", GameItemKind.ShopWeapon, 46, 2, ShopWeaponType.Kunai),
            Entry("weapon_claw", "虾钳", "扇形近战，每秒 4 倍伤害。", GameItemKind.ShopWeapon, 44, 1, ShopWeaponType.Claw),
            Entry("weapon_ak", "AK", "每 0.25 秒发射子弹，7 倍伤害，穿透 3。", GameItemKind.ShopWeapon, 52, 3, ShopWeaponType.Ak),

            Minor("w_range_desert_eagle", "火沙鹰延伸", "火沙鹰攻击范围 +20%。", ShopWeaponType.DesertEagle, GameItemKind.ShopWeaponRangeUp, 38, 2, 20f),
            Minor("w_cd_desert_eagle", "火沙鹰速射", "火沙鹰冷却缩减 +10%。", ShopWeaponType.DesertEagle, GameItemKind.ShopWeaponCooldownUp, 40, 2, 10f),
            Minor("w_range_molotov", "燃烧延伸", "燃烧瓶投掷范围 +20%。", ShopWeaponType.Molotov, GameItemKind.ShopWeaponRangeUp, 40, 2, 20f),
            Minor("w_cd_molotov", "燃烧加速", "燃烧瓶冷却缩减 +10%。", ShopWeaponType.Molotov, GameItemKind.ShopWeaponCooldownUp, 42, 2, 10f),
            Minor("w_range_kunai", "苦无延伸", "苦无索敌范围 +20%。", ShopWeaponType.Kunai, GameItemKind.ShopWeaponRangeUp, 38, 2, 20f),
            Minor("w_cd_kunai", "苦无加速", "苦无冷却缩减 +10%。", ShopWeaponType.Kunai, GameItemKind.ShopWeaponCooldownUp, 40, 2, 10f),
            Minor("w_range_claw", "虾钳延伸", "虾钳攻击范围 +20%。", ShopWeaponType.Claw, GameItemKind.ShopWeaponRangeUp, 35, 1, 20f),
            Minor("w_cd_claw", "虾钳加速", "虾钳冷却缩减 +10%。", ShopWeaponType.Claw, GameItemKind.ShopWeaponCooldownUp, 37, 1, 10f),
            Minor("w_range_ak", "AK 延伸", "AK 攻击范围 +20%。", ShopWeaponType.Ak, GameItemKind.ShopWeaponRangeUp, 42, 3, 20f),
            Minor("w_cd_ak", "AK 速射", "AK 冷却缩减 +10%。", ShopWeaponType.Ak, GameItemKind.ShopWeaponCooldownUp, 44, 3, 10f),

            Minor("w_proj_desert_eagle", "火沙鹰齐射", "火沙鹰发射数量 +1。", ShopWeaponType.DesertEagle, GameItemKind.ShopWeaponProjectileCountUp, 250, 2, 1f),
            Minor("w_ap_desert_eagle", "火沙鹰破甲", "火沙鹰破防 +30%。", ShopWeaponType.DesertEagle, GameItemKind.ShopWeaponArmorPenUp, 35, 2, 30f),
            Minor("w_elem_desert_eagle", "火沙鹰元素", "火沙鹰元素攻击力 +30%。", ShopWeaponType.DesertEagle, GameItemKind.ShopWeaponElementalAttackUp, 35, 2, 30f),
            Minor("w_proj_molotov", "燃烧齐射", "燃烧瓶发射数量 +1。", ShopWeaponType.Molotov, GameItemKind.ShopWeaponProjectileCountUp, 250, 2, 1f),
            Minor("w_ap_molotov", "燃烧破甲", "燃烧瓶破防 +30%。", ShopWeaponType.Molotov, GameItemKind.ShopWeaponArmorPenUp, 35, 2, 30f),
            Minor("w_elem_molotov", "燃烧元素", "燃烧瓶元素攻击力 +30%。", ShopWeaponType.Molotov, GameItemKind.ShopWeaponElementalAttackUp, 35, 2, 30f),
            Minor("w_proj_kunai", "苦无齐射", "苦无发射数量 +1。", ShopWeaponType.Kunai, GameItemKind.ShopWeaponProjectileCountUp, 250, 2, 1f),
            Minor("w_ap_kunai", "苦无破甲", "苦无破防 +30%。", ShopWeaponType.Kunai, GameItemKind.ShopWeaponArmorPenUp, 35, 2, 30f),
            Minor("w_elem_kunai", "苦无元素", "苦无元素攻击力 +30%。", ShopWeaponType.Kunai, GameItemKind.ShopWeaponElementalAttackUp, 35, 2, 30f),
            Minor("w_proj_claw", "虾钳齐射", "虾钳发射数量 +1。", ShopWeaponType.Claw, GameItemKind.ShopWeaponProjectileCountUp, 250, 1, 1f),
            Minor("w_ap_claw", "虾钳破甲", "虾钳破防 +30%。", ShopWeaponType.Claw, GameItemKind.ShopWeaponArmorPenUp, 35, 1, 30f),
            Minor("w_elem_claw", "虾钳元素", "虾钳元素攻击力 +30%。", ShopWeaponType.Claw, GameItemKind.ShopWeaponElementalAttackUp, 35, 1, 30f),
            Minor("w_proj_ak", "AK 齐射", "AK 发射数量 +1。", ShopWeaponType.Ak, GameItemKind.ShopWeaponProjectileCountUp, 250, 3, 1f),
            Minor("w_ap_ak", "AK 破甲", "AK 破防 +30%。", ShopWeaponType.Ak, GameItemKind.ShopWeaponArmorPenUp, 35, 3, 30f),
            Minor("w_elem_ak", "AK 元素", "AK 元素攻击力 +30%。", ShopWeaponType.Ak, GameItemKind.ShopWeaponElementalAttackUp, 35, 3, 30f),

            Stat("lv_attack", "强攻", "攻击力 +15（所有武器）。", LevelUpStatType.Attack, 1, 15f),
            Stat("lv_element", "元素之力", "元素攻击力 +5（所有武器）。", LevelUpStatType.ElementalAttack, 1, 5f),
            Stat("lv_armor_pen", "破甲", "破防 +25（所有武器）。", LevelUpStatType.ArmorPenetration, 2, 25f),
            Stat("lv_defense", "护甲", "防御 +1。", LevelUpStatType.Defense, 1, 1f),
            Stat("lv_health", "生命", "最大生命 +15，并立即恢复。", LevelUpStatType.MaxHealth, 1, 15f),
            Stat("lv_crit_rate", "精准", "暴击率 +3%（所有武器）。", LevelUpStatType.CritRate, 2, 3f),
            Stat("lv_crit_effect", "致命", "暴击效果 +10%（所有武器）。", LevelUpStatType.CritEffect, 2, 10f),
            Stat("lv_speed_pct", "迅捷", "移动速度 +3%。", LevelUpStatType.MoveSpeedPercent, 1, 3f)
        };
    }

    public static List<GameItemEntry> CreateDefaultLevelUpEntries()
    {
        return new List<GameItemEntry>
        {
            Stat("lv_attack", "强攻", "攻击力 +15（所有武器）。", LevelUpStatType.Attack, 1, 15f),
            Stat("lv_element", "元素之力", "元素攻击力 +5（所有武器）。", LevelUpStatType.ElementalAttack, 1, 5f),
            Stat("lv_armor_pen", "破甲", "破防 +25（所有武器）。", LevelUpStatType.ArmorPenetration, 2, 25f),
            Stat("lv_defense", "护甲", "防御 +1。", LevelUpStatType.Defense, 1, 1f),
            Stat("lv_health", "生命", "最大生命 +15，并立即恢复。", LevelUpStatType.MaxHealth, 1, 15f),
            Stat("lv_crit_rate", "精准", "暴击率 +3%（所有武器）。", LevelUpStatType.CritRate, 2, 3f),
            Stat("lv_crit_effect", "致命", "暴击效果 +10%（所有武器）。", LevelUpStatType.CritEffect, 2, 10f),
            Stat("lv_speed_pct", "迅捷", "移动速度 +3%。", LevelUpStatType.MoveSpeedPercent, 1, 3f)
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

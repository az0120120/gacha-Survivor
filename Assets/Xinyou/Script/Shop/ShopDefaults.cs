using UnityEngine;

public static class ShopDefaults
{
    public static ShopItemDefinition[] CreateRuntimeCatalog()
    {
        return new[]
        {
            CreateWeapon("weapon_desert_eagle", "火沙鹰", "每 0.75 秒发射子弹，6 倍伤害，穿透 3。", 48, ShopWeaponType.DesertEagle, 2),
            CreateWeapon("weapon_molotov", "燃烧瓶", "每 3 秒投掷，落点形成持续 5 秒的区域伤害。", 50, ShopWeaponType.Molotov, 2),
            CreateWeapon("weapon_kunai", "苦无", "每秒锁定单体造成 6 倍瞬发伤害。", 46, ShopWeaponType.Kunai, 2),
            CreateWeapon("weapon_claw", "虾钳", "扇形近战，每秒 4 倍伤害。", 44, ShopWeaponType.Claw, 1),
            CreateWeapon("weapon_ak", "AK", "每 0.25 秒发射子弹，7 倍伤害，穿透 3。", 52, ShopWeaponType.Ak, 3),

            CreateMinor("w_range_desert_eagle", "火沙鹰延伸", "火沙鹰攻击范围 +20%。", 38, ShopWeaponType.DesertEagle, ItemEffectType.WeaponRangeUp, 20f, 2),
            CreateMinor("w_cd_desert_eagle", "火沙鹰速射", "火沙鹰冷却缩减 +10%。", 40, ShopWeaponType.DesertEagle, ItemEffectType.WeaponCooldownUp, 10f, 2),
            CreateMinor("w_range_molotov", "燃烧延伸", "燃烧瓶投掷范围 +20%。", 40, ShopWeaponType.Molotov, ItemEffectType.WeaponRangeUp, 20f, 2),
            CreateMinor("w_cd_molotov", "燃烧加速", "燃烧瓶冷却缩减 +10%。", 42, ShopWeaponType.Molotov, ItemEffectType.WeaponCooldownUp, 10f, 2),
            CreateMinor("w_range_kunai", "苦无延伸", "苦无索敌范围 +20%。", 38, ShopWeaponType.Kunai, ItemEffectType.WeaponRangeUp, 20f, 2),
            CreateMinor("w_cd_kunai", "苦无加速", "苦无冷却缩减 +10%。", 40, ShopWeaponType.Kunai, ItemEffectType.WeaponCooldownUp, 10f, 2),
            CreateMinor("w_range_claw", "虾钳延伸", "虾钳攻击范围 +20%。", 35, ShopWeaponType.Claw, ItemEffectType.WeaponRangeUp, 20f, 1),
            CreateMinor("w_cd_claw", "虾钳加速", "虾钳冷却缩减 +10%。", 37, ShopWeaponType.Claw, ItemEffectType.WeaponCooldownUp, 10f, 1),
            CreateMinor("w_range_ak", "AK 延伸", "AK 攻击范围 +20%。", 42, ShopWeaponType.Ak, ItemEffectType.WeaponRangeUp, 20f, 3),
            CreateMinor("w_cd_ak", "AK 速射", "AK 冷却缩减 +10%。", 44, ShopWeaponType.Ak, ItemEffectType.WeaponCooldownUp, 10f, 3)
        };
    }

    static ShopItemDefinition CreateWeapon(string id, string name, string desc, int price, ShopWeaponType weapon, int tier)
    {
        var item = ScriptableObject.CreateInstance<ShopItemDefinition>();
        item.ConfigureRuntime(id, name, desc, ItemPoolType.Shop, ShopItemCategory.Weapon, ItemEffectType.EquipWeapon, tier, price, ShopSizeType.Large, true, weapon);
        return item;
    }

    static ShopItemDefinition CreateMinor(
        string id,
        string name,
        string desc,
        int price,
        ShopWeaponType weapon,
        ItemEffectType effectType,
        float value,
        int tier)
    {
        var item = ScriptableObject.CreateInstance<ShopItemDefinition>();
        item.ConfigureRuntime(id, name, desc, ItemPoolType.Shop, ShopItemCategory.WeaponMinorUpgrade, effectType, tier, price, ShopSizeType.Small, false, weapon, value);
        return item;
    }
}

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
            CreateMinor("w_cd_ak", "AK 速射", "AK 冷却缩减 +10%。", 44, ShopWeaponType.Ak, ItemEffectType.WeaponCooldownUp, 10f, 3),

            CreateMinor("w_proj_desert_eagle", "火沙鹰齐射", "火沙鹰发射数量 +1。", 250, ShopWeaponType.DesertEagle, ItemEffectType.WeaponProjectileCountUp, 1f, 2),
            CreateMinor("w_ap_desert_eagle", "火沙鹰破甲", "火沙鹰破防 +30%。", 35, ShopWeaponType.DesertEagle, ItemEffectType.WeaponArmorPenUp, 30f, 2),
            CreateMinor("w_elem_desert_eagle", "火沙鹰元素", "火沙鹰元素攻击力 +30%。", 35, ShopWeaponType.DesertEagle, ItemEffectType.WeaponElementalAttackUp, 30f, 2),
            CreateMinor("w_proj_molotov", "燃烧齐射", "燃烧瓶发射数量 +1。", 250, ShopWeaponType.Molotov, ItemEffectType.WeaponProjectileCountUp, 1f, 2),
            CreateMinor("w_ap_molotov", "燃烧破甲", "燃烧瓶破防 +30%。", 35, ShopWeaponType.Molotov, ItemEffectType.WeaponArmorPenUp, 30f, 2),
            CreateMinor("w_elem_molotov", "燃烧元素", "燃烧瓶元素攻击力 +30%。", 35, ShopWeaponType.Molotov, ItemEffectType.WeaponElementalAttackUp, 30f, 2),
            CreateMinor("w_proj_kunai", "苦无齐射", "苦无发射数量 +1。", 250, ShopWeaponType.Kunai, ItemEffectType.WeaponProjectileCountUp, 1f, 2),
            CreateMinor("w_ap_kunai", "苦无破甲", "苦无破防 +30%。", 35, ShopWeaponType.Kunai, ItemEffectType.WeaponArmorPenUp, 30f, 2),
            CreateMinor("w_elem_kunai", "苦无元素", "苦无元素攻击力 +30%。", 35, ShopWeaponType.Kunai, ItemEffectType.WeaponElementalAttackUp, 30f, 2),
            CreateMinor("w_proj_claw", "虾钳齐射", "虾钳发射数量 +1。", 250, ShopWeaponType.Claw, ItemEffectType.WeaponProjectileCountUp, 1f, 1),
            CreateMinor("w_ap_claw", "虾钳破甲", "虾钳破防 +30%。", 35, ShopWeaponType.Claw, ItemEffectType.WeaponArmorPenUp, 30f, 1),
            CreateMinor("w_elem_claw", "虾钳元素", "虾钳元素攻击力 +30%。", 35, ShopWeaponType.Claw, ItemEffectType.WeaponElementalAttackUp, 30f, 1),
            CreateMinor("w_proj_ak", "AK 齐射", "AK 发射数量 +1。", 250, ShopWeaponType.Ak, ItemEffectType.WeaponProjectileCountUp, 1f, 3),
            CreateMinor("w_ap_ak", "AK 破甲", "AK 破防 +30%。", 35, ShopWeaponType.Ak, ItemEffectType.WeaponArmorPenUp, 30f, 3),
            CreateMinor("w_elem_ak", "AK 元素", "AK 元素攻击力 +30%。", 35, ShopWeaponType.Ak, ItemEffectType.WeaponElementalAttackUp, 30f, 3)
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

using UnityEngine;

public static class ShopDefaults
{
    public static ShopItemDefinition[] CreateRuntimeCatalog()
    {
        return new[]
        {
            CreateWeapon("weapon_projectile", "魔弹杖", "装备后每 0.5 秒发射子弹，造成 4 倍伤害。", 50, ShopWeaponType.Projectile, 2),
            CreateWeapon("weapon_area", "旋转法阵", "装备后以自身为中心持续造成区域伤害。", 48, ShopWeaponType.Area, 3),
            CreateWeapon("weapon_direct", "虚空指", "装备后锁定最近敌人，每秒造成 6 倍瞬发伤害。", 45, ShopWeaponType.DirectTarget, 3),

            CreateMinor("w_range_projectile", "延伸弹匣", "魔弹杖攻击范围 +20%。", 35, ShopWeaponType.Projectile, ItemEffectType.WeaponRangeUp, 15f, 1),
            CreateMinor("w_cd_projectile", "速射核心", "魔弹杖冷却缩减 +10%。", 37, ShopWeaponType.Projectile, ItemEffectType.WeaponCooldownUp, 10f, 1),
            CreateMinor("w_range_area", "法阵扩张", "旋转法阵范围 +25%。", 40, ShopWeaponType.Area, ItemEffectType.WeaponRangeUp, 25f, 2),
            CreateMinor("w_cd_area", "法阵加速", "旋转法阵冷却缩减 +10%。", 42, ShopWeaponType.Area, ItemEffectType.WeaponCooldownUp, 10f, 2),
            CreateMinor("w_range_direct", "虚空延伸", "虚空指索敌范围 +20%。", 45, ShopWeaponType.DirectTarget, ItemEffectType.WeaponRangeUp, 20f, 2),
            CreateMinor("w_cd_direct", "虚空加速", "虚空指冷却缩减 +10%。", 47, ShopWeaponType.DirectTarget, ItemEffectType.WeaponCooldownUp, 10f, 2),

            CreateMajor("w_major_projectile", "三重散射", "魔弹杖改为三向散射攻击。", 48, ShopWeaponType.Projectile, 1),
            CreateMajor("w_major_area", "双环法阵", "旋转法阵频率提升，伤害范围扩大。", 50, ShopWeaponType.Area, 2),
            CreateMajor("w_major_direct", "连锁虚空", "虚空指改为范围连锁打击。", 50, ShopWeaponType.DirectTarget, 3)
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

    static ShopItemDefinition CreateMajor(string id, string name, string desc, int price, ShopWeaponType weapon, int tier)
    {
        var item = ScriptableObject.CreateInstance<ShopItemDefinition>();
        item.ConfigureRuntime(id, name, desc, ItemPoolType.Shop, ShopItemCategory.WeaponMajorUpgrade, ItemEffectType.WeaponMajorUpgrade, tier, price, ShopSizeType.Large, false, weapon, 1f);
        return item;
    }
}

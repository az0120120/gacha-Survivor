public static class ShopItemRules
{
    public static bool IsDualShopWeaponUpgrade(ItemEffectType effectType)
    {
        return effectType == ItemEffectType.WeaponProjectileCountUp
               || effectType == ItemEffectType.WeaponArmorPenUp
               || effectType == ItemEffectType.WeaponElementalAttackUp;
    }
}

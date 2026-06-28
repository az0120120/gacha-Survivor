using UnityEngine;

public static class ItemEffectApplier
{
    public static void Apply(
        ShopItemDefinition item,
        WeaponManager weaponManager,
        CharacterStats characterStats,
        PlayerHealth playerHealth)
    {
        if (item == null)
            return;

        switch (item.EffectType)
        {
            case ItemEffectType.EquipWeapon:
                weaponManager?.TryEquipShopWeapon(item.WeaponType);
                break;
            case ItemEffectType.WeaponRangeUp:
                weaponManager?.AddWeaponRangePercent(item.WeaponType, item.EffectValue);
                break;
            case ItemEffectType.WeaponCooldownUp:
                weaponManager?.AddWeaponCooldownReductionPercent(item.WeaponType, item.EffectValue);
                break;
            case ItemEffectType.WeaponMajorUpgrade:
                weaponManager?.ApplyMajorUpgrade(item.WeaponType, StatMath.FloorToInt(item.EffectValue));
                break;
            case ItemEffectType.AttackUp:
                characterStats?.AddAttack(StatMath.FloorToInt(item.EffectValue));
                break;
            case ItemEffectType.ElementalAttackUp:
                characterStats?.AddElementalAttack(StatMath.FloorToInt(item.EffectValue));
                break;
            case ItemEffectType.ArmorPenetrationUp:
                characterStats?.AddArmorPenetration(StatMath.FloorToInt(item.EffectValue));
                break;
            case ItemEffectType.MaxHealthUp:
                playerHealth?.AddMaxHealthBonus(StatMath.FloorToInt(item.EffectValue));
                break;
            case ItemEffectType.MoveSpeedUp:
                characterStats?.AddMoveSpeed(item.EffectValue);
                break;
            case ItemEffectType.MoveSpeedPercentUp:
                characterStats?.AddMoveSpeedPercent(item.EffectValue);
                break;
            case ItemEffectType.CritRateUp:
                characterStats?.AddCritRatePercent(item.EffectValue);
                break;
            case ItemEffectType.CritEffectUp:
                characterStats?.AddCritEffectPercent(item.EffectValue);
                break;
            case ItemEffectType.DefenseUp:
                characterStats?.AddDefense(StatMath.FloorToInt(item.EffectValue));
                break;
            case ItemEffectType.CooldownReductionUp:
                characterStats?.AddCooldownReductionPercent(item.EffectValue);
                break;
            case ItemEffectType.ExpBonusUp:
                characterStats?.AddExpBonusPercent(item.EffectValue);
                break;
            case ItemEffectType.GoldBonusUp:
                characterStats?.AddGoldBonusPercent(item.EffectValue);
                break;
            case ItemEffectType.AttackDamageBonusUp:
                characterStats?.AddAttackDamageBonusPercent(item.EffectValue);
                break;
            case ItemEffectType.ElementalDamageBonusUp:
                characterStats?.AddElementalDamageBonusPercent(item.EffectValue);
                break;
        }
    }
}

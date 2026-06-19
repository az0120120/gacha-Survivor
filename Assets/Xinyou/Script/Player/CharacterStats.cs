using UnityEngine;

[AddComponentMenu("GachaSurvivor/Character Stats")]
public class CharacterStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] int baseAttack = 10;
    [SerializeField] int baseElementalAttack;
    [SerializeField] int baseArmorPenetration;
    [SerializeField] float baseCritRatePercent = 5f;
    [SerializeField] float baseCritEffectPercent = 150f;
    [SerializeField] int baseMaxHealth = 100;
    [SerializeField] int baseDefense;
    [SerializeField] float baseDodgeRatePercent;
    [SerializeField] float baseGoldPickupRangePercent;
    [SerializeField] float baseMoveSpeed = 5f;
    [SerializeField] float baseCooldownReductionPercent;
    [SerializeField] float baseExpBonusPercent;
    [SerializeField] float baseGoldBonusPercent;
    [SerializeField] float baseAttackDamageBonusPercent;
    [SerializeField] float baseElementalDamageBonusPercent;
    [SerializeField] int baseBulletPenetration = 2;

    int bonusAttack;
    int bonusElementalAttack;
    int bonusArmorPenetration;
    float bonusCritRatePercent;
    float bonusCritEffectPercent;
    int bonusMaxHealth;
    int bonusDefense;
    float bonusDodgeRatePercent;
    float bonusGoldPickupRangePercent;
    float bonusMoveSpeed;
    float bonusCooldownReductionPercent;
    float bonusExpBonusPercent;
    float bonusGoldBonusPercent;
    float bonusAttackDamageBonusPercent;
    float bonusElementalDamageBonusPercent;
    int bonusBulletPenetration;

    public int Attack => StatMath.FloorToInt(baseAttack + bonusAttack);
    public int ElementalAttack => StatMath.FloorToInt(baseElementalAttack + bonusElementalAttack);
    public int ArmorPenetration => StatMath.FloorToInt(baseArmorPenetration + bonusArmorPenetration);
    public float CritRate => StatMath.ClampRatio(StatMath.PercentToRatio(baseCritRatePercent + bonusCritRatePercent));
    public float CritDamageMultiplier => StatMath.PercentToRatio(baseCritEffectPercent + bonusCritEffectPercent);
    public int MaxHealth => StatMath.FloorToInt(baseMaxHealth + bonusMaxHealth);
    public int Defense => StatMath.FloorToInt(baseDefense + bonusDefense);
    public float DodgeRate => StatMath.ClampRatio(StatMath.PercentToRatio(baseDodgeRatePercent + bonusDodgeRatePercent));
    public float GoldPickupRangeRatio => 1f + StatMath.PercentToRatio(baseGoldPickupRangePercent + bonusGoldPickupRangePercent);
    public float MoveSpeed => baseMoveSpeed + bonusMoveSpeed;
    public float CooldownReduction => StatMath.ClampRatio(StatMath.PercentToRatio(baseCooldownReductionPercent + bonusCooldownReductionPercent));
    public float ExpBonusRatio => 1f + StatMath.PercentToRatio(baseExpBonusPercent + bonusExpBonusPercent);
    public float GoldBonusRatio => 1f + StatMath.PercentToRatio(baseGoldBonusPercent + bonusGoldBonusPercent);
    public float AttackDamageBonus => StatMath.PercentToRatio(baseAttackDamageBonusPercent + bonusAttackDamageBonusPercent);
    public float ElementalDamageBonus => StatMath.PercentToRatio(baseElementalDamageBonusPercent + bonusElementalDamageBonusPercent);
    public int BulletPenetration => StatMath.FloorToInt(baseBulletPenetration + bonusBulletPenetration);

    public void AddAttack(int amount) => bonusAttack += amount;
    public void AddElementalAttack(int amount) => bonusElementalAttack += amount;
    public void AddArmorPenetration(int amount) => bonusArmorPenetration += amount;
    public void AddCritRatePercent(float amount) => bonusCritRatePercent += amount;
    public void AddCritEffectPercent(float amount) => bonusCritEffectPercent += amount;
    public void AddMaxHealth(int amount) => bonusMaxHealth += amount;
    public void AddDefense(int amount) => bonusDefense += amount;
    public void AddDodgeRatePercent(float amount) => bonusDodgeRatePercent += amount;
    public void AddGoldPickupRangePercent(float amount) => bonusGoldPickupRangePercent += amount;
    public void AddMoveSpeed(float amount) => bonusMoveSpeed += amount;
    public void AddCooldownReductionPercent(float amount) => bonusCooldownReductionPercent += amount;
    public void AddExpBonusPercent(float amount) => bonusExpBonusPercent += amount;
    public void AddGoldBonusPercent(float amount) => bonusGoldBonusPercent += amount;
    public void AddAttackDamageBonusPercent(float amount) => bonusAttackDamageBonusPercent += amount;
    public void AddElementalDamageBonusPercent(float amount) => bonusElementalDamageBonusPercent += amount;
    public void AddBulletPenetration(int amount) => bonusBulletPenetration += amount;

    public float GetEffectiveCooldown(float baseCooldown)
    {
        return baseCooldown * (1f - CooldownReduction);
    }
}

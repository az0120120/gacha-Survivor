using UnityEngine;

public struct DamageResult
{
    public int FinalDamage;
    public bool IsCritical;
}

public static class DamageCalculator
{
    public static DamageResult CalculateAgainstEnemy(
        CharacterStats attacker,
        EnemyStats defender,
        float attackMultiplierK,
        float specialMultiplier = 1f,
        bool useWeaponDamageContext = false,
        WeaponDamageContext weaponContext = default)
    {
        if (attacker == null || defender == null)
            return default;

        int armorPenetration = useWeaponDamageContext
            ? weaponContext.ArmorPenetration
            : attacker.ArmorPenetration;
        int elementalAttack = useWeaponDamageContext
            ? weaponContext.ElementalAttack
            : attacker.ElementalAttack;

        int gameMinutes = GetGameMinutes();
        float defenseReduction = CalculateDefenseReduction(defender.Defense, armorPenetration, gameMinutes);

        float attackPower = attackMultiplierK
                            * attacker.Attack
                            * (1f - defenseReduction)
                            * (1f + attacker.AttackDamageBonus);

        float elementalPower = attackMultiplierK
                               * elementalAttack
                               * (1f + attacker.ElementalDamageBonus);

        float totalDamage = attackPower + elementalPower;

        bool isCritical = Random.value < attacker.CritRate;
        if (isCritical)
            totalDamage *= attacker.CritDamageMultiplier;

        totalDamage *= specialMultiplier;
        totalDamage *= MapPropStatusEffects.OutgoingDamageMultiplier;

        return new DamageResult
        {
            FinalDamage = StatMath.FloorToInt(totalDamage),
            IsCritical = isCritical
        };
    }

    public static int CalculateAgainstPlayer(CharacterStats defender, int enemyAttack)
    {
        if (defender == null || enemyAttack <= 0)
            return 0;

        if (Random.value < defender.DodgeRate)
            return 0;

        int gameMinutes = GetGameMinutes();
        float defenseReduction = CalculateDefenseReduction(defender.Defense, 0, gameMinutes);
        float damage = enemyAttack * (1f - defenseReduction);
        return StatMath.FloorToInt(damage);
    }

    public static float CalculateDefenseReduction(int defense, int armorPenetration, int gameMinutes)
    {
        int remainingDefense = Mathf.Max(0, defense - armorPenetration);
        float minuteFactor = gameMinutes * 50f;
        float denominator = remainingDefense + minuteFactor;
        float extraReduction = denominator > 0f ? remainingDefense / denominator : 0f;
        float reduction = 0.1f + extraReduction;
        return StatMath.ClampRatio(reduction);
    }

    static int GetGameMinutes()
    {
        if (GameTimeManager.Instance == null)
            return 0;

        return GameTimeManager.Instance.GameMinutes;
    }
}

using UnityEngine;

public static class EnemyStatScaler
{
    public static int GetMaxHealth(EnemyArchetype archetype, int gameMinutes)
    {
        float baseHealth;
        switch (archetype)
        {
            case EnemyArchetype.MeleeRush:
                baseHealth = 80f;
                break;
            case EnemyArchetype.RangedShooter:
                baseHealth = 40f;
                break;
            case EnemyArchetype.Harasser:
                baseHealth = 55f;
                break;
            default:
                baseHealth = 80f;
                break;
        }

        return StatMath.FloorToInt(baseHealth * Mathf.Pow(1.18f, gameMinutes));
    }

    public static int GetDefense(int gameMinutes)
    {
        return StatMath.FloorToInt(50f * gameMinutes);
    }

    public static int GetAttack(int gameMinutes)
    {
        return StatMath.FloorToInt(3f + gameMinutes);
    }
}

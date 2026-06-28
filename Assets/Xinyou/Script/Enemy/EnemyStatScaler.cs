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

    public static int RollGoldDrop(int gameMinutes)
    {
        if (gameMinutes <= 5)
            return 5;

        if (gameMinutes <= 10)
            return Random.Range(5, 7);

        if (gameMinutes <= 15)
            return Random.Range(6, 8);

        return Random.Range(6, 8);
    }

    public static int GetExpDropFromGold(int goldDrop)
    {
        return goldDrop * 3;
    }
}

using UnityEngine;

public static class EnemyStatScaler
{
    public static int GetMaxHealth(EnemyArchetype archetype, int gameMinutes)
    {
        switch (archetype)
        {
            case EnemyArchetype.Melee:
                return StatMath.FloorToInt(80f * Mathf.Pow(1.18f, gameMinutes));
            case EnemyArchetype.Ranged:
                return StatMath.FloorToInt(40f * Mathf.Pow(1.18f, gameMinutes));
            case EnemyArchetype.Boss:
                return StatMath.FloorToInt(800f * Mathf.Pow(1.18f, gameMinutes));
            default:
                return StatMath.FloorToInt(80f * Mathf.Pow(1.18f, gameMinutes));
        }
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

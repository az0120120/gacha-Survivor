using UnityEngine;

public struct WeaponTarget
{
    public EnemyHealth Enemy;
    public MapDestructibleProp MapProp;

    public bool IsValid =>
        (Enemy != null && Enemy.IsAlive) || (MapProp != null && MapProp.IsAlive);

    public Vector2 Position
    {
        get
        {
            if (Enemy != null)
                return Enemy.transform.position;

            if (MapProp != null)
                return MapProp.WorldPosition;

            return Vector2.zero;
        }
    }
}

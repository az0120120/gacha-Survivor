using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] EnemyArchetype archetype = EnemyArchetype.Melee;

    int maxHealth;
    int attack;
    int defense;

    public EnemyArchetype Archetype => archetype;
    public int MaxHealth => maxHealth;
    public int Attack => attack;
    public int Defense => defense;

    public void RefreshFromGameTime()
    {
        int gameMinutes = GameTimeManager.Instance != null ? GameTimeManager.Instance.GameMinutes : 0;
        maxHealth = EnemyStatScaler.GetMaxHealth(archetype, gameMinutes);
        attack = EnemyStatScaler.GetAttack(gameMinutes);
        defense = EnemyStatScaler.GetDefense(gameMinutes);
    }
}

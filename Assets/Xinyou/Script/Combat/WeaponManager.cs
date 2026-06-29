using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgradeData
{
    public float RangeBonusPercent;
    public float CooldownReductionPercent;
    public float DamageBonusPercent;
    public int MajorUpgradeLevel;
    public int ExtraProjectileCount;
    public float ArmorPenetrationBonusPercent;
    public float ElementalAttackBonusPercent;
}

[AddComponentMenu("GachaSurvivor/Weapon Manager")]
public class WeaponManager : MonoBehaviour
{
    [SerializeField] CharacterStats characterStats;
    [SerializeField] ObjectPool desertEagleProjectilePool;
    [SerializeField] ObjectPool akProjectilePool;
    [SerializeField] ShopWeaponType[] startingWeapons;

    readonly HashSet<ShopWeaponType> equippedWeapons = new HashSet<ShopWeaponType>();
    readonly Dictionary<ShopWeaponType, WeaponUpgradeData> upgradeData = new Dictionary<ShopWeaponType, WeaponUpgradeData>();

    void Awake()
    {
        if (characterStats == null)
            characterStats = GetComponent<CharacterStats>();

        DisableAllWeapons();

        if (startingWeapons != null)
        {
            for (int i = 0; i < startingWeapons.Length; i++)
                EquipWeapon(startingWeapons[i]);
        }
    }

    public bool HasWeapon(ShopWeaponType weaponType)
    {
        return equippedWeapons.Contains(weaponType);
    }

    public bool EquipWeapon(ShopWeaponType weaponType)
    {
        if (equippedWeapons.Contains(weaponType))
            return false;

        var weapon = GetOrAddWeapon(weaponType);
        if (weapon == null)
            return false;

        weapon.enabled = true;
        weapon.Initialize(characterStats, this);
        equippedWeapons.Add(weaponType);
        return true;
    }

    public bool TryEquipShopWeapon(ShopWeaponType weaponType)
    {
        return EquipWeapon(weaponType);
    }

    public float GetRangeMultiplier(ShopWeaponType weaponType)
    {
        var data = GetUpgradeData(weaponType);
        return 1f + data.RangeBonusPercent * 0.01f;
    }

    public float GetWeaponCooldownMultiplier(ShopWeaponType weaponType)
    {
        var data = GetUpgradeData(weaponType);
        float reduction = Mathf.Clamp01(data.CooldownReductionPercent * 0.01f);
        return 1f - reduction;
    }

    public float GetMajorUpgradeDamageMultiplier(ShopWeaponType weaponType)
    {
        var data = GetUpgradeData(weaponType);
        float majorBonus = data.MajorUpgradeLevel * 15f;
        return 1f + (data.DamageBonusPercent + majorBonus) * 0.01f;
    }

    public int GetMajorUpgradeLevel(ShopWeaponType weaponType)
    {
        return GetUpgradeData(weaponType).MajorUpgradeLevel;
    }

    public void AddWeaponRangePercent(ShopWeaponType weaponType, float percent)
    {
        GetUpgradeData(weaponType).RangeBonusPercent += percent;
    }

    public void AddWeaponCooldownReductionPercent(ShopWeaponType weaponType, float percent)
    {
        GetUpgradeData(weaponType).CooldownReductionPercent += percent;
    }

    public void AddWeaponDamagePercent(ShopWeaponType weaponType, float percent)
    {
        GetUpgradeData(weaponType).DamageBonusPercent += percent;
    }

    public void ApplyMajorUpgrade(ShopWeaponType weaponType, int levelDelta)
    {
        var data = GetUpgradeData(weaponType);
        data.MajorUpgradeLevel += levelDelta;

        if (!HasWeapon(weaponType))
            EquipWeapon(weaponType);
    }

    public int GetProjectileCount(ShopWeaponType weaponType)
    {
        return 1 + GetUpgradeData(weaponType).ExtraProjectileCount;
    }

    public void AddWeaponProjectileCount(ShopWeaponType weaponType, int amount = 1)
    {
        GetUpgradeData(weaponType).ExtraProjectileCount += amount;
    }

    public void AddWeaponArmorPenetrationPercent(ShopWeaponType weaponType, float percent)
    {
        GetUpgradeData(weaponType).ArmorPenetrationBonusPercent += percent;
    }

    public void AddWeaponElementalAttackPercent(ShopWeaponType weaponType, float percent)
    {
        GetUpgradeData(weaponType).ElementalAttackBonusPercent += percent;
    }

    public WeaponDamageContext GetWeaponDamageContext(CharacterStats sourceStats, ShopWeaponType weaponType)
    {
        if (sourceStats == null)
            return default;

        var data = GetUpgradeData(weaponType);
        float armorPenMultiplier = 1f + data.ArmorPenetrationBonusPercent * 0.01f;
        float elementalMultiplier = 1f + data.ElementalAttackBonusPercent * 0.01f;

        return new WeaponDamageContext
        {
            ArmorPenetration = StatMath.FloorToInt(sourceStats.ArmorPenetration * armorPenMultiplier),
            ElementalAttack = StatMath.FloorToInt(sourceStats.ElementalAttack * elementalMultiplier)
        };
    }

    public WeaponUpgradeData GetWeaponUpgradeData(ShopWeaponType weaponType)
    {
        return GetUpgradeData(weaponType);
    }

    WeaponUpgradeData GetUpgradeData(ShopWeaponType weaponType)
    {
        if (!upgradeData.TryGetValue(weaponType, out var data))
        {
            data = new WeaponUpgradeData();
            upgradeData[weaponType] = data;
        }

        return data;
    }

    WeaponBase GetOrAddWeapon(ShopWeaponType weaponType)
    {
        switch (weaponType)
        {
            case ShopWeaponType.DesertEagle:
                return AssignProjectilePool(GetOrAddComponent<DesertEagleWeapon>(), desertEagleProjectilePool);
            case ShopWeaponType.Ak:
                return AssignProjectilePool(GetOrAddComponent<AkWeapon>(), akProjectilePool);
            case ShopWeaponType.Molotov:
                return GetOrAddComponent<MolotovWeapon>();
            case ShopWeaponType.Kunai:
                return GetOrAddComponent<KunaiWeapon>();
            case ShopWeaponType.Claw:
                return GetOrAddComponent<ClawWeapon>();
            default:
                return null;
        }
    }

    T GetOrAddComponent<T>() where T : Component
    {
        var component = GetComponent<T>();
        return component != null ? component : gameObject.AddComponent<T>();
    }

    WeaponBase AssignProjectilePool(WeaponBase weapon, ObjectPool dedicatedPool)
    {
        if (weapon is StatProjectileWeapon statProjectile)
            statProjectile.AssignProjectilePoolIfEmpty(dedicatedPool);

        return weapon;
    }

    void DisableAllWeapons()
    {
        var weapons = GetComponents<WeaponBase>();
        for (int i = 0; i < weapons.Length; i++)
            weapons[i].enabled = false;

        equippedWeapons.Clear();
    }
}

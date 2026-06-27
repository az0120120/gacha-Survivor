using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgradeData
{
    public float RangeBonusPercent;
    public float CooldownReductionPercent;
    public int MajorUpgradeLevel;
}

[AddComponentMenu("GachaSurvivor/Weapon Manager")]
public class WeaponManager : MonoBehaviour
{
    [SerializeField] CharacterStats characterStats;
    [Header("Legacy Projectile Pool")]
    [SerializeField] ObjectPool projectilePool;

    [Header("Gun Projectile Pools")]
    [SerializeField] ObjectPool desertEagleProjectilePool;
    [SerializeField] ObjectPool akProjectilePool;
    [SerializeField] ShopWeaponType[] startingWeapons = { ShopWeaponType.Projectile };

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
        float multiplier = 1f + data.RangeBonusPercent * 0.01f;

        if (data.MajorUpgradeLevel > 0 && weaponType == ShopWeaponType.Area)
            multiplier *= 1.3f;

        return multiplier;
    }

    public float GetWeaponCooldownMultiplier(ShopWeaponType weaponType)
    {
        var data = GetUpgradeData(weaponType);
        float reduction = Mathf.Clamp01(data.CooldownReductionPercent * 0.01f);

        if (data.MajorUpgradeLevel > 0 && weaponType == ShopWeaponType.Area)
            reduction = Mathf.Clamp01(reduction + 0.15f);

        return 1f - reduction;
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

    public void ApplyMajorUpgrade(ShopWeaponType weaponType, int levelDelta)
    {
        var data = GetUpgradeData(weaponType);
        data.MajorUpgradeLevel += levelDelta;

        if (!HasWeapon(weaponType))
            EquipWeapon(weaponType);
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
            case ShopWeaponType.Projectile:
                return ConfigureProjectilePool(GetOrAddComponent<ProjectileWeapon>());
            case ShopWeaponType.Area:
                return GetOrAddComponent<AreaWeapon>();
            case ShopWeaponType.DirectTarget:
                return GetOrAddComponent<DirectTargetWeapon>();
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
        if (weapon == null)
            return null;

        if (weapon is ProjectileWeapon legacyProjectile)
        {
            if (projectilePool != null)
                legacyProjectile.SetProjectilePool(projectilePool);
            return weapon;
        }

        if (weapon is StatProjectileWeapon statProjectile)
            statProjectile.AssignProjectilePoolIfEmpty(dedicatedPool);

        return weapon;
    }

    WeaponBase ConfigureProjectilePool(WeaponBase weapon)
    {
        if (projectilePool == null || weapon == null)
            return weapon;

        if (weapon is ProjectileWeapon legacyProjectile)
            legacyProjectile.SetProjectilePool(projectilePool);
        else if (weapon is StatProjectileWeapon statProjectile)
            statProjectile.AssignProjectilePoolIfEmpty(projectilePool);

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

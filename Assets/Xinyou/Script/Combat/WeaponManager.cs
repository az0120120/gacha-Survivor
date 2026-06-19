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
    [SerializeField] ObjectPool projectilePool;
    [SerializeField] ShopWeaponType[] startingWeapons = { ShopWeaponType.Projectile };

    readonly HashSet<ShopWeaponType> equippedWeapons = new HashSet<ShopWeaponType>();
    readonly Dictionary<ShopWeaponType, WeaponUpgradeData> upgradeData = new Dictionary<ShopWeaponType, WeaponUpgradeData>();

    void Awake()
    {
        if (characterStats == null)
            characterStats = GetComponent<CharacterStats>();

        upgradeData[ShopWeaponType.Projectile] = new WeaponUpgradeData();
        upgradeData[ShopWeaponType.Area] = new WeaponUpgradeData();
        upgradeData[ShopWeaponType.DirectTarget] = new WeaponUpgradeData();

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
            {
                var weapon = GetComponent<ProjectileWeapon>();
                if (weapon == null)
                    weapon = gameObject.AddComponent<ProjectileWeapon>();

                if (projectilePool != null)
                    weapon.SetProjectilePool(projectilePool);

                return weapon;
            }
            case ShopWeaponType.Area:
            {
                var weapon = GetComponent<AreaWeapon>();
                return weapon != null ? weapon : gameObject.AddComponent<AreaWeapon>();
            }
            case ShopWeaponType.DirectTarget:
            {
                var weapon = GetComponent<DirectTargetWeapon>();
                return weapon != null ? weapon : gameObject.AddComponent<DirectTargetWeapon>();
            }
            default:
                return null;
        }
    }

    void DisableAllWeapons()
    {
        SetWeaponEnabled(GetComponent<ProjectileWeapon>(), false);
        SetWeaponEnabled(GetComponent<AreaWeapon>(), false);
        SetWeaponEnabled(GetComponent<DirectTargetWeapon>(), false);
        equippedWeapons.Clear();
    }

    static void SetWeaponEnabled(WeaponBase weapon, bool enabled)
    {
        if (weapon != null)
            weapon.enabled = enabled;
    }
}

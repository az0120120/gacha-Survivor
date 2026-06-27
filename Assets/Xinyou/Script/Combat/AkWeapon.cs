using UnityEngine;

[AddComponentMenu("GachaSurvivor/AK Weapon")]
public class AkWeapon : StatProjectileWeapon
{
    protected override ShopWeaponType WeaponIdentity => ShopWeaponType.Ak;

    protected override void ConfigureDefaults()
    {
        damageMultiplier = 7f;
        fireInterval = 0.25f;
        targetRange = 8f;
        pierceCount = 3;
    }

    protected override int GetDefaultPrewarmCount() => 40;
}

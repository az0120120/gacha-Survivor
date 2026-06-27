using UnityEngine;

[AddComponentMenu("GachaSurvivor/Desert Eagle Weapon")]
public class DesertEagleWeapon : StatProjectileWeapon
{
    protected override ShopWeaponType WeaponIdentity => ShopWeaponType.DesertEagle;

    protected override void ConfigureDefaults()
    {
        damageMultiplier = 6f;
        fireInterval = 0.75f;
        targetRange = 8f;
        pierceCount = 3;
    }

    protected override int GetDefaultPrewarmCount() => 16;
}

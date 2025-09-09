// WeaponFireRateUpgrade.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Fire Rate")]
public class WeaponFireRateUpgrade : UpgradeBase
{
    public float addFireRate = 0.5f; // +shots/sec
    public override void Apply(GameObject player)
    {
        var w = player.GetComponent<AutoFireWeapon>();
        if (w) w.fireRate += addFireRate;
    }
}

// DamageUpgrade.cs
[CreateAssetMenu(menuName = "Upgrades/Damage")]
public class DamageUpgrade : UpgradeBase
{
    public float addDamage = 2f;
    public override void Apply(GameObject player)
    {
        var w = player.GetComponent<AutoFireWeapon>();
        if (w) w.damage += addDamage;
    }
}

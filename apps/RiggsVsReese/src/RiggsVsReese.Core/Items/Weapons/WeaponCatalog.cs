namespace RiggsVsReese.Core.Items.Weapons;

public static class WeaponCatalog
{
    public static IReadOnlyList <WeaponDefinition> All {get;}=
    [
        new()
        {
            Id="beretta-92f",
            Name="Beretta 92F",
            Type=WeaponType.Pistol,
            BaseDamage=25,
            FireRate=4.0f,
            Range=40
        }
    ];
}
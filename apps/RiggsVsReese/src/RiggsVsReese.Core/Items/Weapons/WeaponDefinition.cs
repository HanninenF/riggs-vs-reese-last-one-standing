namespace RiggsVsReese.Core.Items.Weapons;

public class WeaponDefinition:ItemDefinition
{
    public required WeaponType Type {get;init;}
    public required int BaseDamage {get;init;}
    public required float FireRate {get;init;}
    public required float Range {get;init;}

}
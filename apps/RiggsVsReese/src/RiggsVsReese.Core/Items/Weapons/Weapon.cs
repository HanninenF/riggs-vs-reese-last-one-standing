namespace RiggsVsReese.Core.Items.Weapons;

public class Weapon
{
    public WeaponDefinition Definition {get;}
    public Magazine Magazine{get; private set;}
    public bool HasRoundInChamber {get; private set;}
    
    public int Condition {get; private set;}=100;

    public Weapon(WeaponDefinition definition,Magazine magazine)
    {
        Definition=definition;
        Magazine= magazine;
        HasRoundInChamber=false;
    }

}
namespace RiggsVsReese.Core.Items.Weapons;

public class Magazine
{
    public MagazineDefinition Definition{get;}
    public int Rounds {get; private set;}

    public Magazine(MagazineDefinition definition)
    {
        Definition=definition;
        Rounds=definition.Capacity;
    }
}
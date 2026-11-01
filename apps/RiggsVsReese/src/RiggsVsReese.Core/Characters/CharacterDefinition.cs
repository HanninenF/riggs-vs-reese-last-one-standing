namespace RiggsVsReese.Core.Characters;

public abstract class CharacterDefinition
{
    public required string Id {get;init;}
    public required string Name {get;
    init;}

    public required int MaxHealth { get; init; }
    public required int Positioning {get;init;}
    public required int Awareness  {get;init;}
    public required int Marksmanship {get;init;}
    public required int Reaction {get;init;}
    public required int Resilience {get;init;}
    public required int Stealth {get;init;}
    public required int CloseCombat {get;init;}
    public required int Improvisation {get;init;}
}
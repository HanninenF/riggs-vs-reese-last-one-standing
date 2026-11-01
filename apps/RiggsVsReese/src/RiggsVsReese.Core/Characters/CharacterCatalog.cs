namespace RiggsVsReese.Core.Characters;

public static class CharacterCatalog
{
    public static IReadOnlyList <HumanDefinition> All {get;}=
    [
        new()
        {
            Id="martin-riggs",
            Name="Martin Riggs",
            MaxHealth=100,
            Positioning=90,
            Awareness=82,
            Marksmanship=96,
            Reaction=91,
            Resilience=88,
            Stealth=80,
            CloseCombat=90,
            Improvisation=80
        }
    ];
}
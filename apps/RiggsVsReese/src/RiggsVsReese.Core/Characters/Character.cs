namespace RiggsVsReese.Core.Characters;

public abstract class Character(CharacterDefinition definition)
{
    public CharacterDefinition Definition { get; } = definition;

    public int Health { get; private set; } = definition.MaxHealth;

    /* Lägg in metoder för skada och läkning här. */
}

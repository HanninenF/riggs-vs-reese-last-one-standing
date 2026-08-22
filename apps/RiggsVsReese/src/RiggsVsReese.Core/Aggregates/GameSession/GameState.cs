namespace RiggsVsReese.Core.Aggregates.GameSession;

public sealed class GameState
{
    public PlayerState Player { get; } = new();
}

public sealed class PlayerState
{
    public List<Item> Inventory { get; } = new();
}

public sealed record Item(string Id, string Name);

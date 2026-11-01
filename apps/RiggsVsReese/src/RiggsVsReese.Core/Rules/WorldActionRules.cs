using RiggsVsReese.Core.Aggregates.GameSession;

namespace RiggsVsReese.Core.Rules;

public static class WorldActionRules
{
    public static SearchResult InspectOrSearch(string? query)
    {
        return new SearchResult(
            query,
            "not_implemented",
            "Den riktiga spelmotorn är inte implementerad ännu. Objektet är därför okänt och får inte antas finnas.");
    }

    public static InteractionResult AttemptInteraction(
        string? action,
        string? target,
        GameState gameState)
    {
        var hasEquippedWeapon = gameState.Player.EquippedWeapon?.Definition.Name
            .Equals(target, StringComparison.OrdinalIgnoreCase) == true;
        var hasTargetInInventory = gameState.Player.Inventory.Any(item =>
            item.Definition.Name.Equals(target, StringComparison.OrdinalIgnoreCase));
        var hasTarget = hasEquippedWeapon || hasTargetInInventory;

        return new InteractionResult(
            action,
            target,
            hasTarget,
            hasTarget ? "allowed" : "not_in_inventory",
            hasTarget
                ? "Föremålet finns i spelarens inventory."
                : "Föremålet finns inte i spelarens inventory.");
    }
}

public sealed record SearchResult(string? Query, string Status, string Result);

public sealed record InteractionResult(
    string? Action,
    string? Target,
    bool Success,
    string Reason,
    string Result);

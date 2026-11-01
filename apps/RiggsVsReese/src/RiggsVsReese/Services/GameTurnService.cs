using RiggsVsReese.Core.Abstractions;
using RiggsVsReese.Core.Aggregates.GameSession;
using RiggsVsReese.Core.Rules;

namespace RiggsVsReese.Services;

internal sealed class GameTurnService
{
    private readonly GameState _gameState;
    private readonly ILanguageModelClient _languageModel;
    private readonly List<LanguageModelMessage> _messages =
    [
        new("system", "Du är spelledare i ett fritt textbaserat RPG. " +
                      "Svara på svenska och skriv levande men koncist. " +
                      "Detta är spelets första scen. Skriv endast en vanlig berättande scenbeskrivning. " +
                      "Använd inga verktyg och skriv inte JSON i detta första svar."),
        new("user", "Starta spelet med en provisorisk scen där Martin Riggs vaknar på en övergiven bensinstation.")
    ];

    private bool _firstRequest = true;

    internal GameTurnService(ILanguageModelClient languageModel, GameState gameState)
    {
        _languageModel = languageModel;
        _gameState = gameState;
    }

    internal Task<string?> StartAsync(CancellationToken cancellationToken = default)
    {
        return ContinueAsync(cancellationToken);
    }

    internal Task<string?> RespondAsync(
        string playerCommand,
        CancellationToken cancellationToken = default)
    {
        _messages.Add(new LanguageModelMessage("user", playerCommand));
        return ContinueAsync(cancellationToken);
    }

    private async Task<string?> ContinueAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var message = await _languageModel.SendAsync(
                BuildMessagesForRequest(),
                requireToolCall: !_firstRequest,
                cancellationToken);
            _firstRequest = false;

            if (!TryGetToolCalls(message, out var calls))
            {
                AddToolInstructionsAfterOpeningScene();
                return message.Content;
            }

            _messages.Add(message);
            var processing = ProcessToolCalls(calls);

            if (processing.ToolError || processing.NeedsContinuation)
                continue;

            return processing.NarrativeText;
        }
    }

    private static bool TryGetToolCalls(
        LanguageModelMessage message,
        out IReadOnlyList<LanguageModelToolCall> calls)
    {
        if (message.ToolCalls is { Count: > 0 } toolCalls)
        {
            calls = toolCalls;
            return true;
        }

        calls = [];
        return false;
    }

    private ToolProcessingResult ProcessToolCalls(
        IReadOnlyList<LanguageModelToolCall> calls)
    {
        var result = new ToolProcessingResult();

        foreach (var call in calls)
        {
            var callResult = ProcessToolCall(call);
            result = result.Combine(callResult);
        }

        return result;
    }

    private ToolProcessingResult ProcessToolCall(LanguageModelToolCall call)
    {
        return call.Name switch
        {
            LanguageModelToolNames.RespondNarratively => ProcessNarrativeCall(call),
            LanguageModelToolNames.InspectOrSearch => ProcessInspectionCall(call),
            LanguageModelToolNames.AttemptInteraction => ProcessInteractionCall(call),
            _ => new ToolProcessingResult()
        };
    }

    private ToolProcessingResult ProcessNarrativeCall(LanguageModelToolCall call)
    {
        if (call.Arguments.TryGetValue("text", out var text))
            return new ToolProcessingResult(NarrativeText: text);

        AddToolError(call.Name, "Ogiltiga argument: respond_narratively kräver fältet text.");
        return new ToolProcessingResult(ToolError: true);
    }

    private ToolProcessingResult ProcessInspectionCall(LanguageModelToolCall call)
    {
        if (!call.Arguments.TryGetValue("query", out var query))
        {
            AddToolError(call.Name, "Ogiltiga argument: inspect_or_search kräver fältet query.");
            return new ToolProcessingResult(ToolError: true);
        }

        var result = WorldActionRules.InspectOrSearch(query);
        AddToolResult(call.Name, result);
        return new ToolProcessingResult(NeedsContinuation: true);
    }

    private ToolProcessingResult ProcessInteractionCall(LanguageModelToolCall call)
    {
        if (!call.Arguments.TryGetValue("action", out var action) ||
            !call.Arguments.TryGetValue("target", out var target))
        {
            AddToolError(call.Name, "Ogiltiga argument: attempt_interaction kräver fälten action och target.");
            return new ToolProcessingResult(ToolError: true);
        }

        var result = WorldActionRules.AttemptInteraction(action, target, _gameState);
        AddToolResult(call.Name, result);
        return new ToolProcessingResult(NeedsContinuation: true);
    }

    private void AddToolResult(string toolName, object result)
    {
        _messages.Add(new LanguageModelMessage("tool", null, ToolName: toolName, ToolResult: result));
    }

    private sealed record ToolProcessingResult(
        bool ToolError = false,
        bool NeedsContinuation = false,
        string? NarrativeText = null)
    {
        internal ToolProcessingResult Combine(ToolProcessingResult other)
        {
            return new ToolProcessingResult(
                ToolError || other.ToolError,
                NeedsContinuation || other.NeedsContinuation,
                other.NarrativeText ?? NarrativeText);
        }
    }

    private void AddToolInstructionsAfterOpeningScene()
    {
        if (_messages.Count != 2)
            return;

        _messages.Add(new LanguageModelMessage(
            "system",
            "Från och med nästa spelartext måste du välja exakt ett verktyg. " +
            "Använd respond_narratively för samtal, tankar och fri berättelse. " +
            "Använd inspect_or_search när spelaren undersöker, söker efter eller frågar om något som inte är känt. " +
            "Använd attempt_interaction när spelaren försöker ta, använda, ladda eller påverka något. " +
            "Hitta aldrig på konkreta objekt eller fakta utan att först använda inspect_or_search."));
    }

    private IReadOnlyList<LanguageModelMessage> BuildMessagesForRequest()
    {
        var stateContext = new LanguageModelMessage("system", BuildGameStateContext());

        return _messages
            .Take(1)
            .Append(stateContext)
            .Concat(_messages.Skip(1))
            .ToArray();
    }

    private string BuildGameStateContext()
    {
        var weapon = _gameState.Player.EquippedWeapon;
        var chamberStatus = weapon?.HasRoundInChamber == true ? "ja" : "nej";
        var weaponDescription = weapon is null
            ? "Inget vapen är utrustat."
            : $"Utrustat vapen: {weapon.Definition.Name}; " +
              $"laddat magasin: {weapon.Magazine.Rounds}/{weapon.Magazine.Definition.Capacity} skott; " +
              $"patron i kammaren: {chamberStatus}; " +
              $"skick: {weapon.Condition}/100.";

        var inventoryDescription = _gameState.Player.Inventory.Count == 0
            ? "Inventory: tomt."
            : "Inventory: " + string.Join(
                ", ",
                _gameState.Player.Inventory.Select(magazine =>
                    $"{magazine.Definition.Name} ({magazine.Rounds}/{magazine.Definition.Capacity} skott)"));

        return "Aktuellt game state från spelmotorn. Detta är den auktoritativa informationen " +
               "om spelarens utrustning och inventory; hitta inte på andra värden. " +
               $"{weaponDescription} {inventoryDescription}";
    }

    private void AddToolError(string toolName, string error)
    {
        _messages.Add(new LanguageModelMessage("tool", error, ToolName: toolName));
    }
}
